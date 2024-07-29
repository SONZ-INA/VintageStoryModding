using System.Linq;

namespace FoodShelves;

public class BlockFruitBasket : BlockContainer {
    WorldInteraction[] interactions;

    public override void OnLoaded(ICoreAPI api) {
        base.OnLoaded(api);
        PlacedPriorityInteract = true; // Needed to call OnBlockInteractStart when shifting with an item in hand

        interactions = ObjectCacheUtil.GetOrCreate(api, "fruitbasketBlockInteractions", () => {
            List<ItemStack> fruitStackList = new();

            foreach(Item item in api.World.Items) {
                if (item.Code == null) continue;

                if (WildcardUtil.Match(FruitBasketCodes, item.Code.Path.ToString())) {
                    fruitStackList.Add(new ItemStack(item));
                }
            }

            return new WorldInteraction[] {
                new() {
                    ActionLangCode = "foodshelves:blockhelp-fruitbasket-add",
                    MouseButton = EnumMouseButton.Right,
                    HotKeyCode = "shift",
                    Itemstacks = fruitStackList.ToArray()
                },
                new() {
                    ActionLangCode = "foodshelves:blockhelp-fruitbasket-remove",
                    MouseButton = EnumMouseButton.Right,
                    HotKeyCode = "shift"
                }
            };
        });
    }

    public override WorldInteraction[] GetPlacedBlockInteractionHelp(IWorldAccessor world, BlockSelection selection, IPlayer forPlayer) {
        return interactions.Append(base.GetPlacedBlockInteractionHelp(world, selection, forPlayer));      
    }

    // Rotation logic
    public override bool DoPlaceBlock(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel, ItemStack byItemStack) {
        bool val = base.DoPlaceBlock(world, byPlayer, blockSel, byItemStack);
        SetBlockMeshAngle(world, byPlayer, blockSel, val);

        return val;
    }

    public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel) {
        if (byPlayer.Entity.Controls.Sneak) {
            if (world.BlockAccessor.GetBlockEntity(blockSel.Position) is BlockEntityFruitBasket frbasket) 
                return frbasket.OnInteract(byPlayer, blockSel);
        }

        return base.OnBlockInteractStart(world, byPlayer, blockSel);
    }

    public override void GetHeldItemInfo(ItemSlot inSlot, StringBuilder dsc, IWorldAccessor world, bool withDebugInfo) {
        base.GetHeldItemInfo(inSlot, dsc, world, withDebugInfo);

        dsc.Append(Lang.Get("foodshelves:Contents: "));
        GetBlockContent(inSlot, dsc, world);
    }

    #region InventoryMeshRender

    // Mesh rendering for items when inside inventory
    private string meshRefsCacheKey => Code.ToShortString() + "meshRefs";

    public override void OnBeforeRender(ICoreClientAPI capi, ItemStack itemstack, EnumItemRenderTarget target, ref ItemRenderInfo renderinfo) {
        Dictionary<int, MultiTextureMeshRef> meshrefs;

        if (capi.ObjectCache.TryGetValue(meshRefsCacheKey, out object obj)) {
            meshrefs = obj as Dictionary<int, MultiTextureMeshRef>;
        }
        else {
            capi.ObjectCache[meshRefsCacheKey] = meshrefs = new Dictionary<int, MultiTextureMeshRef>();
        }

        int hashcode = GetStackCacheHashCode(itemstack);

        if (!meshrefs.TryGetValue(hashcode, out MultiTextureMeshRef meshRef)) {
            ItemStack[] contents = GetContents(api.World, itemstack);
            MeshData meshdata = GenBlockWContentMesh(capi, this, contents);
            meshrefs[hashcode] = meshRef = capi.Render.UploadMultiTextureMesh(meshdata);
        }

        renderinfo.ModelRef = meshRef;
    }

    protected int GetStackCacheHashCode(ItemStack contentStack) {
        if (contentStack == null || contentStack.StackSize == 0 || contentStack.Collectible == null || contentStack.Collectible.Code == null) {
            return 0;
        }

        unchecked {
            // FNV-1 hash since any other simpler one ends up colliding, fuck data structures & algorithms btw
            const uint FNV_OFFSET_BASIS = 2166136261;
            const uint FNV_32_PRIME = 16777619;

            uint hash = FNV_OFFSET_BASIS;
            ItemStack[] contents = GetContents(api.World, contentStack);

            hash = (hash ^ (uint)contentStack.StackSize.GetHashCode()) * FNV_32_PRIME;

            for (int i = 0; i < contents.Length; i++) {
                if (contents[i] == null) continue;

                uint collectibleHash = (uint)(contents[i].Collectible != null ? contents[i].Collectible.Code.GetHashCode() : 0);
                hash = (hash ^ collectibleHash) * FNV_32_PRIME;
            }

            return (int)hash; 
        }
    }

    #endregion

    private void GetBlockContent(ItemSlot inSlot, StringBuilder dsc, IWorldAccessor world) {
        if (inSlot.Itemstack == null) {
            dsc.AppendLine("Empty.");
            return;
        }

        ItemStack[] contents = GetContents(world, inSlot.Itemstack);

        int fruitsCount = 0;
        int rotCount = 0;
        double totalFreshHours = 0;
        int itemCount = 0;
        ItemStack soonestPerishStack = null;
        double soonestPerishHours = double.MaxValue;
        float soonestTransitionLevel = 0;

        foreach (var stack in contents) {
            if (stack == null) continue;

            if (stack?.Collectible?.Code.Path.StartsWith("rot") == true) rotCount++;
            else fruitsCount++;

            ItemSlot tempSlot = new DummySlot(stack);
            TransitionState[] transitionStates = stack?.Collectible.UpdateAndGetTransitionStates(world, tempSlot);

            if (transitionStates != null && transitionStates.Length > 0) {
                foreach (var state in transitionStates) {
                    double freshHoursLeft = state.FreshHoursLeft / stack.Collectible.GetTransitionRateMul(world, tempSlot, state.Props.Type);
                    if (state.Props.Type == EnumTransitionType.Perish) {
                        totalFreshHours += freshHoursLeft * stack.StackSize;
                        itemCount += stack.StackSize;

                        if (freshHoursLeft < soonestPerishHours) {
                            soonestPerishHours = freshHoursLeft;
                            soonestPerishStack = stack;
                            soonestTransitionLevel = state.TransitionLevel;
                        }
                    }
                }
            }
        }

        // Number of fruits inside
        if (fruitsCount > 0) {
            dsc.AppendLine(Lang.Get("Fruits inside: {0}", fruitsCount));
        }

        // Number of rotten items
        if (rotCount > 0) {
            dsc.AppendLine(Lang.Get("Rotten fruits: {0}", rotCount));
        }

        // Average perish rate
        if (itemCount > 0) {
            double averageFreshHoursLeft = totalFreshHours / itemCount;
            double hoursPerday = world.Calendar.HoursPerDay;

            if (averageFreshHoursLeft / hoursPerday >= world.Calendar.DaysPerYear) {
                dsc.AppendLine(Lang.Get("Average perish rate: {0} years", Math.Round(averageFreshHoursLeft / hoursPerday / world.Calendar.DaysPerYear, 1)));
            }
            else if (averageFreshHoursLeft > hoursPerday) {
                dsc.AppendLine(Lang.Get("Average perish rate: {0} days", Math.Round(averageFreshHoursLeft / hoursPerday, 1)));
            }
            else {
                dsc.AppendLine(Lang.Get("Average perish rate: {0} hours", Math.Round(averageFreshHoursLeft, 1)));
            }
        }

        // Item that will perish the soonest
        if (soonestPerishStack != null) {
            dsc.Append(Lang.Get("Soonest: ") + soonestPerishStack.GetName());
            double hoursPerday = world.Calendar.HoursPerDay;

            if (soonestTransitionLevel > 0) {
                dsc.AppendLine(", " + Lang.Get("{0}% spoiled", (int)Math.Round(soonestTransitionLevel * 100)));
            }
            else {
                if (soonestPerishHours / hoursPerday >= world.Calendar.DaysPerYear) {
                    dsc.AppendLine(", " + Lang.Get("will perish in {0} years", Math.Round(soonestPerishHours / hoursPerday / world.Calendar.DaysPerYear, 1)));
                }
                else if (soonestPerishHours > hoursPerday) {
                    dsc.AppendLine(", " + Lang.Get("will perish in {0} days", Math.Round(soonestPerishHours / hoursPerday, 1)));
                }
                else {
                    dsc.AppendLine(", " + Lang.Get("will perish in {0} hours", Math.Round(soonestPerishHours, 1)));
                }
            }
        }
        else {
            dsc.AppendLine(Lang.Get("No items will perish soon."));
        }
    }

}

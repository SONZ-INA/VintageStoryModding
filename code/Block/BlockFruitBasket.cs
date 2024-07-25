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

        dsc.Append("<font color=\"orange\">Contents: </font>");
        GetBlockContent(inSlot, dsc, world);
    }

    public override WorldInteraction[] GetPlacedBlockInteractionHelp(IWorldAccessor world, BlockSelection selection, IPlayer forPlayer) {
        return interactions.Append(base.GetPlacedBlockInteractionHelp(world, selection, forPlayer));      
    }

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
            MeshData meshdata = GenMesh(capi, itemstack);
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

    public MeshData GenMesh(ICoreClientAPI capi, ItemStack contentStack) {
        // Block Region
        string shapePath = Shape?.Base?.ToString();
        string blockName = Code?.ToString();
        string modDomain = null;
        int colonIndex = shapePath.IndexOf(':');

        if (colonIndex != -1) {
            blockName = blockName.Substring(colonIndex + 1);
            modDomain = shapePath.Substring(0, colonIndex);
            shapePath = shapePath.Substring(colonIndex + 1);
        }
        else {
            api.Logger.Debug(modDomain + " - GenMesh: Indexing for shapePath failed.");
            return null;
        }

        string key = blockName + "Meshes" + Code.ToString();
        Dictionary<string, MeshData> meshes = ObjectCacheUtil.GetOrCreate(api, key, () => {
            return new Dictionary<string, MeshData>();
        });

        AssetLocation shapeLocation = new(modDomain + ":shapes/" + shapePath + ".json");

        Shape shape = api.Assets.TryGet(shapeLocation)?.ToObject<Shape>();
        if (shape == null) return null;

        ITexPositionSource texSource = capi.Tesselator.GetTextureSource(this);
        capi.Tesselator.TesselateBlock(this, out MeshData basketMesh); // Generate mesh data

        // Content Region
        if (contentStack != null) {
            ItemStack[] contents = GetContents(api.World, contentStack);

            if (contents != null) {
                for (int i = 0; i < contents.Length; i++) {
                    if (contents[i] != null) {
                        capi.Tesselator.TesselateItem(contents[i].Item, out MeshData contentData);

                        float[] x = { .65f, .3f, .3f,  .3f,  .6f, .35f,  .5f, .65f, .35f, .1f,  .6f, .58f, .3f,   .2f, -.1f,  .1f, .1f, .25f,  .2f, .55f,   .6f, .3f };
                        float[] y = {    0,   0,   0, .25f,    0, .35f,  .2f, -.3f,  .3f, .2f,  .4f,  .4f, .4f,   .5f, .57f, .05f, .3f, .52f, .55f, .45f, -.65f, .5f };
                        float[] z = { .05f,   0, .4f,  .1f, .45f, .35f, .18f,  .7f, .55f, .1f, .02f,  .3f, .7f, -.15f, .15f, -.2f, .9f, .05f,  .6f, .35f,  -.2f, .6f };

                        float[] rX = {  -2,   0,   0,   -3,   -3,   28,   16,   -2,   20,  30,  -20,    5, -75,    -8,   10,   85,   0,    8,   15,   -8,    90, -10 };
                        float[] rY = {   4,  -2,  15,   -4,   10,   12,   30,    3,   -2,   4,   -5,   -2,   2,    20,   55,    2,  50,   15,    0,    0,    22,  10 };
                        float[] rZ = {   1,  -1,   0,   45,    1,   41,    5,   70,   10,  17,   -2,  -20,   3,    16,    7,    6, -20,    8,  -25,   15,    45, -10 };

                        if (i < x.Length) {
                            float[] matrixTransform = 
                                new Matrixf()
                                .Translate(0.5f, 0, 0.5f)
                                .RotateXDeg(rX[i])
                                .RotateYDeg(rY[i])
                                .RotateZDeg(rZ[i])
                                .Scale(0.5f, 0.5f, 0.5f)
                                .Translate(x[i] - 0.84375f, y[i], z[i] - 0.8125f)
                                .Values;

                            contentData.MatrixTransform(matrixTransform);
                        }

                        basketMesh.AddMeshData(contentData);
                    }
                }
            }
        }

        return basketMesh;
    }

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

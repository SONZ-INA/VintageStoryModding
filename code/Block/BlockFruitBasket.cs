
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

    public override void OnBlockBroken(IWorldAccessor world, BlockPos pos, IPlayer byPlayer, float dropQuantityMultiplier = 1) {
        // Prevent duplicating of items inside
        if (world.BlockAccessor.GetBlockEntity(pos) is BlockEntityFruitBasket frbasket) {
            ItemStack[] contents = frbasket.GetContentStacks();
            ItemStack emptyFruitBasket = new(this);
            world.SpawnItemEntity(emptyFruitBasket, pos.ToVec3d().Add(0.5, 0.5, 0.5));
            for (int i = 0; i < contents.Length; i++) {
                world.SpawnItemEntity(contents[i], pos.ToVec3d().Add(0.5, 0.5, 0.5));
            }

            world.BlockAccessor.SetBlock(0, pos);
        }
    }

    // Rotation logic
    public override bool DoPlaceBlock(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel, ItemStack byItemStack) {
        bool val = base.DoPlaceBlock(world, byPlayer, blockSel, byItemStack);
        SetBlockMeshAngle(world, byPlayer, blockSel, val);

        return val;
    }

    public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel) {
        if (byPlayer.Entity.Controls.ShiftKey) {
            if (world.BlockAccessor.GetBlockEntity(blockSel.Position) is BlockEntityFruitBasket frbasket) 
                return frbasket.OnInteract(byPlayer, blockSel);
        }

        return base.OnBlockInteractStart(world, byPlayer, blockSel);
    }

    public override void GetHeldItemInfo(ItemSlot inSlot, StringBuilder dsc, IWorldAccessor world, bool withDebugInfo) {
        base.GetHeldItemInfo(inSlot, dsc, world, withDebugInfo);

        dsc.Append(Lang.Get("Contents: "));
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
            if (meshdata != null) { 
                meshrefs[hashcode] = meshRef = capi.Render.UploadMultiTextureMesh(meshdata);
            }
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
            dsc.AppendLine(Lang.Get("Empty."));
            return;
        }

        ItemStack[] contents = GetContents(world, inSlot.Itemstack);
        PerishableInfoAverageAndSoonest(contents, dsc, world);
    }
}

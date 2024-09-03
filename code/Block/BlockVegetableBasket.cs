namespace FoodShelves;

public class BlockVegetableBasket : BlockContainer {
    WorldInteraction[] interactions;

    public override void OnLoaded(ICoreAPI api) {
        base.OnLoaded(api);
        PlacedPriorityInteract = true; // Needed to call OnBlockInteractStart when shifting with an item in hand

        interactions = ObjectCacheUtil.GetOrCreate(api, "vegetablebasketBlockInteractions", () => {
            List<ItemStack> vegetableStackList = new();

            foreach(Item item in api.World.Items) {
                if (item.Code == null) continue;

                if (WildcardUtil.Match(VegetableBasketData.VegetableBasketCodes, item.Code.Path.ToString())) {
                    vegetableStackList.Add(new ItemStack(item));
                }
            }

            return new WorldInteraction[] {
                new() {
                    ActionLangCode = "foodshelves:blockhelp-fruitbasket-add",
                    MouseButton = EnumMouseButton.Right,
                    HotKeyCode = "shift",
                    Itemstacks = vegetableStackList.ToArray()
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
        if (world.BlockAccessor.GetBlockEntity(pos) is BlockEntityVegetableBasket vgbasket) {
            ItemStack[] contents = vgbasket.GetContentStacks();
            ItemStack emptyVegetableBasket = new(this);
            world.SpawnItemEntity(emptyVegetableBasket, pos.ToVec3d().Add(0.5, 0.5, 0.5));
            for (int i = 0; i < contents.Length; i++) {
                world.SpawnItemEntity(contents[i], pos.ToVec3d().Add(0.5, 0.5, 0.5));
            }

            world.BlockAccessor.SetBlock(0, pos);
        }
    }

    // Rotation logic
    public override bool DoPlaceBlock(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel, ItemStack byItemStack) {
        bool val = base.DoPlaceBlock(world, byPlayer, blockSel, byItemStack);
        BlockEntityVegetableBasket block = world.BlockAccessor.GetBlockEntity(blockSel.Position) as BlockEntityVegetableBasket;
        block.MeshAngle = GetBlockMeshAngle(byPlayer, blockSel, val);

        return val;
    }

    public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel) {
        if (byPlayer.Entity.Controls.ShiftKey) {
            if (world.BlockAccessor.GetBlockEntity(blockSel.Position) is BlockEntityVegetableBasket frbasket) 
                return frbasket.OnInteract(byPlayer, blockSel);
        }

        return base.OnBlockInteractStart(world, byPlayer, blockSel);
    }

    public override void GetHeldItemInfo(ItemSlot inSlot, StringBuilder dsc, IWorldAccessor world, bool withDebugInfo) {
        base.GetHeldItemInfo(inSlot, dsc, world, withDebugInfo);

        dsc.Append(Lang.Get("Contents: "));
        GetBlockContent(inSlot, dsc, world);
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

        ItemStack[] contents = GetContents(api.World, itemstack);
        int hashcode = GetStackCacheHashCodeFNV(contents);

        if (!meshrefs.TryGetValue(hashcode, out MultiTextureMeshRef meshRef)) {
            MeshData meshdata = GenBlockWContentMesh(capi, this, contents);
            if (meshdata != null) { 
                meshrefs[hashcode] = meshRef = capi.Render.UploadMultiTextureMesh(meshdata);
            }
        }

        renderinfo.ModelRef = meshRef;
    }

    private void GetBlockContent(ItemSlot inSlot, StringBuilder dsc, IWorldAccessor world) {
        if (inSlot.Itemstack == null) {
            dsc.AppendLine(Lang.Get("Empty."));
            return;
        }

        ItemStack[] contents = GetContents(world, inSlot.Itemstack);
        PerishableInfoAverageAndSoonest(contents, dsc, world);
    }
}

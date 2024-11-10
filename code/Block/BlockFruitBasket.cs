namespace FoodShelves;

public class BlockFruitBasket : BlockContainer {
    WorldInteraction[] interactions;

    public override void OnLoaded(ICoreAPI api) {
        base.OnLoaded(api);
        PlacedPriorityInteract = true; // Needed to call OnBlockInteractStart when shifting with an item in hand

        interactions = ObjectCacheUtil.GetOrCreate(api, "basketBlockInteractions", () => {
            List<ItemStack> fruitStackList = new();

            foreach(Item item in api.World.Items) {
                if (item.Code == null) continue;

                if (WildcardUtil.Match(FruitBasketData.FruitBasketCodes, item.Code.Path.ToString())) {
                    fruitStackList.Add(new ItemStack(item));
                }
            }

            return new WorldInteraction[] {
                new() {
                    ActionLangCode = "blockhelp-groundstorage-add",
                    MouseButton = EnumMouseButton.Right,
                    HotKeyCode = "shift",
                    Itemstacks = fruitStackList.ToArray()
                },
                new() {
                    ActionLangCode = "blockhelp-groundstorage-remove",
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
        if (byPlayer.WorldData.CurrentGameMode == EnumGameMode.Survival) {
            if (world.BlockAccessor.GetBlockEntity(pos) is BlockEntityFruitBasket frbasket) {
                ItemStack[] contents = frbasket.GetContentStacks();
                ItemStack emptyFruitBasket = new(this);
                world.SpawnItemEntity(emptyFruitBasket, pos.ToVec3d().Add(0.5, 0.5, 0.5));
                for (int i = 0; i < contents.Length; i++) {
                    world.SpawnItemEntity(contents[i], pos.ToVec3d().Add(0.5, 0.5, 0.5));
                }
            }
        }

        world.BlockAccessor.SetBlock(0, pos);
    }

    public override string GetHeldItemName(ItemStack itemStack) {
        string variantName = itemStack.GetMaterialNameLocalized();
        return base.GetHeldItemName(itemStack) + variantName;
    }

    // Rotation logic
    public override bool DoPlaceBlock(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel, ItemStack byItemStack) {
        bool val = base.DoPlaceBlock(world, byPlayer, blockSel, byItemStack);
        BlockEntityFruitBasket block = world.BlockAccessor.GetBlockEntity(blockSel.Position) as BlockEntityFruitBasket;
        block.MeshAngle = GetBlockMeshAngle(byPlayer, blockSel, val);

        return val;
    }

    public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel) {
        if (byPlayer.Entity.Controls.ShiftKey) {
            if (world.BlockAccessor.GetBlockEntity(blockSel.Position) is BlockEntityFruitBasket frbasket) 
                return frbasket.OnInteract(byPlayer);
        }

        return base.OnBlockInteractStart(world, byPlayer, blockSel);
    }

    public override void GetHeldItemInfo(ItemSlot inSlot, StringBuilder dsc, IWorldAccessor world, bool withDebugInfo) {
        base.GetHeldItemInfo(inSlot, dsc, world, withDebugInfo);

        dsc.Append(Lang.Get("foodshelves:Contents"));

        if (inSlot.Itemstack == null) {
            dsc.AppendLine(Lang.Get("foodshelves:Empty."));
            return;
        }

        ItemStack[] contents = GetContents(world, inSlot.Itemstack);
        PerishableInfoAverageAndSoonest(contents.ToDummySlots(), dsc, world);
    }

    // Mesh rendering for items when inside inventory
    private string MeshRefsCacheKey => this.Code.ToShortString() + "meshRefs";
    public override void OnBeforeRender(ICoreClientAPI capi, ItemStack itemstack, EnumItemRenderTarget target, ref ItemRenderInfo renderinfo) {
        Dictionary<int, MultiTextureMeshRef> meshrefs;

        if (capi.ObjectCache.TryGetValue(MeshRefsCacheKey, out object obj)) {
            meshrefs = obj as Dictionary<int, MultiTextureMeshRef>;
        }
        else {
            capi.ObjectCache[MeshRefsCacheKey] = meshrefs = new Dictionary<int, MultiTextureMeshRef>();
        }

        ItemStack[] contents = GetContents(api.World, itemstack);
        int hashcode = GetStackCacheHashCodeFNV(contents);

        if (!meshrefs.TryGetValue(hashcode, out MultiTextureMeshRef meshRef)) {
            GetTransformationMatrix(out float[,] transformationMatrix);

            capi.Tesselator.TesselateBlock(this, out MeshData basketMesh);
            MeshData contentMesh = GenContentMesh(capi, contents, transformationMatrix, 0.5f, FruitBasketTransformations);
            if (contentMesh != null) basketMesh.AddMeshData(contentMesh);

            if (basketMesh != null) { 
                meshrefs[hashcode] = meshRef = capi.Render.UploadMultiTextureMesh(basketMesh);
            }
        }

        renderinfo.ModelRef = meshRef;
    }

    public static void GetTransformationMatrix(out float[,] transformationMatrix) {
        float[] x = { .65f, .3f, .3f,  .3f,  .6f, .35f,  .5f, .65f, .35f, .1f,  .6f, .58f, .3f,   .2f, -.1f,  .1f, .1f, .25f,  .2f, .55f,   .6f, .3f };
        float[] y = {    0,   0,   0, .25f,    0, .35f,  .2f, -.3f,  .3f, .2f,  .4f,  .4f, .4f,   .5f, .57f, .05f, .3f, .52f, .55f, .45f, -.65f, .5f };
        float[] z = { .05f,   0, .4f,  .1f, .45f, .35f, .18f,  .7f, .55f, .1f, .02f,  .3f, .7f, -.15f, .15f, -.2f, .9f, .05f,  .6f, .35f,  -.2f, .6f };

        float[] rX = {  -2,   0,   0,   -3,   -3,   28,   16,   -2,   20,  30,  -20,    5, -75,    -8,   10,   85,   0,    8,   15,   -8,    90, -10 };
        float[] rY = {   4,  -2,  15,   -4,   10,   12,   30,    3,   -2,   4,   -5,   -2,   2,    20,   55,    2,  50,   15,    0,    0,    22,  10 };
        float[] rZ = {   1,  -1,   0,   45,    1,   41,    5,   70,   10,  17,   -2,  -20,   3,    16,    7,    6, -20,    8,  -25,   15,    45, -10 };

        transformationMatrix = GenTransformationMatrix(x, y, z, rX, rY, rZ);
    }
}

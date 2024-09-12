namespace FoodShelves;

public class BlockEntityFruitBasket : BlockEntityDisplay {
    readonly InventoryGeneric inv;
    BlockFruitBasket block;
    public float MeshAngle { get; set; }

    public override InventoryBase Inventory => inv;
    public override string InventoryClassName => Block?.Attributes?["inventoryClassName"].AsString();
    public override string AttributeTransformCode => Block?.Attributes?["attributeTransformCode"].AsString();

    private const int shelfCount = 1;
    private const int segmentsPerShelf = 1;
    private const int itemsPerSegment = 22;
    static readonly int slotCount = shelfCount * segmentsPerShelf * itemsPerSegment;
    private readonly InfoDisplayOptions displaySelection = InfoDisplayOptions.ByBlockAverageAndSoonest;

    public BlockEntityFruitBasket() { inv = new InventoryGeneric(slotCount, InventoryClassName + "-0", Api, (_, inv) => new ItemSlotFruitBasket(inv)); }

    public override void Initialize(ICoreAPI api) {
        block = api.World.BlockAccessor.GetBlock(Pos) as BlockFruitBasket;
        base.Initialize(api);
    }

    internal bool OnInteract(IPlayer byPlayer, BlockSelection blockSel) {
        ItemSlot slot = byPlayer.InventoryManager.ActiveHotbarSlot;

        if (slot.Empty) {
            return TryTake(byPlayer, blockSel);
        }
        else {
            if (slot.FruitBasketCheck()) {
                AssetLocation sound = slot.Itemstack?.Block?.Sounds?.Place;

                if (TryPut(slot, blockSel)) {
                    Api.World.PlaySoundAt(sound ?? new AssetLocation("sounds/player/build"), byPlayer.Entity, byPlayer, true, 16);
                    MarkDirty();
                    return true;
                }
            }
            else {
                (Api as ICoreClientAPI)?.TriggerIngameError(this, "cantplace", Lang.Get("foodshelves:Only fruit can be placed in this basket."));
            }

            return false;
        }
    }

    private bool TryPut(ItemSlot slot, BlockSelection blockSel) {
        if (blockSel.SelectionBoxIndex != 0) return false;

        for (int i = 0; i < itemsPerSegment; i++) {
            if (inv[i].Empty) {
                int moved = slot.TryPutInto(Api.World, inv[i]);
                MarkDirty();
                (Api as ICoreClientAPI)?.World.Player.TriggerFpAnimation(EnumHandInteract.HeldItemInteract);
                return moved > 0;
            }
        }

        return false;
    }

    private bool TryTake(IPlayer byPlayer, BlockSelection blockSel) {
        if (blockSel.SelectionBoxIndex != 0) return false;

        for (int i = itemsPerSegment - 1; i >= 0; i--) {
            if (!inv[i].Empty) {
                ItemStack stack = inv[i].TakeOut(1);
                if (byPlayer.InventoryManager.TryGiveItemstack(stack)) {
                    AssetLocation sound = stack.Block?.Sounds?.Place;
                    Api.World.PlaySoundAt(sound ?? new AssetLocation("sounds/player/build"), byPlayer.Entity, byPlayer, true, 16);
                }

                if (stack.StackSize > 0) {
                    Api.World.SpawnItemEntity(stack, Pos.ToVec3d().Add(0.5, 0.5, 0.5));
                }

                (Api as ICoreClientAPI)?.World.Player.TriggerFpAnimation(EnumHandInteract.HeldItemInteract);
                MarkDirty();
                return true;
            }
        }

        return false;
    }

    protected override float[][] genTransformationMatrices() {
        BlockFruitBasket.GetTransformationMatrix(out float[,] transformationMatrix);
        float[][] tfMatrices = new float[slotCount][];

        for (int shelf = 0; shelf < shelfCount; shelf++) {
            for (int segment = 0; segment < segmentsPerShelf; segment++) {
                for (int item = 0; item < itemsPerSegment; item++) {
                    int index = shelf * (segmentsPerShelf * itemsPerSegment) + segment * itemsPerSegment + item;

                    tfMatrices[index] = 
                        new Matrixf()
                        .Translate(0.5f, 0, 0.5f)
                        .RotateYDeg(block.Shape.rotateY + MeshAngle * GameMath.RAD2DEG)
                        .RotateXDeg(transformationMatrix[3, index])
                        .RotateYDeg(transformationMatrix[4, index])
                        .RotateZDeg(transformationMatrix[5, index])
                        .Scale(0.5f, 0.5f, 0.5f)
                        .Translate(transformationMatrix[0, index] - 0.84375f, transformationMatrix[1, index], transformationMatrix[2, index] - 0.8125f)
                        .Values;
                }
            }
        }

        return tfMatrices;
    }

    #region RotationRender

    public override bool OnTesselation(ITerrainMeshPool mesher, ITesselatorAPI tesselator) {
        bool skipmesh = base.OnTesselation(mesher, tesselator);

        if (!skipmesh) {
            MeshData meshData = GenBlockMesh(Api, this, tesselator);
            if (meshData == null) return false;

            mesher.AddMeshData(meshData.Clone().Rotate(new Vec3f(0.5f, 0.5f, 0.5f), 0, MeshAngle, 0));
        }

        return true;
    }

    public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldForResolving) {
        base.FromTreeAttributes(tree, worldForResolving);
        MeshAngle = tree.GetFloat("meshAngle", 0f);
        RedrawAfterReceivingTreeAttributes(worldForResolving);
    }

    public override void ToTreeAttributes(ITreeAttribute tree) {
        base.ToTreeAttributes(tree);
        tree.SetFloat("meshAngle", MeshAngle);
    }

    #endregion

    public override void GetBlockInfo(IPlayer forPlayer, StringBuilder sb) {
        base.GetBlockInfo(forPlayer, sb);
        DisplayInfo(forPlayer, sb, inv, displaySelection, slotCount, segmentsPerShelf, itemsPerSegment, "fruit");
    }
}

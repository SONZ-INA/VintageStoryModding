namespace FoodShelves;

public class BlockEntityFruitBasket : BlockEntityDisplay {
    readonly InventoryGeneric inv;
    BlockFruitBasket block;
    public float MeshAngle { get; set; }

    public override InventoryBase Inventory => inv;
    public override string InventoryClassName => Block?.Attributes?["inventoryClassName"].AsString();
    public override string AttributeTransformCode => Block?.Attributes?["attributeTransformCode"].AsString();

    static readonly int slotCount = 22;
    private readonly InfoDisplayOptions displaySelection = InfoDisplayOptions.ByBlockAverageAndSoonest;

    public BlockEntityFruitBasket() { inv = new InventoryGeneric(slotCount, InventoryClassName + "-0", Api, (_, inv) => new ItemSlotFruitBasket(inv)); }

    public override void Initialize(ICoreAPI api) {
        block = api.World.BlockAccessor.GetBlock(Pos) as BlockFruitBasket;
        base.Initialize(api);
    }

    internal bool OnInteract(IPlayer byPlayer) {
        ItemSlot slot = byPlayer.InventoryManager.ActiveHotbarSlot;

        if (slot.Empty) {
            return TryTake(byPlayer);
        }
        else {
            if (slot.FruitBasketCheck()) {
                AssetLocation sound = slot.Itemstack?.Block?.Sounds?.Place;

                if (TryPut(slot)) {
                    Api.World.PlaySoundAt(sound ?? new AssetLocation("sounds/player/build"), byPlayer.Entity, byPlayer, true, 16);
                    MarkDirty();
                    return true;
                }
            }

            (Api as ICoreClientAPI)?.TriggerIngameError(this, "cantplace", Lang.Get("foodshelves:Only fruit can be placed in this basket."));
            return false;
        }
    }

    private bool TryPut(ItemSlot slot) {
        for (int i = 0; i < slotCount; i++) {
            if (inv[i].Empty) {
                int moved = slot.TryPutInto(Api.World, inv[i]);
                MarkDirty();
                (Api as ICoreClientAPI)?.World.Player.TriggerFpAnimation(EnumHandInteract.HeldItemInteract);
                return moved > 0;
            }
        }

        return false;
    }

    private bool TryTake(IPlayer byPlayer) {
        for (int i = slotCount - 1; i >= 0; i--) {
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

        for (int item = 0; item < slotCount; item++) {
            tfMatrices[item] = 
                new Matrixf()
                .Translate(0.5f, 0, 0.5f)
                .RotateYDeg((block != null ? block.Shape.rotateY : 0) + MeshAngle * GameMath.RAD2DEG)
                .RotateXDeg(transformationMatrix[3, item])
                .RotateYDeg(transformationMatrix[4, item])
                .RotateZDeg(transformationMatrix[5, item])
                .Scale(0.5f, 0.5f, 0.5f)
                .Translate(transformationMatrix[0, item] - 0.84375f, transformationMatrix[1, item], transformationMatrix[2, item] - 0.8125f)
                .Values;
        }

        return tfMatrices;
    }

    #region RotationRender

    public override bool OnTesselation(ITerrainMeshPool mesher, ITesselatorAPI tesselator) {
        bool skipmesh = base.OnTesselation(mesher, tesselator);

        if (!skipmesh) {
            tesselator.TesselateBlock(Api.World.BlockAccessor.GetBlock(this.Pos), out MeshData blockMesh);
            if (blockMesh == null) return false;

            mesher.AddMeshData(blockMesh.Clone().Rotate(new Vec3f(0.5f, 0.5f, 0.5f), 0, MeshAngle, 0));
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
        DisplayInfo(forPlayer, sb, inv, displaySelection, slotCount, 0, 0, "fruit");
    }
}

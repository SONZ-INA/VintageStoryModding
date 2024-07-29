namespace FoodShelves;

public class BlockEntityPieShelf : BlockEntityDisplay {
    readonly InventoryGeneric inv;
    Block block;
    
    public override InventoryBase Inventory => inv;
    public override string InventoryClassName => Block?.Attributes?["inventoryClassName"].AsString();
    public override string AttributeTransformCode => Block?.Attributes?["attributeTransformCode"].AsString();

    private const int shelfCount = 3;
    private const int segmentsPerShelf = 1;
    private const int itemsPerSegment = 1;
    static readonly int slotCount = shelfCount * segmentsPerShelf * itemsPerSegment;
    private readonly InfoDisplayOptions displaySelection = InfoDisplayOptions.ByBlock;

    public BlockEntityPieShelf() { inv = new InventoryGeneric(slotCount, InventoryClassName + "-0", Api, (_, inv) => new ItemSlotPieShelf(inv)); }

    public override void Initialize(ICoreAPI api) {
        block = api.World.BlockAccessor.GetBlock(Pos);
        base.Initialize(api);
    }

    internal bool OnInteract(IPlayer byPlayer, BlockSelection blockSel) {
        ItemSlot slot = byPlayer.InventoryManager.ActiveHotbarSlot;

        if (slot.Empty) {
            if (TryTake(byPlayer, blockSel)) return true;
            else return false;
        }
        else {
            if (slot.PieShelfCheck()) {
                AssetLocation sound = slot.Itemstack?.Block?.Sounds?.Place;

                if (TryPut(slot, blockSel)) {
                    Api.World.PlaySoundAt(sound ?? new AssetLocation("sounds/player/build"), byPlayer.Entity, byPlayer, true, 16);
                    MarkDirty();
                    return true;
                }
            }
            else {
                (Api as ICoreClientAPI).TriggerIngameError(this, "cantplace", Lang.Get("foodshelves:Only pies or cheese can be placed on this shelf."));
            }

            return false;
        }
    }

    private bool TryPut(ItemSlot slot, BlockSelection blockSel) {
        int index = blockSel.SelectionBoxIndex;
        if (index < 0 || index >= slotCount) return false;

        if (inv[index].Empty) {
            int moved = slot.TryPutInto(Api.World, inv[index]);
            MarkDirty();
            (Api as ICoreClientAPI)?.World.Player.TriggerFpAnimation(EnumHandInteract.HeldItemInteract);

            return moved > 0;
        }

        return false;
    }

    private bool TryTake(IPlayer byPlayer, BlockSelection blockSel) {
        int index = blockSel.SelectionBoxIndex;
        if (index < 0 || index >= slotCount) return false;

        if (!inv[index].Empty) {
            ItemStack stack = inv[index].TakeOut(1);
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

        return false;
    }

    protected override float[][] genTransformationMatrices() {
        float[][] tfMatrices = new float[slotCount][];

        for (int index = 0; index < slotCount; index++) {
            float x = 0f;
            float y = index * 0.3f;
            float z = 0f;

            tfMatrices[index] =
                new Matrixf()
                .Translate(0.5f, 0, 0.5f)
                .RotateYDeg(block.Shape.rotateY)
                .Translate(x - 0.5f, y + 0.06f, z - 0.5f)
                .Values;
        }

        return tfMatrices;
    }

    public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldForResolving) {
        base.FromTreeAttributes(tree, worldForResolving);
        RedrawAfterReceivingTreeAttributes(worldForResolving);
    }

    public override void GetBlockInfo(IPlayer forPlayer, StringBuilder sb) {
        base.GetBlockInfo(forPlayer, sb);
        DisplayInfo(forPlayer, sb, inv, displaySelection, slotCount, segmentsPerShelf, itemsPerSegment);
    }
}

namespace FoodShelves;

public class BlockEntityTableWShelf : BlockEntityDisplay {
    InventoryGeneric inv;
    Block block;
    
    public override InventoryBase Inventory => inv;
    public override string InventoryClassName => Block?.Attributes?["inventoryClassName"].AsString();
    public override string AttributeTransformCode => onDisplayTransform;

    private const int shelfCount = 1;
    private const int segmentsPerShelf = 1;
    private const int itemsPerSegment = 2;
    static readonly int slotCount = shelfCount * segmentsPerShelf * itemsPerSegment;
    private readonly InfoDisplayOptions displaySelection = InfoDisplayOptions.ByBlock;

    public BlockEntityTableWShelf() {
        inv = new InventoryGeneric(slotCount, Block?.Attributes?["inventoryClassName"].AsString() + "-0", Api, (_, inv) => new ItemSlotTableWShelf(inv));
    }

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
            if (slot.ShelvableCheck()) {
                AssetLocation sound = slot.Itemstack?.Block?.Sounds?.Place;

                if (TryPut(slot, blockSel)) {
                    Api.World.PlaySoundAt(sound ?? new AssetLocation("sounds/player/build"), byPlayer.Entity, byPlayer, true, 16);
                    MarkDirty();
                    return true;
                }
            }

            return false;
        }
    }

    private bool TryPut(ItemSlot slot, BlockSelection blockSel) {
        int segmentIndex = blockSel.SelectionBoxIndex;
        if (segmentIndex != 1) return false;

        int startIndex = 0;

        for (int i = 0; i < itemsPerSegment; i++) {
            int currentIndex = startIndex + i;
            if (inv[currentIndex].Empty) {
                int moved = slot.TryPutInto(Api.World, inv[currentIndex]);
                MarkDirty();
                (Api as ICoreClientAPI)?.World.Player.TriggerFpAnimation(EnumHandInteract.HeldItemInteract);
                return moved > 0;
            }
        }

        return false;
    }

    private bool TryTake(IPlayer byPlayer, BlockSelection blockSel) {
        int segmentIndex = blockSel.SelectionBoxIndex;
        if (segmentIndex != 1) return false;

        int startIndex = 0;

        for (int i = itemsPerSegment - 1; i >= 0; i--) {
            int currentIndex = startIndex + i;
            if (!inv[currentIndex].Empty) {
                ItemStack stack = inv[currentIndex].TakeOut(1);
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
        float[][] tfMatrices = new float[slotCount][];

        for (int index = 0; index < slotCount; index++) {
            float x = 0f;
            float y = 0.3f;
            float z = index * 0.5f;
            float scaleValue = 1f;

            // Using vanilla shelf transformations, the pot is too big so need to adjust it
            ItemSlot slot = inv[index];
            bool? scaleCheck = slot?.Itemstack?.Collectible?.Code.Path.StartsWith("claypot-");
            if (scaleCheck == true) scaleValue = 0.85f;

            tfMatrices[index] =
                new Matrixf()
                .Translate(0.5f, 0, 0.5f)
                .RotateYDeg(block.Shape.rotateY)
                .Scale(scaleValue, scaleValue, scaleValue)
                .Translate(x - 0.5f, y + 0.06f, z - 0.75f)
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
        DisplayInfo(forPlayer, sb, inv, Api, displaySelection, slotCount, segmentsPerShelf, itemsPerSegment);
    }
}

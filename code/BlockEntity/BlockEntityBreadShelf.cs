namespace FoodShelves;

public class BlockEntityBreadShelf : BlockEntityDisplay {
    readonly InventoryGeneric inv;
    Block block;

    public override InventoryBase Inventory => inv;
    public override string InventoryClassName => Block?.Attributes?["inventoryClassName"].AsString();
    public override string AttributeTransformCode => Block?.Attributes?["attributeTransformCode"].AsString();

    private const int shelfCount = 4;
    private const int segmentsPerShelf = 3;
    private const int itemsPerSegment = 2;
    static readonly int slotCount = shelfCount * segmentsPerShelf * itemsPerSegment;
    private readonly InfoDisplayOptions displaySelection = InfoDisplayOptions.ByShelf;

    public BlockEntityBreadShelf() { inv = new InventoryGeneric(slotCount, InventoryClassName + "-0", Api, (_, inv) => new ItemSlotBreadShelf(inv)); }

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
            if (slot.BreadShelfCheck()) {
                AssetLocation sound = slot.Itemstack?.Block?.Sounds?.Place;

                if (TryPut(slot, blockSel)) {
                    Api.World.PlaySoundAt(sound ?? new AssetLocation("sounds/player/build"), byPlayer.Entity, byPlayer, true, 16);
                    MarkDirty();
                    return true;
                }
            }
            else {
                (Api as ICoreClientAPI).TriggerIngameError(this, "cantplace", Lang.Get("foodshelves:Only bread, muffins, dumplings, pacoca, halva or marzipam can be placed on this shelf."));
            }

            return false;
        }
    }

    private bool TryPut(ItemSlot slot, BlockSelection blockSel) {
        int segmentIndex = blockSel.SelectionBoxIndex;
        if (segmentIndex < 0 || segmentIndex >= slotCount / itemsPerSegment) return false;

        int startIndex = segmentIndex * itemsPerSegment;

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
        if (segmentIndex < 0 || segmentIndex >= slotCount / itemsPerSegment) return false;

        int startIndex = segmentIndex * itemsPerSegment;

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

        for (int shelf = 0; shelf < shelfCount; shelf++) {
            for (int segment = 0; segment < segmentsPerShelf; segment++) {
                for (int item = 0; item < itemsPerSegment; item++) {
                    int index = shelf * (segmentsPerShelf * itemsPerSegment) + segment * itemsPerSegment + item;

                    float x = segment * 0.29f;
                    float y = shelf * 0.25f;
                    float z = item * 0.4f;

                    tfMatrices[index] =
                        new Matrixf()
                        .Translate(0.5f, 0, 0.5f)
                        .RotateYDeg(block.Shape.rotateY)
                        .Translate(x - 0.79f, y + 0.06f, z - 0.75f)
                        .Values;
                }
            }
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
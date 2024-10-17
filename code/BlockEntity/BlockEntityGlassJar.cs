namespace FoodShelves;

public class BlockEntityGlassJar : BlockEntityDisplay {
    private InventoryGeneric inv;
    private Block block;
    
    public override InventoryBase Inventory => inv;
    public override string InventoryClassName => Block?.Attributes?["inventoryClassName"].AsString();
    public override string AttributeTransformCode => Block?.Attributes?["attributeTransformCode"].AsString();

    private const int slotCount = 1;
    private readonly InfoDisplayOptions displaySelection = InfoDisplayOptions.BySegment;

    public BlockEntityGlassJar() { inv = new InventoryGeneric(slotCount, InventoryClassName + "-0", Api, (_, inv) => new ItemSlotLiquidyStuff(inv)); }

    public override void Initialize(ICoreAPI api) {
        block = api.World.BlockAccessor.GetBlock(Pos);
        base.Initialize(api);
    }

    // Check this method for dehydrated fruit -> dry fruit
    protected override float Inventory_OnAcquireTransitionSpeed(EnumTransitionType transType, ItemStack stack, float baseMul) {
        if (transType == EnumTransitionType.Dry || transType == EnumTransitionType.Melt) return room?.ExitCount == 0 ? 2f : 0.5f;
        if (Api == null) return 0;

        if (transType == EnumTransitionType.Perish || transType == EnumTransitionType.Ripen) {
            float perishRate = GetPerishRate();
            if (transType == EnumTransitionType.Ripen) {
                return GameMath.Clamp((1 - perishRate - 0.5f) * 3, 0, 1);
            }

            return baseMul * perishRate;
        }

        return 1;
    }

    internal bool OnInteract(IPlayer byPlayer) {
        ItemSlot slot = byPlayer.InventoryManager.ActiveHotbarSlot;

        if (slot.Empty) {
            return TryTake(byPlayer);
        }
        else {
            if (slot.LiquidyStuffCheck()) {
                AssetLocation sound = slot.Itemstack?.Block?.Sounds?.Place;

                if (TryPut(slot)) {
                    Api.World.PlaySoundAt(sound ?? new AssetLocation("sounds/player/build"), byPlayer.Entity, byPlayer, true, 16);
                    MarkDirty();
                    return true;
                }
            }

            return false;
        }
    }

    private bool TryPut(ItemSlot slot) {
        if (inv[0].Empty) {
            int moved = slot.TryPutInto(Api.World, inv[0]);
            MarkDirty();
            (Api as ICoreClientAPI)?.World.Player.TriggerFpAnimation(EnumHandInteract.HeldItemInteract);
            return moved > 0;
        }

        return false;
    }

    private bool TryTake(IPlayer byPlayer) {
        if (!inv[0].Empty) {
            ItemStack stack = inv[0].TakeOut(1);
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

    public override bool OnTesselation(ITerrainMeshPool mesher, ITesselatorAPI tesselator) {
        bool skipmesh = base.OnTesselation(mesher, tesselator);

        if (!skipmesh) {
            // todo - make a meshing method for DA CUUUUUBE
        }
        
        return true;
    }

    protected override float[][] genTransformationMatrices() {
        float[][] tfMatrices = new float[1][];

        tfMatrices[0] =
            new Matrixf()
            .Translate(0.5f, 0, 0.5f)
            .Translate(-0.5f, 0.2525f, -0.5f)
            .Values;

        return tfMatrices;
    }

    public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldForResolving) {
        base.FromTreeAttributes(tree, worldForResolving);
        RedrawAfterReceivingTreeAttributes(worldForResolving);
    }

    public override void GetBlockInfo(IPlayer forPlayer, StringBuilder sb) {
        base.GetBlockInfo(forPlayer, sb);
        DisplayInfo(forPlayer, sb, inv, displaySelection, slotCount);
    }
}

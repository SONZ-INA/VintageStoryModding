namespace FoodShelves;

public class BlockBarrelRackBig : BlockLiquidContainerBase, IMultiBlockColSelBoxes {
    public override bool AllowHeldLiquidTransfer => false;
    public override int GetContainerSlotId(BlockPos pos) => 1;
    public override int GetContainerSlotId(ItemStack containerStack) => 1;

    public override void OnLoaded(ICoreAPI api) {
        base.OnLoaded(api);
        PlacedPriorityInteract = true; // Needed to call OnBlockInteractStart when shifting with an item in hand
    }

    public override bool DoParticalSelection(IWorldAccessor world, BlockPos pos) {
        return true;
    }

    public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel) {
        if (world.BlockAccessor.GetBlockEntity(blockSel.Position) is BlockEntityBarrelRackBig hbrb) return hbrb.OnInteract(byPlayer, blockSel);
        return base.OnBlockInteractStart(world, byPlayer, blockSel);
    }

    public bool BaseOnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel) {
        return base.OnBlockInteractStart(world, byPlayer, blockSel);
    }

    public override WorldInteraction[] GetPlacedBlockInteractionHelp(IWorldAccessor world, BlockSelection selection, IPlayer forPlayer) {
        if (world.BlockAccessor.GetBlockEntity(selection.Position) is BlockEntityBarrelRackBig be && be.Inventory.Empty) return null;
        else return base.GetPlacedBlockInteractionHelp(world, selection, forPlayer);
    }

    public override void OnBlockBroken(IWorldAccessor world, BlockPos pos, IPlayer byPlayer, float dropQuantityMultiplier = 1) {
        // First, check for behaviors preventing default, for example Reinforcement system
        bool preventDefault = false;
        foreach (BlockBehavior behavior in BlockBehaviors) {
            EnumHandling handled = EnumHandling.PassThrough;

            behavior.OnBlockBroken(world, pos, byPlayer, ref handled);
            if (handled == EnumHandling.PreventDefault) preventDefault = true;
            if (handled == EnumHandling.PreventSubsequent) return;
        }

        if (preventDefault) return;

        // Drop inventory (the barrel)
        BlockEntityBarrelRackBig be = GetBlockEntity<BlockEntityBarrelRackBig>(pos);
        be?.Inventory.DropAll(pos.ToVec3d());

        base.OnBlockBroken(world, pos, byPlayer);
    }

    // Selection box for master block
    public override Cuboidf[] GetSelectionBoxes(IBlockAccessor blockAccessor, BlockPos pos) {
        BlockEntityBarrelRackBig be = blockAccessor.GetBlockEntity<BlockEntityBarrelRackBig>(pos);
        if (be != null) {
            int[] transformedIndex = GetMultiblockIndex(new Vec3i() { X = 0, Y = 0, Z = 0 }, be);
            Cuboidf singleSelectionBox = new(
                transformedIndex[0],
                transformedIndex[1],
                transformedIndex[2] - 1,
                transformedIndex[0] + 2,
                transformedIndex[1] + 2,
                transformedIndex[2] + 1
            );

            return new Cuboidf[] { singleSelectionBox };
        }

        return base.GetSelectionBoxes(blockAccessor, pos);
    }

    // Selection boxes for multiblock parts
    public Cuboidf[] MBGetSelectionBoxes(IBlockAccessor blockAccessor, BlockPos pos, Vec3i offset) {
        BlockEntityBarrelRackBig be = blockAccessor.GetBlockEntityExt<BlockEntityBarrelRackBig>(pos);
        if (be != null) {
            int[] transformedIndex = GetMultiblockIndex(offset, be);

            Cuboidf singleSelectionBox = new(
                transformedIndex[0],
                transformedIndex[1],
                transformedIndex[2] - 1,
                transformedIndex[0] + 2,
                transformedIndex[1] + 2,
                transformedIndex[2] + 1
            );

            return new Cuboidf[] { singleSelectionBox };
        }

        return base.GetSelectionBoxes(blockAccessor, pos);
    }

    public Cuboidf[] MBGetCollisionBoxes(IBlockAccessor blockAccessor, BlockPos pos, Vec3i offset) {
        return base.GetCollisionBoxes(blockAccessor, pos);
    }

    public override void TryFillFromBlock(EntityItem byEntityItem, BlockPos pos) {
        // Don't fill when dropped as item in water
    }

    public override string GetPlacedBlockInfo(IWorldAccessor world, BlockPos pos, IPlayer forPlayer) {
        StringBuilder dsc = new();

        BlockEntityBarrelRackBig be = GetBlockEntity<BlockEntityBarrelRackBig>(pos);
        if (be != null && be.Inventory.Empty) {
            dsc.Append(Lang.Get("foodshelves:Missing barrel."));
        }
        else {
            dsc.Append(base.GetPlacedBlockInfo(world, pos, forPlayer));
        }

        return dsc.ToString();
    }
}

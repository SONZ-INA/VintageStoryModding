using System.Linq;

namespace FoodShelves;

public class BlockCoolingCabinet : Block, IMultiBlockColSelBoxes {
    public override int GetRetention(BlockPos pos, BlockFacing facing, EnumRetentionType type) {
        return 0; // To prevent the block reducing the cellar rating
    }

    public override bool DoParticalSelection(IWorldAccessor world, BlockPos pos) {
        return true;
    }

    public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel) {
        if (world.BlockAccessor.GetBlockEntity(blockSel.Position) is BlockEntityCoolingCabinet befridge) return befridge.OnInteract(byPlayer, blockSel);
        return base.OnBlockInteractStart(world, byPlayer, blockSel);
    }

    public override string GetHeldItemName(ItemStack itemStack) {
        string variantName = itemStack.GetMaterialNameLocalized();
        return base.GetHeldItemName(itemStack) + " " + variantName;
    }

    // Selection box for master block
    public override Cuboidf[] GetSelectionBoxes(IBlockAccessor blockAccessor, BlockPos pos) {
        return base.GetSelectionBoxes(blockAccessor, pos);
    }

    // Selection boxes for multiblock parts
    public Cuboidf[] MBGetSelectionBoxes(IBlockAccessor blockAccessor, BlockPos pos, Vec3i offset) {
        // All of this needs to be in a specific static method for easier management.
        BlockEntityCoolingCabinet be = blockAccessor.GetBlockEntityExt<BlockEntityCoolingCabinet>(pos);
        if (be != null) {
            Cuboidf drawerSelBox = base.GetSelectionBoxes(blockAccessor, pos).ElementAt(0).Clone();
            Cuboidf cabinetSelBox = base.GetSelectionBoxes(blockAccessor, pos).ElementAt(1).Clone();

            Cuboidf currentSelBox = offset.Y == 0 ? drawerSelBox : cabinetSelBox;

            currentSelBox.X1 += offset.X;
            currentSelBox.X2 += offset.X;
            currentSelBox.Z1 += offset.Z;
            currentSelBox.Z2 += offset.Z;

            // Push the selection box down when aiming at the top 2 blocks of the cabinet
            if (offset.Y != 0) {
                currentSelBox.Y1 -= 1f;
                currentSelBox.Y2 -= 1f;
            }

            // Bottom-right block fixing the "deadzone"
            if (offset.Z != offset.X && offset.Y == 0) {
                cabinetSelBox.X1 += offset.X;
                cabinetSelBox.X2 += offset.X;
                cabinetSelBox.Z1 += offset.Z;
                cabinetSelBox.Z2 += offset.Z;

                return new Cuboidf[] { drawerSelBox, cabinetSelBox };
            }

            return new Cuboidf[] { currentSelBox };
        }

        return base.GetSelectionBoxes(blockAccessor, pos);
    }

    public Cuboidf[] MBGetCollisionBoxes(IBlockAccessor blockAccessor, BlockPos pos, Vec3i offset) {
        return base.GetCollisionBoxes(blockAccessor, pos);
    }
}
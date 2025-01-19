using System.Linq;

namespace FoodShelves;

public class BlockCoolingCabinet : Block, IMultiBlockColSelBoxes {
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
        BlockEntityCoolingCabinet be = blockAccessor.GetBlockEntityExt<BlockEntityCoolingCabinet>(pos);
        if (be != null) {
            Cuboidf currentSelBox = base.GetSelectionBoxes(blockAccessor, pos).FirstOrDefault().Clone(); // Ne moze first or default
            // Takodje, ispada da nesto bas i ne radi najbolje kada se ne targetuje master block.

            currentSelBox.X1 += offset.X;
            currentSelBox.X2 += offset.X;
            currentSelBox.Z1 += offset.Z;
            currentSelBox.Z2 += offset.Z;
            api.Logger.Debug(currentSelBox.ToString());

            return new Cuboidf[] { currentSelBox };
        }

        return base.GetSelectionBoxes(blockAccessor, pos);
    }

    public Cuboidf[] MBGetCollisionBoxes(IBlockAccessor blockAccessor, BlockPos pos, Vec3i offset) {
        return base.GetCollisionBoxes(blockAccessor, pos);
    }
}
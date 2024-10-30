namespace FoodShelves;

public class BlockBehaviorCeilingAttachable : BlockBehavior {
    public BlockBehaviorCeilingAttachable(Block block) : base(block) { }

    public override bool TryPlaceBlock(IWorldAccessor world, IPlayer byPlayer, ItemStack itemstack, BlockSelection blockSel, ref EnumHandling handling, ref string failureCode) {
        handling = EnumHandling.PreventDefault;

        if (blockSel.Face == BlockFacing.DOWN) {
            block.DoPlaceBlock(world, byPlayer, blockSel, itemstack);
            return true;
        }

        failureCode = "requireceilingattachable";
        return false;
    }

    public override void OnNeighbourBlockChange(IWorldAccessor world, BlockPos pos, BlockPos neibpos, ref EnumHandling handled) {
        handled = EnumHandling.PreventDefault;

        if (!CanBlockStay(world, pos)) {
            world.BlockAccessor.BreakBlock(pos, null);
        }
    }

    bool CanBlockStay(IWorldAccessor world, BlockPos pos) {
        Block attachingBlock = world.BlockAccessor.GetBlock(pos.UpCopy());
        return attachingBlock.CanAttachBlockAt(world.BlockAccessor, block, pos, BlockFacing.DOWN);
    }

    public override bool CanAttachBlockAt(IBlockAccessor world, Block block, BlockPos pos, BlockFacing blockFace, ref EnumHandling handled, Cuboidi attachmentArea = null) {
        handled = EnumHandling.PreventDefault;
        return false;
    }
}
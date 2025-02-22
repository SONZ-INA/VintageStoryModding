namespace FoodShelves;

public class BlockTableWShelf : Block {
    public override int GetRetention(BlockPos pos, BlockFacing facing, EnumRetentionType type) {
        return 0; // To prevent the block reducing the cellar rating
    }

    public override bool DoParticalSelection(IWorldAccessor world, BlockPos pos) {
        return true;
    }

    public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel) {
        if (world.BlockAccessor.GetBlockEntity(blockSel.Position) is BlockEntityTableWShelf beshelf) return beshelf.OnInteract(byPlayer, blockSel);
        return base.OnBlockInteractStart(world, byPlayer, blockSel);
    }
}

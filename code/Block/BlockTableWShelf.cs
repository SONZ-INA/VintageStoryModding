namespace FoodShelves;

public class BlockTableWShelf : Block {
    public override void OnLoaded(ICoreAPI api) {
        base.OnLoaded(api);
    }

    public override bool DoParticalSelection(IWorldAccessor world, BlockPos pos) {
        return true;
    }

    public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel) {
        if (world.BlockAccessor.GetBlockEntity(blockSel.Position) is BlockEntityTableWShelf beshelf) return beshelf.OnInteract(byPlayer, blockSel);
        return base.OnBlockInteractStart(world, byPlayer, blockSel);
    }
}

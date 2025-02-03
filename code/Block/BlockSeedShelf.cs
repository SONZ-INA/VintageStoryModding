namespace FoodShelves;

public class BlockSeedShelf : Block {
    public override void OnLoaded(ICoreAPI api) {
        base.OnLoaded(api);
        PlacedPriorityInteract = true; // Needed to call OnBlockInteractStart when shifting with an item in hand
    }

    public override bool DoParticalSelection(IWorldAccessor world, BlockPos pos) {
        return true;
    }

    public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel) {
        if (world.BlockAccessor.GetBlockEntity(blockSel.Position) is BlockEntitySeedShelf beshelf) return beshelf.OnInteract(byPlayer, blockSel);
        return base.OnBlockInteractStart(world, byPlayer, blockSel);
    }

    public override string GetHeldItemName(ItemStack itemStack) {
        string variantType = "";
        if (this.Code.SecondCodePart().StartsWith("short"))
            variantType = Lang.Get("foodshelves:Short") + " ";
        if (this.Code.SecondCodePart().StartsWith("veryshort"))
            variantType = Lang.Get("foodshelves:Very Short") + " ";

        return variantType + base.GetHeldItemName(itemStack) + " " + itemStack.GetMaterialNameLocalized();
    }
}

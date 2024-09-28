namespace FoodShelves;

public class BlockHorizontalBarrelBig : Block {
    public override bool DoParticalSelection(IWorldAccessor world, BlockPos pos) {
        return true;
    }

    public override bool TryPlaceBlock(IWorldAccessor world, IPlayer byPlayer, ItemStack itemstack, BlockSelection blockSel, ref string failureCode) {
        (api as ICoreClientAPI).TriggerIngameError(this, "cantplace", Lang.Get("This barrel needs to be placed in a barrel rack."));
        failureCode = "__ignore__";
        return false;
    }

    public override string GetHeldItemName(ItemStack itemStack) {
        string variantName = itemStack.GetMaterialNameLocalized();
        return base.GetHeldItemName(itemStack) + variantName;
    }
}

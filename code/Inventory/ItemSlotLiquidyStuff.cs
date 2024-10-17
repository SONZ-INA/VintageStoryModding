namespace FoodShelves;

public class ItemSlotLiquidyStuff : ItemSlot {
    public override int MaxSlotStackSize => 32;

    public ItemSlotLiquidyStuff(InventoryBase inventory) : base(inventory) {
        this.inventory = inventory;
    }

    public override bool CanTakeFrom(ItemSlot slot, EnumMergePriority priority = EnumMergePriority.AutoMerge) {
        return slot.LiquidyStuffCheck() && base.CanTakeFrom(slot, priority);
    }

    public override bool CanHold(ItemSlot slot) {
        return slot.LiquidyStuffCheck() && base.CanHold(slot);
    }
}
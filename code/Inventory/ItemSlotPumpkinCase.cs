namespace FoodShelves;

public class ItemSlotPumpkinCase : ItemSlot {
    public override int MaxSlotStackSize => 1;

    public ItemSlotPumpkinCase(InventoryBase inventory) : base(inventory) {
        this.inventory = inventory;
    }

    public override bool CanTakeFrom(ItemSlot slot, EnumMergePriority priority = EnumMergePriority.AutoMerge) {
        return slot.PumpkinCaseCheck() && base.CanTakeFrom(slot, priority);
    }

    public override bool CanHold(ItemSlot slot) {
        return slot.PumpkinCaseCheck() && base.CanHold(slot);
    }
}
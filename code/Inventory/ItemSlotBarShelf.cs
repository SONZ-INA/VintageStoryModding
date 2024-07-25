namespace FoodShelves;

public class ItemSlotBarShelf : ItemSlot {
    public override int MaxSlotStackSize => 1;

    public ItemSlotBarShelf(InventoryBase inventory) : base(inventory) {
        this.inventory = inventory;
    }

    public override bool CanTakeFrom(ItemSlot slot, EnumMergePriority priority = EnumMergePriority.AutoMerge) {
        return slot.BarShelfCheck() && base.CanTakeFrom(slot, priority);
    }

    public override bool CanHold(ItemSlot slot) {
        return slot.BarShelfCheck() && base.CanHold(slot);
    }
}
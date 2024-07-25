namespace FoodShelves;

public class ItemSlotBreadShelf : ItemSlot {
    public override int MaxSlotStackSize => 1;

    public ItemSlotBreadShelf(InventoryBase inventory) : base(inventory) {
        this.inventory = inventory;
    }

    public override bool CanTakeFrom(ItemSlot slot, EnumMergePriority priority = EnumMergePriority.AutoMerge) {
        return slot.BreadShelfCheck() && base.CanTakeFrom(slot, priority);
    }

    public override bool CanHold(ItemSlot slot) {
        return slot.BreadShelfCheck() && base.CanHold(slot);
    }
}
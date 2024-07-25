namespace FoodShelves;

public class ItemSlotPieShelf : ItemSlot {
    public override int MaxSlotStackSize => 1;

    public ItemSlotPieShelf(InventoryBase inventory) : base(inventory) {
        this.inventory = inventory;
    }

    public override bool CanTakeFrom(ItemSlot slot, EnumMergePriority priority = EnumMergePriority.AutoMerge) {
        return slot.PieShelfCheck() && base.CanTakeFrom(slot, priority);
    }

    public override bool CanHold(ItemSlot slot) {
        return slot.PieShelfCheck() && base.CanHold(slot);
    }
}
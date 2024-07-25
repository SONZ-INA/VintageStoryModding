namespace FoodShelves;

public class ItemSlotTableWShelf : ItemSlot {
    public override int MaxSlotStackSize => 1;

    public ItemSlotTableWShelf(InventoryBase inventory) : base(inventory) {
        this.inventory = inventory;
    }

    public override bool CanTakeFrom(ItemSlot slot, EnumMergePriority priority = EnumMergePriority.AutoMerge) {
        return slot.ShelvableCheck() && base.CanTakeFrom(slot, priority);
    }

    public override bool CanHold(ItemSlot slot) {
        return slot.ShelvableCheck() && base.CanHold(slot);
    }
}
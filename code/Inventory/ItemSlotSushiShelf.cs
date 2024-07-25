namespace FoodShelves;

public class ItemSlotSushiShelf : ItemSlot {
    public override int MaxSlotStackSize => 1;

    public ItemSlotSushiShelf(InventoryBase inventory) : base(inventory) {
        this.inventory = inventory;
    }

    public override bool CanTakeFrom(ItemSlot slot, EnumMergePriority priority = EnumMergePriority.AutoMerge) {
        return slot.SushiShelfCheck() && base.CanTakeFrom(slot, priority);
    }

    public override bool CanHold(ItemSlot slot) {
        return slot.SushiShelfCheck() && base.CanHold(slot);
    }
}
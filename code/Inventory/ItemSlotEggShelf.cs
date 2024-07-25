namespace FoodShelves;

public class ItemSlotEggShelf : ItemSlot {
    public override int MaxSlotStackSize => 1;

    public ItemSlotEggShelf(InventoryBase inventory) : base(inventory) {
        this.inventory = inventory;
    }

    public override bool CanTakeFrom(ItemSlot slot, EnumMergePriority priority = EnumMergePriority.AutoMerge) {
        return slot.EggShelfCheck() && base.CanTakeFrom(slot, priority);
    }

    public override bool CanHold(ItemSlot slot) {
        return slot.EggShelfCheck() && base.CanHold(slot);
    }
}
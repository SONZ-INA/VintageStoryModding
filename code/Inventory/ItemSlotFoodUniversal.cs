namespace FoodShelves;

public class ItemSlotFoodUniversal : ItemSlot {
    public override int MaxSlotStackSize => 1;

    public ItemSlotFoodUniversal(InventoryBase inventory) : base(inventory) {
        this.inventory = inventory;
    }

    public override bool CanTakeFrom(ItemSlot slot, EnumMergePriority priority = EnumMergePriority.AutoMerge) {
        return slot.FoodUniversalCheck() && base.CanTakeFrom(slot, priority);
    }

    public override bool CanHold(ItemSlot slot) {
        return slot.FoodUniversalCheck() && base.CanHold(slot);
    }
}
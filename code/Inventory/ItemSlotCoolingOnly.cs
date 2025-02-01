namespace FoodShelves;

public class ItemSlotCoolingOnly : ItemSlot {
    public override int MaxSlotStackSize => 1;

    public ItemSlotCoolingOnly(InventoryBase inventory) : base(inventory) {
        this.inventory = inventory;
    }

    public override bool CanTakeFrom(ItemSlot slot, EnumMergePriority priority = EnumMergePriority.AutoMerge) {
        return slot.CoolingOnlyCheck() && base.CanTakeFrom(slot, priority);
    }

    public override bool CanHold(ItemSlot slot) {
        return slot.CoolingOnlyCheck() && base.CanHold(slot);
    }
}
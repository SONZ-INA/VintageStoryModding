namespace FoodShelves;

public class ItemSlotBarrelRackBig : ItemSlot {
    public override int MaxSlotStackSize => 1;

    public ItemSlotBarrelRackBig(InventoryBase inventory) : base(inventory) {
        this.inventory = inventory;
    }

    public override bool CanTakeFrom(ItemSlot slot, EnumMergePriority priority = EnumMergePriority.AutoMerge) {
        return slot.HorizontalBarrelRackBigCheck() && base.CanTakeFrom(slot, priority);
    }

    public override bool CanHold(ItemSlot slot) {
        return slot.HorizontalBarrelRackBigCheck() && base.CanHold(slot);
    }
}
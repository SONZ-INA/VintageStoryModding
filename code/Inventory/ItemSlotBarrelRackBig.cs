namespace FoodShelves;

public class ItemSlotBarrelRackBig : ItemSlot {
    public override int MaxSlotStackSize => 1;

    public ItemSlotBarrelRackBig(InventoryBase inventory) : base(inventory) {
        this.inventory = inventory;
    }

    public override bool CanTakeFrom(ItemSlot slot, EnumMergePriority priority = EnumMergePriority.AutoMerge) {
        return slot.BarrelRackBigCheck() && base.CanTakeFrom(slot, priority);
    }

    public override bool CanHold(ItemSlot slot) {
        return slot.BarrelRackBigCheck() && base.CanHold(slot);
    }
}
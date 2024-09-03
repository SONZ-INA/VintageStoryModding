namespace FoodShelves;

public class ItemSlotBarrelRack : ItemSlot {
    public override int MaxSlotStackSize => 1;

    public ItemSlotBarrelRack(InventoryBase inventory) : base(inventory) {
        this.inventory = inventory;
    }

    public override bool CanTakeFrom(ItemSlot slot, EnumMergePriority priority = EnumMergePriority.AutoMerge) {
        return slot.BarrelRackCheck() && base.CanTakeFrom(slot, priority);
    }

    public override bool CanHold(ItemSlot slot) {
        return slot.BarrelRackCheck() && base.CanHold(slot);
    }
}
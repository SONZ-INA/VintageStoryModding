namespace FoodShelves;

public class ItemSlotHorizontalBarrelRack : ItemSlot {
    public override int MaxSlotStackSize => 1;

    public ItemSlotHorizontalBarrelRack(InventoryBase inventory) : base(inventory) {
        this.inventory = inventory;
    }

    public override bool CanTakeFrom(ItemSlot slot, EnumMergePriority priority = EnumMergePriority.AutoMerge) {
        return slot.HorizontalBarrelRackCheck() && base.CanTakeFrom(slot, priority);
    }

    public override bool CanHold(ItemSlot slot) {
        return slot.HorizontalBarrelRackCheck() && base.CanHold(slot);
    }
}
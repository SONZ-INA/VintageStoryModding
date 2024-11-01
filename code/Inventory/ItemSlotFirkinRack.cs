namespace FoodShelves;

public class ItemSlotFirkinRack : ItemSlot {
    public override int MaxSlotStackSize => 1;

    public ItemSlotFirkinRack(InventoryBase inventory) : base(inventory) {
        this.inventory = inventory;
    }

    public override bool CanTakeFrom(ItemSlot slot, EnumMergePriority priority = EnumMergePriority.AutoMerge) {
        return slot.FirkinRackCheck() && base.CanTakeFrom(slot, priority);
    }

    public override bool CanHold(ItemSlot slot) {
        return slot.FirkinRackCheck() && base.CanHold(slot);
    }
}
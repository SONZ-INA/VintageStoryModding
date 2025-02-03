namespace FoodShelves;

public class ItemSlotHolderUniversal : ItemSlot {
    public override int MaxSlotStackSize => 1;

    public ItemSlotHolderUniversal(InventoryBase inventory) : base(inventory) {
        this.inventory = inventory;
    }

    public override bool CanTakeFrom(ItemSlot slot, EnumMergePriority priority = EnumMergePriority.AutoMerge) {
        return slot.HolderUniversalCheck() && base.CanTakeFrom(slot, priority);
    }

    public override bool CanHold(ItemSlot slot) {
        return slot.HolderUniversalCheck() && base.CanHold(slot);
    }
}
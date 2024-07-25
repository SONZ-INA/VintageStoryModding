namespace FoodShelves;

public class ItemSlotSeedShelf : ItemSlot {
    public override int MaxSlotStackSize => 64;

    public ItemSlotSeedShelf(InventoryBase inventory) : base(inventory) {
        this.inventory = inventory;
    }

    public override bool CanTakeFrom(ItemSlot slot, EnumMergePriority priority = EnumMergePriority.AutoMerge) {
        return slot.SeedShelfCheck() && base.CanTakeFrom(slot, priority);
    }

    public override bool CanHold(ItemSlot slot) {
        return slot.SeedShelfCheck() && base.CanHold(slot);
    }
}
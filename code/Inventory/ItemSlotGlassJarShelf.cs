namespace FoodShelves;

public class ItemSlotGlassJarShelf : ItemSlot {
    public override int MaxSlotStackSize => 1;

    public ItemSlotGlassJarShelf(InventoryBase inventory) : base(inventory) {
        this.inventory = inventory;
    }

    public override bool CanTakeFrom(ItemSlot slot, EnumMergePriority priority = EnumMergePriority.AutoMerge) {
        return slot.GlassJarShelfCheck() && base.CanTakeFrom(slot, priority);
    }

    public override bool CanHold(ItemSlot slot) {
        return slot.GlassJarShelfCheck() && base.CanHold(slot);
    }
}
namespace FoodShelves;

public class ItemSlotEggBasket : ItemSlot {
    public override int MaxSlotStackSize => 1;

    public ItemSlotEggBasket(InventoryBase inventory) : base(inventory) {
        this.inventory = inventory;
    }

    public override bool CanTakeFrom(ItemSlot slot, EnumMergePriority priority = EnumMergePriority.AutoMerge) {
        return slot.EggBasketCheck() && base.CanTakeFrom(slot, priority);
    }

    public override bool CanHold(ItemSlot slot) {
        return slot.EggBasketCheck() && base.CanHold(slot);
    }
}
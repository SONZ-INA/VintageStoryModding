namespace FoodShelves;

public class ItemSlotFruitBasket : ItemSlot {
    public override int MaxSlotStackSize => 1;

    public ItemSlotFruitBasket(InventoryBase inventory) : base(inventory) {
        this.inventory = inventory;
    }

    public override bool CanTakeFrom(ItemSlot slot, EnumMergePriority priority = EnumMergePriority.AutoMerge) {
        return slot.FruitBasketCheck() && base.CanTakeFrom(slot, priority);
    }

    public override bool CanHold(ItemSlot slot) {
        return slot.FruitBasketCheck() && base.CanHold(slot);
    }
}
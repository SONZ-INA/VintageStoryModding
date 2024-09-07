namespace FoodShelves;

public class ItemSlotVegetableBasket : ItemSlot {
    public override int MaxSlotStackSize => 1;

    public ItemSlotVegetableBasket(InventoryBase inventory) : base(inventory) {
        this.inventory = inventory;
    }

    public override bool CanTakeFrom(ItemSlot slot, EnumMergePriority priority = EnumMergePriority.AutoMerge) {
        return slot.VegetableBasketCheck() && base.CanTakeFrom(slot, priority);
    }

    public override bool CanHold(ItemSlot slot) {
        return slot.VegetableBasketCheck() && base.CanHold(slot);
    }
}
using static Vintagestory.GameContent.BlockLiquidContainerBase;

namespace FoodShelves;

public class BlockEntityBarrelRackBig : BlockEntityContainer {
    readonly InventoryGeneric inv;
    BlockBarrelRackBig block;

    public override InventoryBase Inventory => inv;
    public override string InventoryClassName => Block?.Attributes?["inventoryClassName"].AsString();

    private int CapacityLitres { get; set; } = 500;
    static readonly int slotCount = 2;

    public BlockEntityBarrelRackBig() {
        inv = new InventoryGeneric(slotCount, InventoryClassName + "-0", Api, (id, inv) => {
            if (id == 0) return new ItemSlotBarrelRackBig(inv);
            else return new ItemSlotLiquidOnly(inv, CapacityLitres);
        });
    }

    public override void Initialize(ICoreAPI api) {
        base.Initialize(api);
        block = api.World.BlockAccessor.GetBlock(Pos) as BlockBarrelRackBig;

        if (block?.Attributes?["capacityLitres"].Exists == true) {
            CapacityLitres = block.Attributes["capacityLitres"].AsInt(50);
            (inv[1] as ItemSlotLiquidOnly).CapacityLitres = CapacityLitres;
        }

        // Patch "rack-top" to not be stackable
        if (block?.Code.Path.StartsWith("horizontalbarrelrackbig-top-") == true) {
            block.SideSolid = new SmallBoolArray(0); 
        }
    }

    internal bool OnInteract(IPlayer byPlayer, BlockSelection blockSel) {
        ItemSlot slot = byPlayer.InventoryManager.ActiveHotbarSlot;


        if (slot.Empty && byPlayer.CurrentBlockSelection.SelectionBoxIndex == 0) { // Take barrel from rack
            if (inv[1].Empty) {
                return TryTake(byPlayer, blockSel);
            }
            else {
                (Api as ICoreClientAPI)?.TriggerIngameError(this, "canttake", Lang.Get("foodshelves:The barrel must be emptied before it can be picked up."));
                return false;
            }
        }
        else if (!slot.Empty && byPlayer.CurrentBlockSelection.SelectionBoxIndex == 2) { // Fill container with liquid from barrel rack
            CollectibleObject collectible = slot.Itemstack?.Collectible;
            if (collectible is ILiquidSink objLsi) {
                if (!objLsi.AllowHeldLiquidTransfer) {
                    return false;
                }

                ItemStack owncontentStack = block.GetContent(blockSel.Position);
                if (owncontentStack == null) {
                    return false;
                }

                ItemStack contentStack = owncontentStack.Clone();
                bool shiftKey = byPlayer.Entity.Controls.ShiftKey;
                float litres = (shiftKey ? objLsi.TransferSizeLitres : objLsi.CapacityLitres);

                int num = SplitStackAndPerformAction(byPlayer.Entity, slot, (ItemStack stack) => objLsi.TryPutLiquid(stack, owncontentStack, litres));
                if (num > 0) {
                    block.TryTakeContent(blockSel.Position, num);
                    block.DoLiquidMovedEffects(byPlayer, contentStack, num, EnumLiquidDirection.Fill);
                    return true;
                }
            }

            return false;
        }
        else {
            if (inv.Empty && slot.HorizontalBarrelRackBigCheck()) { // Put barrel inside
                AssetLocation sound = slot.Itemstack?.Block?.Sounds?.Place;

                if (TryPut(slot, blockSel)) {
                    Api.World.PlaySoundAt(sound ?? new AssetLocation("sounds/player/build"), byPlayer.Entity, byPlayer, true, 16);
                    MarkDirty();
                    return true;
                }
            }
            else if (!inv.Empty && blockSel.SelectionBoxIndex == 1) { // Putting liquid inside the barrel (only possible if there's a barrel inside)
                CollectibleObject obj = slot.Itemstack?.Collectible;

                if (obj is ILiquidSource objLso) {
                    var contentStack = objLso.GetContent(slot.Itemstack);
                    if (contentStack != null) {
                        bool shiftKey = byPlayer.Entity.Controls.ShiftKey;
                        float litres = (shiftKey ? objLso.TransferSizeLitres : objLso.CapacityLitres);

                        int moved = block.TryPutLiquid(blockSel.Position, contentStack, litres);
                        if (moved > 0) {
                            SplitStackAndPerformAction(byPlayer.Entity, slot, delegate (ItemStack stack) {
                                objLso.TryTakeContent(stack, moved);
                                return moved;
                            });
                            block.DoLiquidMovedEffects(byPlayer, contentStack, moved, EnumLiquidDirection.Pour);
                            return true;
                        }
                    }
                }
            }
            else {
                (Api as ICoreClientAPI)?.TriggerIngameError(this, "cantplace", Lang.Get("foodshelves:Only barrels can be placed on this rack."));
            }

            return false;
        }
    }

    private bool TryPut(ItemSlot slot, BlockSelection blockSel) {
        int index = blockSel.SelectionBoxIndex;
        if (index < 0 || index >= slotCount) return false;

        if (inv[index].Empty) {
            int moved = slot.TryPutInto(Api.World, inv[index]);
            MarkDirty(true);
            (Api as ICoreClientAPI)?.World.Player.TriggerFpAnimation(EnumHandInteract.HeldItemInteract);

            return moved > 0;
        }

        return false;
    }

    private bool TryTake(IPlayer byPlayer, BlockSelection blockSel) {
        int index = blockSel.SelectionBoxIndex;
        if (index < 0 || index >= slotCount) return false;

        if (!inv[index].Empty) {
            ItemStack stack = inv[index].TakeOut(1);
            if (byPlayer.InventoryManager.TryGiveItemstack(stack)) {
                AssetLocation sound = stack.Block?.Sounds?.Place;
                Api.World.PlaySoundAt(sound ?? new AssetLocation("sounds/player/build"), byPlayer.Entity, byPlayer, true, 16);
            }

            if (stack.StackSize > 0) {
                Api.World.SpawnItemEntity(stack, Pos.ToVec3d().Add(0.5, 0.5, 0.5));
            }

            (Api as ICoreClientAPI)?.World.Player.TriggerFpAnimation(EnumHandInteract.HeldItemInteract);
            MarkDirty(true);
            return true;
        }

        return false;
    }

    public override bool OnTesselation(ITerrainMeshPool mesher, ITesselatorAPI tesselator) {
        bool skipmesh = base.OnTesselation(mesher, tesselator);

        if (!skipmesh) {
            MeshData meshData = GenBlockMeshUnhashed(Api, this, tesselator);
            if (meshData == null) return false;

            ItemStack[] stack = GetContentStacks();
            if (stack[0] != null && stack[0].Block != null) {
                tesselator.TesselateBlock(stack[0].Block, out MeshData barrel);
                if (barrel != null) meshData.AddMeshData(barrel.BlockYRotation(this));
            }

            mesher.AddMeshData(meshData.Clone());
        }

        return true;
    }

    // Copied from vanilla BlockLiquidContainerBase
    private int SplitStackAndPerformAction(Entity byEntity, ItemSlot slot, System.Func<ItemStack, int> action) {
        if (slot.Itemstack == null) {
            return 0;
        }

        if (slot.Itemstack.StackSize == 1) {
            int num = action(slot.Itemstack);
            if (num > 0) {
                _ = slot.Itemstack.Collectible.MaxStackSize;
                EntityPlayer obj = byEntity as EntityPlayer;
                if (obj == null) {
                    return num;
                }

                obj.WalkInventory(delegate (ItemSlot pslot) {
                    if (pslot.Empty || pslot is ItemSlotCreative || pslot.StackSize == pslot.Itemstack.Collectible.MaxStackSize) {
                        return true;
                    }

                    int mergableQuantity = slot.Itemstack.Collectible.GetMergableQuantity(slot.Itemstack, pslot.Itemstack, EnumMergePriority.DirectMerge);
                    if (mergableQuantity == 0) {
                        return true;
                    }

                    BlockLiquidContainerBase obj3 = slot.Itemstack.Collectible as BlockLiquidContainerBase;
                    BlockLiquidContainerBase blockLiquidContainerBase = pslot.Itemstack.Collectible as BlockLiquidContainerBase;
                    if ((obj3?.GetContent(slot.Itemstack)?.StackSize).GetValueOrDefault() != (blockLiquidContainerBase?.GetContent(pslot.Itemstack)?.StackSize).GetValueOrDefault()) {
                        return true;
                    }

                    slot.Itemstack.StackSize += mergableQuantity;
                    pslot.TakeOut(mergableQuantity);
                    slot.MarkDirty();
                    pslot.MarkDirty();
                    return true;
                });
            }

            return num;
        }

        ItemStack itemStack = slot.Itemstack.Clone();
        itemStack.StackSize = 1;
        int num2 = action(itemStack);
        if (num2 > 0) {
            slot.TakeOut(1);
            if (byEntity is not EntityPlayer obj2 || !obj2.Player.InventoryManager.TryGiveItemstack(itemStack, slotNotifyEffect: true)) {
                Api.World.SpawnItemEntity(itemStack, byEntity.SidedPos.XYZ);
            }

            slot.MarkDirty();
        }

        return num2;
    }
}

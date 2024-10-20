namespace FoodShelves;

public class BlockEntityCeilingJar : BlockEntityDisplay {
    private readonly InventoryGeneric inv;
    private Block block;
    
    public override InventoryBase Inventory => inv;
    public override string InventoryClassName => Block?.Attributes?["inventoryClassName"].AsString();
    public override string AttributeTransformCode => Block?.Attributes?["attributeTransformCode"].AsString();

    private const int slotCount = 12;
    private readonly InfoDisplayOptions displaySelection = InfoDisplayOptions.ByBlockMerged;

    public BlockEntityCeilingJar() { inv = new InventoryGeneric(slotCount, InventoryClassName + "-0", Api, (_, inv) => new ItemSlotLiquidyStuff(inv)); }

    public override void Initialize(ICoreAPI api) {
        block = api.World.BlockAccessor.GetBlock(Pos);
        base.Initialize(api);
    }

    internal bool OnInteract(IPlayer byPlayer) {
        ItemSlot slot = byPlayer.InventoryManager.ActiveHotbarSlot;

        if (slot.Empty) {
            return TryTake(byPlayer);
        }
        else {
            if (slot.LiquidyStuffCheck()) {
                AssetLocation sound = slot.Itemstack?.Block?.Sounds?.Place;

                if (TryPut(byPlayer, slot)) {
                    Api.World.PlaySoundAt(sound ?? new AssetLocation("sounds/player/build"), byPlayer.Entity, byPlayer, true, 16);
                    MarkDirty();
                    return true;
                }
            }

            return false;
        }
    }

    // TryPut and TryTake should go to CeilingJar instead of this one, if this one would allow dropoff/pickup of items via UI mouse1/mouse2
    private bool TryPut(IPlayer byPlayer, ItemSlot slot) {
        // Ensure that we are only adding the same item type to the inventory
        if (!inv[0].Empty && inv[0].Itemstack.Collectible.Equals(slot.Itemstack.Collectible)) {
            int moved = 0;

            // Shift-click handling: Try to fill slots in order, starting from the first one
            if (byPlayer.Entity.Controls.ShiftKey) {
                for (int i = 0; i < inv.Count; i++) {
                    int availableSpace = inv[i].MaxSlotStackSize - inv[i].StackSize;
                    moved += slot.TryPutInto(Api.World, inv[i], availableSpace);

                    // Stop if the slot is still not full
                    if (inv[i].StackSize < inv[i].MaxSlotStackSize) {
                        break;
                    }
                }
            }
            else // Regular click: Place one item at a time, filling in sequence
            {
                for (int i = 0; i < inv.Count; i++) {
                    if (inv[i].StackSize < inv[i].MaxSlotStackSize) {
                        moved = slot.TryPutInto(Api.World, inv[i], 1);
                        break; // Stop after moving one item
                    }
                }
            }

            // If any items were moved, update the inventory and trigger the animation
            if (moved > 0) {
                MarkDirty();
                (Api as ICoreClientAPI)?.World.Player.TriggerFpAnimation(EnumHandInteract.HeldItemInteract);
                return true;
            }
        }

        // Handle the case where the first slot is empty and can receive items
        if (inv[0].Empty) {
            int moved = slot.TryPutInto(Api.World, inv[0]);

            if (moved > 0) {
                MarkDirty();
                (Api as ICoreClientAPI)?.World.Player.TriggerFpAnimation(EnumHandInteract.HeldItemInteract);
                return true;
            }
        }

        return false;
    }

    private bool TryTake(IPlayer byPlayer) {
        ItemStack stack;
        bool overflow = false;

        if (!inv[1].Empty) {
            if (byPlayer.Entity.Controls.ShiftKey)
                stack = inv[1].TakeOutWhole();
            else
                stack = inv[1].TakeOut(1);

            if (stack?.StackSize < stack?.Collectible.MaxStackSize && byPlayer.Entity.Controls.ShiftKey) overflow = true;
            if (stack != null && stack.StackSize > 0 && byPlayer.InventoryManager.TryGiveItemstack(stack)) {
                AssetLocation sound = stack.Block?.Sounds?.Place;
                Api.World.PlaySoundAt(sound ?? new AssetLocation("sounds/player/build"), byPlayer.Entity, byPlayer, true, 16);
                (Api as ICoreClientAPI)?.World.Player.TriggerFpAnimation(EnumHandInteract.HeldItemInteract);

                MarkDirty();
                if (!overflow) return true;
            }
        }

        if (!inv[0].Empty || overflow) {
            if (byPlayer.Entity.Controls.ShiftKey)
                stack = inv[0].TakeOutWhole();
            else
                stack = inv[0].TakeOut(1);

            if (stack != null && stack.StackSize > 0 && byPlayer.InventoryManager.TryGiveItemstack(stack)) {
                AssetLocation sound = stack.Block?.Sounds?.Place;
                Api.World.PlaySoundAt(sound ?? new AssetLocation("sounds/player/build"), byPlayer.Entity, byPlayer, true, 16);
                (Api as ICoreClientAPI)?.World.Player.TriggerFpAnimation(EnumHandInteract.HeldItemInteract);

                MarkDirty();
                return true;
            }
        }

        return false;
    }

    // Check this method for dehydrated fruit -> dry fruit
    protected override float Inventory_OnAcquireTransitionSpeed(EnumTransitionType transType, ItemStack stack, float baseMul) {
        if (transType == EnumTransitionType.Dry || transType == EnumTransitionType.Melt) return room?.ExitCount == 0 ? 2f : 0.5f;
        if (Api == null) return 0;

        return base.Inventory_OnAcquireTransitionSpeed(transType, stack, 0.75f);
    }

    public override bool OnTesselation(ITerrainMeshPool mesher, ITesselatorAPI tesselator) {
        tesselator.TesselateBlock(block, out MeshData blockMesh);
        if (blockMesh == null) return false;

        MeshData contentMesh = GenLiquidyMesh(capi, inv, ShapeReferences.CeilingJarUtil);
        if (contentMesh != null) blockMesh.AddMeshData(contentMesh);

        mesher.AddMeshData(blockMesh);
        
        return true;
    }

    // Unneeded
    protected override float[][] genTransformationMatrices() {
        float[][] tfMatrices = new float[1][];
        tfMatrices[0] = new Matrixf().Values;
        return tfMatrices;
    }

    public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldForResolving) {
        base.FromTreeAttributes(tree, worldForResolving);
        RedrawAfterReceivingTreeAttributes(worldForResolving);
    }

    public override void GetBlockInfo(IPlayer forPlayer, StringBuilder sb) {
        base.GetBlockInfo(forPlayer, sb);
        DisplayInfo(forPlayer, sb, inv, displaySelection, slotCount);
    }
}

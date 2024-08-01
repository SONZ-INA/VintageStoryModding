namespace FoodShelves;

public class BlockEntityHorizontalBarrelRack : BlockEntityContainer {
    readonly InventoryGeneric inv;
    Block block;
    
    public override InventoryBase Inventory => inv;
    public override string InventoryClassName => Block?.Attributes?["inventoryClassName"].AsString();

    private const int shelfCount = 1;
    private const int segmentsPerShelf = 1;
    private const int itemsPerSegment = 1;
    static readonly int slotCount = shelfCount * segmentsPerShelf * itemsPerSegment;
    private readonly InfoDisplayOptions displaySelection = InfoDisplayOptions.ByBlock;

    public BlockEntityHorizontalBarrelRack() { 
        inv = new InventoryGeneric(slotCount, InventoryClassName + "-0", Api, (_, inv) => new ItemSlotHorizontalBarrelRack(inv));
    }

    public override void Initialize(ICoreAPI api) {
        block = api.World.BlockAccessor.GetBlock(Pos);
        base.Initialize(api);

        // Patch "rack-top" to not be stackable
        if (block.Code.Path.StartsWith("horizontalbarrelrack-top-")) {
            block.SideSolid = new SmallBoolArray(0); 
        }
    }

    internal bool OnInteract(IPlayer byPlayer, BlockSelection blockSel) {
        ItemSlot slot = byPlayer.InventoryManager.ActiveHotbarSlot;

        if (slot.Empty) {
            if (TryTake(byPlayer, blockSel)) return true;
            else return false;
        }
        else {
            if (inv.Empty && slot.HorizontalBarrelRackCheck()) {
                AssetLocation sound = slot.Itemstack?.Block?.Sounds?.Place;

                if (TryPut(slot, blockSel)) {
                    Api.World.PlaySoundAt(sound ?? new AssetLocation("sounds/player/build"), byPlayer.Entity, byPlayer, true, 16);
                    MarkDirty();
                    return true;
                }
            }
            else if (!inv.Empty && blockSel.SelectionBoxIndex == 1) { // Putting liquid inside (only possible if there's a barrel inside)
                // putting a barrel into a container makes it an item, so i cannot use block classes for the barrel itself, i have to code the rack like that
                Api.Logger.Debug("SLCTION PASSD...");
                BlockLiquidContainerBase block = inv[0].Itemstack.Block as BlockLiquidContainerBase;
                CollectibleObject obj = slot.Itemstack.Collectible;
                if (obj is ILiquidSource objLso) {
                    Api.Logger.Debug("TRYING PUT LIQUID...");
                    var contentStack = objLso.GetContent(slot.Itemstack);
                    Api.Logger.Debug("   " + contentStack.ToString());
                    int moved = block.TryPutLiquid(blockSel.Position, contentStack, objLso.CapacityLitres);
                    Api.Logger.Debug("   " + moved);
                }
                else {
                    Api.Logger.Debug("TRANSFR FAILD.");
                }
            }
            else {
                (Api as ICoreClientAPI).TriggerIngameError(this, "cantplace", Lang.Get("foodshelves:Only horizontal barrels can be placed on this rack."));
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
            if (stack[0].Block != null) {
                MeshData substituteBarrelShape = SubstituteBlockShape(Api, tesselator, ShapeReferences.HorizontalBarrel, stack[0].Block);
                meshData.AddMeshData(substituteBarrelShape.BlockYRotation(this));
            }

            mesher.AddMeshData(meshData.Clone());
        }

        return true;
    }

    public override void GetBlockInfo(IPlayer forPlayer, StringBuilder sb) {
        base.GetBlockInfo(forPlayer, sb);

        switch(forPlayer.CurrentBlockSelection.SelectionBoxIndex) {
            case 1:
                sb.AppendLine(Lang.Get("foodshelves:Pour liquid into barrel."));
                break;
            case 2:
                sb.AppendLine(Lang.Get("foodshelves:Pour liquid into held container."));
                break;
            default: 
                break;
        }

        DisplayInfo(forPlayer, sb, inv, displaySelection, slotCount, segmentsPerShelf, itemsPerSegment);
    }
}

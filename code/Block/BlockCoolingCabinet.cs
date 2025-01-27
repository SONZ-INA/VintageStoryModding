using System.Linq;

namespace FoodShelves;

public class BlockCoolingCabinet : Block, IMultiBlockColSelBoxes {
    private WorldInteraction[] itemSlottableInteractions;
    private WorldInteraction[] cabinetInteractions;

    public override void OnLoaded(ICoreAPI api) {
        base.OnLoaded(api);

        itemSlottableInteractions = ObjectCacheUtil.GetOrCreate(api, "coolingCabinetItemInteractions", () => {
            List<ItemStack> holderUniversalStackList = new();

            foreach (var obj in api.World.Collectibles) {
                if (obj.Code == null) continue;
                
                if (obj.HolderUniversalCheck()) {
                    holderUniversalStackList.Add(new ItemStack(obj));
                }
            }

            return new WorldInteraction[] {
                new() {
                    ActionLangCode = "blockhelp-groundstorage-add",
                    MouseButton = EnumMouseButton.Right,
                    Itemstacks = holderUniversalStackList.ToArray()
                },
                new() {
                    ActionLangCode = "blockhelp-groundstorage-remove",
                    MouseButton = EnumMouseButton.Right,
                }
            };
        });

        cabinetInteractions = ObjectCacheUtil.GetOrCreate(api, "coolingCabinetBlockInteractions", () => {
            return new WorldInteraction[] {
                new() {
                    ActionLangCode = "blockhelp-door-openclose",
                    MouseButton = EnumMouseButton.Right,
                    HotKeyCode = "shift",
                }
            };
        });
    }

    public override WorldInteraction[] GetPlacedBlockInteractionHelp(IWorldAccessor world, BlockSelection selection, IPlayer forPlayer) {
        if (selection.SelectionBoxIndex > 8) return cabinetInteractions.Append(base.GetPlacedBlockInteractionHelp(world, selection, forPlayer));
        else return cabinetInteractions.Append(itemSlottableInteractions.Append(base.GetPlacedBlockInteractionHelp(world, selection, forPlayer)));
    }

    public override int GetRetention(BlockPos pos, BlockFacing facing, EnumRetentionType type) {
        return 0; // To prevent the block reducing the cellar rating
    }

    public override bool DoParticalSelection(IWorldAccessor world, BlockPos pos) {
        return true;
    }

    public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel) {
        if (world.BlockAccessor.GetBlockEntity(blockSel.Position) is BlockEntityCoolingCabinet becb) return becb.OnInteract(byPlayer, blockSel);
        return base.OnBlockInteractStart(world, byPlayer, blockSel);
    }

    public override string GetHeldItemName(ItemStack itemStack) {
        string variantName = itemStack.GetMaterialNameLocalized();
        return base.GetHeldItemName(itemStack) + " " + variantName;
    }

    // Selection box for master block
    public override Cuboidf[] GetSelectionBoxes(IBlockAccessor blockAccessor, BlockPos pos) {
        BlockEntityCoolingCabinet be = blockAccessor.GetBlockEntityExt<BlockEntityCoolingCabinet>(pos);
        if (be != null && be.CabinetOpen) {
            Cuboidf drawerSelBox = base.GetSelectionBoxes(blockAccessor, pos).ElementAt(9).Clone();
            Cuboidf bottomShelfL = base.GetSelectionBoxes(blockAccessor, pos).ElementAt(0).Clone();
            Cuboidf bottomShelfM = base.GetSelectionBoxes(blockAccessor, pos).ElementAt(1).Clone();
            Cuboidf skip = new(); // Skip selectionBox, to keep consistency between selectionBox indexes (1-8-shelves 9-drawer 10-cabinet)

            return new Cuboidf[] { bottomShelfL, bottomShelfM, skip, skip, skip, skip, skip, skip, skip, drawerSelBox };
        }

        return base.GetSelectionBoxes(blockAccessor, pos);
    }

    // Selection boxes for multiblock parts
    public Cuboidf[] MBGetSelectionBoxes(IBlockAccessor blockAccessor, BlockPos pos, Vec3i offset) {
        // Selection Box indexes:
        // Drawer - 9 / Cabinet - 10
        // Top    - 6 7 8
        // Middle - 3 4 5
        // Bottom - 0 1 2

        BlockEntityCoolingCabinet be = blockAccessor.GetBlockEntityExt<BlockEntityCoolingCabinet>(pos);
        if (be != null) {
            if (!be.CabinetOpen) {
                Cuboidf cabinetSelBox = base.GetSelectionBoxes(blockAccessor, pos).ElementAt(10).Clone();
                Cuboidf drawerSelBox = base.GetSelectionBoxes(blockAccessor, pos).ElementAt(9).Clone();
                Cuboidf skip = new(); // Skip selectionBox, to keep consistency between selectionBox indexes (1-8-shelves 9-drawer 10-cabinet)

                cabinetSelBox.MBNormalizeSelectionBox(offset);
                drawerSelBox.MBNormalizeSelectionBox(offset);

                return new Cuboidf[] { skip, skip, skip, skip, skip, skip, skip, skip, skip, drawerSelBox, cabinetSelBox};
            }
            else {
                List<Cuboidf> sBs = new();

                for (int i = 0; i < 10; i++) {
                    sBs.Add(base.GetSelectionBoxes(blockAccessor, pos).ElementAt(i).Clone());
                    sBs[i].MBNormalizeSelectionBox(offset);
                }
                sBs.Add(new Cuboidf()); // Skip selectionBox, to keep consistency between selectionBox indexes (1-8-shelves 9-drawer 10-cabinet)

                if (offset.Y != 0) {
                    return new Cuboidf[] { sBs[10], sBs[10], sBs[10], sBs[3], sBs[4], sBs[5], sBs[6], sBs[7], sBs[8] };
                }

                return new Cuboidf[] { sBs[10], sBs[1], sBs[2], sBs[10], sBs[10], sBs[10], sBs[10], sBs[10], sBs[10], sBs[9] };
            }
        }

        return base.GetSelectionBoxes(blockAccessor, pos);
    }

    public Cuboidf[] MBGetCollisionBoxes(IBlockAccessor blockAccessor, BlockPos pos, Vec3i offset) {
        return base.GetCollisionBoxes(blockAccessor, pos);
    }
}
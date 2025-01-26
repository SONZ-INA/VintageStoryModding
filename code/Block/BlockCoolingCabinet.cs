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
        if (selection.SelectionBoxIndex == 0 || selection.SelectionBoxIndex == 1) return cabinetInteractions.Append(base.GetPlacedBlockInteractionHelp(world, selection, forPlayer));
        else return cabinetInteractions.Append(itemSlottableInteractions.Append(base.GetPlacedBlockInteractionHelp(world, selection, forPlayer)));
    }

    public override int GetRetention(BlockPos pos, BlockFacing facing, EnumRetentionType type) {
        return 0; // To prevent the block reducing the cellar rating
    }

    public override bool DoParticalSelection(IWorldAccessor world, BlockPos pos) {
        return true;
    }

    public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel) {
        if (world.BlockAccessor.GetBlockEntity(blockSel.Position) is BlockEntityCoolingCabinet becb)
            return becb.OnInteract(byPlayer, blockSel);

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
            Cuboidf drawerSelBox = base.GetSelectionBoxes(blockAccessor, pos).ElementAt(1).Clone();
            Cuboidf bottomShelf = base.GetSelectionBoxes(blockAccessor, pos).ElementAt(2).Clone();
            Cuboidf skip = new(); // Skip selectionBox, to keep consistency between selectionBox indexes (0-cabinet 1-drawer 2,3,4-shelves)

            return new Cuboidf[] { skip, drawerSelBox, skip, skip, bottomShelf };
        }

        return base.GetSelectionBoxes(blockAccessor, pos);
    }

    // Selection boxes for multiblock parts
    public Cuboidf[] MBGetSelectionBoxes(IBlockAccessor blockAccessor, BlockPos pos, Vec3i offset) {
        BlockEntityCoolingCabinet be = blockAccessor.GetBlockEntityExt<BlockEntityCoolingCabinet>(pos);
        if (be != null) {
            if (!be.CabinetOpen) {
                Cuboidf cabinetSelBox = base.GetSelectionBoxes(blockAccessor, pos).ElementAt(0).Clone();
                Cuboidf drawerSelBox = base.GetSelectionBoxes(blockAccessor, pos).ElementAt(1).Clone();

                Cuboidf currentSelBox = offset.Y == 0 ? drawerSelBox : cabinetSelBox;
                currentSelBox.MBNormalizeSelectionBox(offset);

                // Bottom-right block fixing the "deadzone"
                if (offset.Z != offset.X && offset.Y == 0) {
                    cabinetSelBox.MBNormalizeSelectionBox(offset);
                    return new Cuboidf[] { cabinetSelBox, drawerSelBox };
                }

                return new Cuboidf[] { currentSelBox };
            }
            else {
                Cuboidf drawerSelBox = base.GetSelectionBoxes(blockAccessor, pos).ElementAt(1).Clone();
                Cuboidf bottomShelf = base.GetSelectionBoxes(blockAccessor, pos).ElementAt(2).Clone();
                Cuboidf middleShelf = base.GetSelectionBoxes(blockAccessor, pos).ElementAt(3).Clone();
                Cuboidf topShelf = base.GetSelectionBoxes(blockAccessor, pos).ElementAt(4).Clone();
                Cuboidf skip = new(); // Skip selectionBox, to keep consistency between selectionBox indexes (0-cabinet 1-drawer 2,3,4-shelves)

                bottomShelf.MBNormalizeSelectionBox(offset);
                middleShelf.MBNormalizeSelectionBox(offset);
                topShelf.MBNormalizeSelectionBox(offset);
                drawerSelBox.MBNormalizeSelectionBox(offset);

                if (offset.Y != 0) {
                    return new Cuboidf[] { skip, skip, topShelf, middleShelf };
                }

                return new Cuboidf[] { skip, drawerSelBox, skip, middleShelf, bottomShelf };
            }
        }

        return base.GetSelectionBoxes(blockAccessor, pos);
    }

    public Cuboidf[] MBGetCollisionBoxes(IBlockAccessor blockAccessor, BlockPos pos, Vec3i offset) {
        return base.GetCollisionBoxes(blockAccessor, pos);
    }
}
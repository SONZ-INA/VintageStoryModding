namespace FoodShelves;

public class BlockBarrelRackBig : BlockLiquidContainerBase, IMultiBlockColSelBoxes {
    public override bool AllowHeldLiquidTransfer => false;
    public override int GetContainerSlotId(BlockPos pos) => 1;
    public override int GetContainerSlotId(ItemStack containerStack) => 1;

    public override void OnLoaded(ICoreAPI api) {
        base.OnLoaded(api);
        PlacedPriorityInteract = true; // Needed to call OnBlockInteractStart when shifting with an item in hand
    }

    public override bool DoParticalSelection(IWorldAccessor world, BlockPos pos) {
        return true;
    }

    public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel) {
        if (world.BlockAccessor.GetBlockEntity(blockSel.Position) is BlockEntityBarrelRackBig hbrb) return hbrb.OnInteract(byPlayer, blockSel);
        return base.OnBlockInteractStart(world, byPlayer, blockSel);
    }

    public override WorldInteraction[] GetPlacedBlockInteractionHelp(IWorldAccessor world, BlockSelection selection, IPlayer forPlayer) {
        return null;
    }

    public override void OnBlockBroken(IWorldAccessor world, BlockPos pos, IPlayer byPlayer, float dropQuantityMultiplier = 1) {
        bool preventDefault = false;
        foreach (BlockBehavior behavior in BlockBehaviors) {
            EnumHandling handled = EnumHandling.PassThrough;

            behavior.OnBlockBroken(world, pos, byPlayer, ref handled);
            if (handled == EnumHandling.PreventDefault) preventDefault = true;
            if (handled == EnumHandling.PreventSubsequent) return;
        }

        if (preventDefault) return;

        if (world.Side == EnumAppSide.Server && (byPlayer == null || byPlayer.WorldData.CurrentGameMode != EnumGameMode.Creative)) {
            ItemStack[] drops = new ItemStack[] { new(this) };

            for (int i = 0; i < drops.Length; i++) {
                world.SpawnItemEntity(drops[i], new Vec3d(pos.X + 0.5, pos.Y + 0.5, pos.Z + 0.5), null);
            }

            world.PlaySoundAt(Sounds.GetBreakSound(byPlayer), pos.X, pos.Y, pos.Z, byPlayer);
        }

        if (EntityClass != null) {
            BlockEntity entity = world.BlockAccessor.GetBlockEntity(pos);
            entity?.OnBlockBroken();
        }

        world.BlockAccessor.SetBlock(0, pos);
    }

    // Selection box for master block
    public override Cuboidf[] GetSelectionBoxes(IBlockAccessor blockAccessor, BlockPos pos) {
        BlockEntityBarrelRackBig be = blockAccessor.GetBlockEntity<BlockEntityBarrelRackBig>(pos);
        if (be != null) {
            int[] transformedIndex = GetMultiblockIndex(new Vec3i() { X = 0, Y = 0, Z = 0 }, be);
            Cuboidf singleSelectionBox = new(
                transformedIndex[0],
                transformedIndex[1],
                transformedIndex[2] - 1,
                transformedIndex[0] + 2,
                transformedIndex[1] + 2,
                transformedIndex[2] + 1
            );

            return new Cuboidf[] { singleSelectionBox };
        }

        return base.GetSelectionBoxes(blockAccessor, pos);
    }

    // Selection boxes for multiblock parts
    public Cuboidf[] MBGetSelectionBoxes(IBlockAccessor blockAccessor, BlockPos pos, Vec3i offset) {
        BlockEntityBarrelRackBig be = blockAccessor.GetBlockEntityExt<BlockEntityBarrelRackBig>(pos);
        if (be != null) {
            int[] transformedIndex = GetMultiblockIndex(offset, be);

            Cuboidf singleSelectionBox = new(
                transformedIndex[0],
                transformedIndex[1],
                transformedIndex[2] - 1,
                transformedIndex[0] + 2,
                transformedIndex[1] + 2,
                transformedIndex[2] + 1
            );

            return new Cuboidf[] { singleSelectionBox };
        }

        return base.GetSelectionBoxes(blockAccessor, pos);
    }

    //public override Cuboidf[] GetCollisionBoxes(IBlockAccessor blockAccessor, BlockPos pos) {
    //    Block block = blockAccessor.GetBlock(pos);
    //    if (block.Code.Path.StartsWith("barrelrackbig-top-")) {
    //        if (blockAccessor.GetBlockEntity(pos) is BlockEntityBarrelRackBig be && be.Inventory.Empty) {
    //            return new Cuboidf[] { new(0, 0, 0, 1f, 0.3f, 1f) };
    //        }
    //    }

    //    return base.GetCollisionBoxes(blockAccessor, pos);
    //}

    public Cuboidf[] MBGetCollisionBoxes(IBlockAccessor blockAccessor, BlockPos pos, Vec3i offset) {
        Block block = blockAccessor.GetBlock(pos);
        if (block.Code.Path.StartsWith("barrelrackbig-top-")) {
            if (blockAccessor.GetBlockEntity(pos) is BlockEntityBarrelRackBig be && be.Inventory.Empty) {
                return new Cuboidf[] { new(0, 0, 0, 1f, 0.3f, 1f) };
            }
        }

        return base.GetCollisionBoxes(blockAccessor, pos);
    }

    public override string GetPlacedBlockInfo(IWorldAccessor world, BlockPos pos, IPlayer forPlayer) {
        StringBuilder dsc = new();

        switch (forPlayer.CurrentBlockSelection.SelectionBoxIndex) {
            case 1:
                dsc.AppendLine(Lang.Get("foodshelves:Pour liquid into barrel."));
                break;
            case 2:
                dsc.AppendLine(Lang.Get("foodshelves:Pour liquid into held container."));
                break;
            default:
                break;
        }

        dsc.AppendLine();

        if (forPlayer.CurrentBlockSelection.Block.GetSelectionBoxes(world.BlockAccessor, pos).Length == 1) {
            dsc.AppendLine(Lang.Get("foodshelves:Missing barrel."));
        }
        else {
            dsc.Append(base.GetPlacedBlockInfo(world, pos, forPlayer));
        }

        return dsc.ToString();
    }
}

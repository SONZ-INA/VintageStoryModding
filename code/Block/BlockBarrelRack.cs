using System.Linq;
using static FoodShelves.BlockBounds;

namespace FoodShelves;

public class BlockBarrelRack : BlockLiquidContainerBase {
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
        if (world.BlockAccessor.GetBlockEntity(blockSel.Position) is BlockEntityBarrelRack hbr) return hbr.OnInteract(byPlayer, blockSel);
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

    public override Cuboidf[] GetCollisionBoxes(IBlockAccessor blockAccessor, BlockPos pos) {
        Block block = blockAccessor.GetBlock(pos);
        if (block.Code.Path.StartsWith("barrelrack-top-")) {
            if (blockAccessor.GetBlockEntity(pos) is BlockEntityBarrelRack be && be.Inventory.Empty) {
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

    public override Cuboidf[] GetSelectionBoxes(IBlockAccessor blockAccessor, BlockPos pos) {
        Block block = blockAccessor.GetBlock(pos);
        if (blockAccessor.GetBlockEntity(pos) is BlockEntityBarrelRack be && !be.Inventory.Empty) {
            Cuboidf[] selectionBoxes = SelectionBoxReferences.BarrelRackCuboids.Values.ToArray();

            int rotationAngle = GetRotationAngle(block);

            for (int i = 0; i < selectionBoxes.Length; i++) {
                selectionBoxes[i] = RotateCuboid90Deg(selectionBoxes[i], rotationAngle);
            }

            return selectionBoxes;
        }

        return base.GetSelectionBoxes(blockAccessor, pos);
    }
}

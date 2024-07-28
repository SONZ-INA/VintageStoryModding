namespace FoodShelves;

public class BlockHorizontalBarrelRack : Block {
    public override void OnLoaded(ICoreAPI api) {
        base.OnLoaded(api);
    }

    public override bool DoParticalSelection(IWorldAccessor world, BlockPos pos) {
        return true;
    }

    public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel) {
        if (world.BlockAccessor.GetBlockEntity(blockSel.Position) is BlockEntityHorizontalBarrelRack hbr) return hbr.OnInteract(byPlayer, blockSel);
        return base.OnBlockInteractStart(world, byPlayer, blockSel);
    }

    public override Cuboidf[] GetCollisionBoxes(IBlockAccessor blockAccessor, BlockPos pos) {
        Block block = blockAccessor.GetBlock(pos);
        if (block.Code.Path == "horizontalbarrelracktop-normal-west") {
            if (blockAccessor.GetBlockEntity(pos) is BlockEntityHorizontalBarrelRack be && be.Inventory.Empty) {
                return new Cuboidf[] { new(0, 0, 0, 1f, 0.3f, 1f) };
            }
        }

        return base.GetCollisionBoxes(blockAccessor, pos);
    }

    public override Cuboidf[] GetSelectionBoxes(IBlockAccessor blockAccessor, BlockPos pos) {
        Block block = blockAccessor.GetBlock(pos);
        if (blockAccessor.GetBlockEntity(pos) is BlockEntityHorizontalBarrelRack be && !be.Inventory.Empty) {
            Cuboidf[] selectionBoxes = new Cuboidf[] {
                new(0.01f, 0, 0, 1f, 0.99f, 1f),
                new(0.4f, 0.9f, 0.4f, 0.6f, 1f, 0.6f),
                new(0f, 0.1f, 0.4f, 0.1f, 0.2f, 0.6f)
            };

            int rotationAngle = GetRotationAngle(block);

            for (int i = 0; i < selectionBoxes.Length; i++) {
                selectionBoxes[i] = RotateCuboid(selectionBoxes[i], rotationAngle);
            }

            return selectionBoxes;
        }

        return base.GetSelectionBoxes(blockAccessor, pos);
    }

    private int GetRotationAngle(Block block) {
        string blockPath = block.Code.Path;
        if (blockPath.EndsWith("-north")) return 270;
        if (blockPath.EndsWith("-south")) return 90;
        if (blockPath.EndsWith("-east")) return 0;
        if (blockPath.EndsWith("-west")) return 180;
        return 0;
    }

    private Cuboidf RotateCuboid(Cuboidf cuboid, int angle) {
        if (angle == 0) {
            return cuboid;
        }

        float x1 = cuboid.X1;
        float y1 = cuboid.Y1;
        float z1 = cuboid.Z1;
        float x2 = cuboid.X2;
        float y2 = cuboid.Y2;
        float z2 = cuboid.Z2;

        return angle switch {
            90 => new Cuboidf(1 - z2, y1, x1, 1 - z1, y2, x2),
            180 => new Cuboidf(1 - x2, y1, 1 - z2, 1 - x1, y2, 1 - z1),
            270 => new Cuboidf(z1, y1, 1 - x2, z2, y2, 1 - x1),
            _ => throw new ArgumentException("Angle must be 0, 90, 180, or 270 degrees"),
        };
    }
}

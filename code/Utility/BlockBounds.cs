namespace FoodShelves;

public static class BlockBounds {
    public static int GetRotationAngle(Block block) {
        string blockPath = block.Code.Path;
        if (blockPath.EndsWith("-north")) return 270;
        if (blockPath.EndsWith("-south")) return 90;
        if (blockPath.EndsWith("-east")) return 0;
        if (blockPath.EndsWith("-west")) return 180;
        return 0;
    }

    public static Cuboidf RotateCuboid90Deg(Cuboidf cuboid, int angle) {
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


using System.Linq;

namespace FoodShelves; 

public static class Extensions {
    public static void EnsureAttributesNotNull(this CollectibleObject obj) => obj.Attributes ??= new JsonObject(new JObject());

    public static ModelTransform GetTransformation(this CollectibleObject obj, Dictionary<string, ModelTransform> transformations) {
        foreach (KeyValuePair<string, ModelTransform> transformation in transformations) {
            if (WildcardUtil.Match(transformation.Key, obj.Code.Path.ToString())) return transformation.Value;
        }

        return null;
    }

    public static ModelTransform GetFullTransformation(this CollectibleObject obj, Dictionary<string, ModelTransform> transformations) {
        foreach (KeyValuePair<string, ModelTransform> transformation in transformations) {
            if (WildcardUtil.Match(transformation.Key, obj.Code.ToString())) return transformation.Value;
        }

        return null;
    }

    public static MeshData BlockYRotation(this MeshData obj, BlockEntity BE) {
        Block block = BE.Api.World.BlockAccessor.GetBlock(BE.Pos);
        return obj.Rotate(new Vec3f(0.5f, 0.5f, 0.5f), 0, block.Shape.rotateY * GameMath.DEG2RAD, 0);
    }

    public static T GetBlockEntityExt<T>(this IBlockAccessor blockAccessor, BlockPos pos) where T : BlockEntity {
        if (blockAccessor.GetBlockEntity<T>(pos) is T blockEntity) {
            return blockEntity;
        }

        if (blockAccessor.GetBlock(pos) is BlockMultiblock multiblock) {
            BlockPos multiblockPos = new BlockPos(pos.X + multiblock.OffsetInv.X, pos.Y + multiblock.OffsetInv.Y, pos.Z + multiblock.OffsetInv.Z, pos.dimension);

            return blockAccessor.GetBlockEntity<T>(multiblockPos);
        }

        return null;
    }

    private static char GetFacingFromBlockCode(BlockEntity block) {
        string codePath = block.Block.Code.ToString();
        if (codePath == null) return 'n';

        string[] parts = codePath.Split('-');
        string facingStr = parts.Last().ToLowerInvariant();

        return facingStr switch {
            "north" => 'n',
            "east" => 'e',
            "south" => 's',
            "west" => 'w',
            _ => 'n'
        };
    }

    public static int[] GetMultiblockIndex(Vec3i offset, BlockEntity block) {
        char facing = GetFacingFromBlockCode(block);

        int transformedX = offset.X;
        int transformedY = offset.Y;
        int transformedZ = offset.Z;

        switch (facing) {
            case 'n':
                break; // No change needed for North
            case 's':
                transformedX -= 1;
                transformedZ += 1;
                break;
            case 'e':
                transformedZ += 1;
                break;
            case 'w':
                transformedX -= 1;
                break;
        }

        return new int[3] { transformedX, transformedY, transformedZ };
    }
}

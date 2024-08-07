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
}

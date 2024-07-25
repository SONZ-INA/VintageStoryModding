namespace FoodShelves; 

public static class Extensions {
    public static void EnsureAttributesNotNull(this CollectibleObject obj) => obj.Attributes ??= new JsonObject(new JObject());

    public static ModelTransform GetTransformation(this CollectibleObject obj, Dictionary<string, ModelTransform> transformations) {
        foreach (KeyValuePair<string, ModelTransform> transformation in transformations) {
            if (WildcardUtil.Match(transformation.Key, obj.Code.Path.ToString())) return transformation.Value;
        }

        return null;
    }
}

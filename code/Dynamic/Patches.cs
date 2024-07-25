namespace FoodShelves;

public static class Patches {
    private static readonly Transformations Transformations = new();

    public static void PatchPieShelf(CollectibleObject obj) {
        if (PieShelfTypes.Contains(obj.GetType()) || WildcardUtil.Match(PieShelfCodes, obj.Code.Path.ToString())) {
            obj.EnsureAttributesNotNull();
            obj.Attributes.Token[PieShelf] = JToken.FromObject(true);

            ModelTransform transformation = obj.GetTransformation(Transformations.PieShelfTransformations);
            if (transformation != null) {
                obj.Attributes.Token[onPieShelfTransform] = JToken.FromObject(transformation);
            }
        }
    }

    public static void PatchBreadShelf(CollectibleObject obj) {
        if (WildcardUtil.Match(BreadShelfCodes, obj.Code.Path.ToString())) {
            obj.EnsureAttributesNotNull();
            obj.Attributes.Token[BreadShelf] = JToken.FromObject(true);

            ModelTransform transformation = obj.GetTransformation(Transformations.BreadShelfTransformations);
            if (transformation != null) {
                obj.Attributes.Token[onBreadShelfTransform] = JToken.FromObject(transformation);
            }
        }
    }

    public static void PatchBarShelf(CollectibleObject obj) {
        if (WildcardUtil.Match(BarShelfCodes, obj.Code.Path.ToString())) {
            obj.EnsureAttributesNotNull();
            obj.Attributes.Token[BarShelf] = JToken.FromObject(true);

            ModelTransform transformation = obj.GetTransformation(Transformations.BarShelfTransformations);
            if (transformation != null) {
                obj.Attributes.Token[onBarShelfTransform] = JToken.FromObject(transformation);
            }
        }
    }

    public static void PatchSushiShelf(CollectibleObject obj) {
        if (WildcardUtil.Match(SushiShelfCodes, obj.Code.Path.ToString())) {
            obj.EnsureAttributesNotNull();
            obj.Attributes.Token[SushiShelf] = JToken.FromObject(true);
        }
    }

    public static void PatchEggShelf(CollectibleObject obj) {
        if (WildcardUtil.Match(EggShelfCodes, obj.Code.Path.ToString())) {
            obj.EnsureAttributesNotNull();
            obj.Attributes.Token[EggShelf] = JToken.FromObject(true);
        }
    }

    public static void PatchFruitBasket(CollectibleObject obj) {
        if (WildcardUtil.Match(FruitBasketCodes, obj.Code.Path.ToString())) {
            obj.EnsureAttributesNotNull();
            obj.Attributes.Token[FruitBasket] = JToken.FromObject(true);
        }
    }

    public static void PatchSeedShelf(CollectibleObject obj) {
        if (WildcardUtil.Match(SeedShelfCodes, obj.Code.Path.ToString())) {
            obj.EnsureAttributesNotNull();
            obj.Attributes.Token[SeedShelf] = JToken.FromObject(true);
        }
    }
}
using static FoodShelves.RestrictionData;

namespace FoodShelves;

public static class Patches {
    #region Generic

    public static void PatchFoodUniversal(CollectibleObject obj, FoodUniversalData data) {
        if (data.FoodUniversalTypes.Contains(obj.GetType().Name) || WildcardUtil.Match(data.FoodUniversalCodes, obj.Code.Path.ToString())) {
            obj.EnsureAttributesNotNull();
            obj.Attributes.Token[FoodUniversal] = JToken.FromObject(true);
        }

        ModelTransform transformation = obj.GetTransformation(FoodUniversalTransformations);
        if (transformation != null) {
            obj.Attributes.Token[onGlassFoodBlockTransform] = JToken.FromObject(transformation);
            obj.Attributes.Token[onGlassFoodCaseTransform] = JToken.FromObject(transformation);
        }
    }

    public static void PatchLiquidyStuff(CollectibleObject obj, LiquidyStuffData data) {
        if (data.LiquidyStuffTypes.Contains(obj.GetType().Name) || WildcardUtil.Match(data.LiquidyStuffCodes, obj.Code.Path.ToString())) {
            obj.EnsureAttributesNotNull();
            obj.Attributes.Token[LiquidyStuff] = JToken.FromObject(true);
        }
    }

    #endregion

    #region Shelves

    public static void PatchPieShelf(CollectibleObject obj, PieShelfData data) {
        if (data.PieShelfTypes.Contains(obj.GetType().Name) || WildcardUtil.Match(data.PieShelfCodes, obj.Code.Path.ToString())) {
            obj.EnsureAttributesNotNull();
            obj.Attributes.Token[PieShelf] = JToken.FromObject(true);

            ModelTransform transformation = obj.GetTransformation(PieShelfTransformations);
            if (transformation != null) {
                obj.Attributes.Token[onPieShelfTransform] = JToken.FromObject(transformation);
            }
        }
    }

    public static void PatchBreadShelf(CollectibleObject obj, BreadShelfData data) {
        if (data.BreadShelfTypes.Contains(obj.GetType().Name) || WildcardUtil.Match(data.BreadShelfCodes, obj.Code.Path.ToString())) {
            obj.EnsureAttributesNotNull();
            obj.Attributes.Token[BreadShelf] = JToken.FromObject(true);

            ModelTransform transformation = obj.GetTransformation(BreadShelfTransformations);
            if (transformation != null) {
                obj.Attributes.Token[onBreadShelfTransform] = JToken.FromObject(transformation);
            }
        }
    }

    public static void PatchBarShelf(CollectibleObject obj, BarShelfData data) {
        if (data.BarShelfTypes.Contains(obj.GetType().Name) || WildcardUtil.Match(data.BarShelfCodes, obj.Code.Path.ToString())) {
            obj.EnsureAttributesNotNull();
            obj.Attributes.Token[BarShelf] = JToken.FromObject(true);

            ModelTransform transformation = obj.GetTransformation(BarShelfTransformations);
            if (transformation != null) {
                obj.Attributes.Token[onBarShelfTransform] = JToken.FromObject(transformation);
            }
        }
    }

    public static void PatchSushiShelf(CollectibleObject obj, SushiShelfData data) {
        if (data.SushiShelfTypes.Contains(obj.GetType().Name) || WildcardUtil.Match(data.SushiShelfCodes, obj.Code.Path.ToString())) {
            obj.EnsureAttributesNotNull();
            obj.Attributes.Token[SushiShelf] = JToken.FromObject(true);
        }
    }

    public static void PatchEggShelf(CollectibleObject obj, EggShelfData data) {
        if (data.EggShelfTypes.Contains(obj.GetType().Name) || WildcardUtil.Match(data.EggShelfCodes, obj.Code.Path.ToString())) {
            obj.EnsureAttributesNotNull();
            obj.Attributes.Token[EggShelf] = JToken.FromObject(true);
        }
    }

    public static void PatchSeedShelf(CollectibleObject obj, SeedShelfData data) {
        if (data.SeedShelfTypes.Contains(obj.GetType().Name) || WildcardUtil.Match(data.SeedShelfCodes, obj.Code.Path.ToString())) {
            obj.EnsureAttributesNotNull();
            obj.Attributes.Token[SeedShelf] = JToken.FromObject(true);
        }
    }

    public static void PatchGlassJarShelf(CollectibleObject obj, GlassJarShelfData data) {
        if (data.GlassJarShelfTypes.Contains(obj.GetType().Name) || WildcardUtil.Match(data.GlassJarShelfCodes, obj.Code.Path.ToString())) {
            obj.EnsureAttributesNotNull();
            obj.Attributes.Token[GlassJarShelf] = JToken.FromObject(true);
        }
    }

    #endregion

    #region Baskets

    public static void PatchFruitBasket(CollectibleObject obj, FruitBasketData data) {
        if (data.FruitBasketTypes.Contains(obj.GetType().Name) || WildcardUtil.Match(data.FruitBasketCodes, obj.Code.Path.ToString())) {
            obj.EnsureAttributesNotNull();
            obj.Attributes.Token[FruitBasket] = JToken.FromObject(true);

            ModelTransform transformation = obj.GetTransformation(FruitBasketTransformations);
            if (transformation != null) {
                obj.Attributes.Token[onFruitBasketTransform] = JToken.FromObject(transformation);
            }
        }
    }

    public static void PatchVegetableBasket(CollectibleObject obj, VegetableBasketData data) {
        if (WildcardUtil.Match(data.VegetableBasketCodes, obj.Code.Path.ToString())) {
            obj.EnsureAttributesNotNull();
            obj.Attributes.Token[VegetableBasket] = JToken.FromObject(true);

            ModelTransform transformation = obj.GetTransformation(VegetableBasketTransformations);
            if (transformation != null) {
                obj.Attributes.Token[onVegetableBasketTransform] = JToken.FromObject(transformation);
            }
        }
    }

    public static void PatchEggBasket(CollectibleObject obj, EggBasketData data) {
        if (data.EggBasketTypes.Contains(obj.GetType().Name) || WildcardUtil.Match(data.EggBasketCodes, obj.Code.Path.ToString())) {
            obj.EnsureAttributesNotNull();
            obj.Attributes.Token[EggBasket] = JToken.FromObject(true);
        }
    }

    #endregion

    #region Barrels

    public static void PatchBarrelRack(CollectibleObject obj, BarrelRackData data) {
        if (data.BarrelRackTypes.Contains(obj.GetType().Name) || WildcardUtil.Match(data.BarrelRackCodes, obj.Code.Path.ToString())) {
            obj.EnsureAttributesNotNull();
            obj.Attributes.Token[BarrelRack] = JToken.FromObject(true);
        }
    }

    public static void PatchBarrelRackBig(CollectibleObject obj, BarrelRackBigData data) {
        if (data.BarrelRackBigTypes.Contains(obj.GetType().Name) || WildcardUtil.Match(data.BarrelRackBigCodes, obj.Code.Path.ToString())) {
            obj.EnsureAttributesNotNull();
            obj.Attributes.Token[BarrelRackBig] = JToken.FromObject(true);
        }
    }

    public static void PatchFirkinRack(CollectibleObject obj, FirkinRackData data) {
        if (data.FirkinRackTypes.Contains(obj.GetType().Name) || WildcardUtil.Match(data.FirkinRackCodes, obj.Code.Path.ToString())) {
            obj.EnsureAttributesNotNull();
            obj.Attributes.Token[FirkinRack] = JToken.FromObject(true);
        }
    }

    #endregion

    public static void PatchPumpkinCase(CollectibleObject obj, PumpkinCaseData data) {
        if (data.PumpkinCaseTypes.Contains(obj.GetType().Name) || WildcardUtil.Match(data.PumpkinCaseCodes, obj.Code.Path.ToString())) {
            obj.EnsureAttributesNotNull();
            obj.Attributes.Token[PumpkinCase] = JToken.FromObject(true);
        }
    }
}
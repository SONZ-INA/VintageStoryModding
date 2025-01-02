namespace FoodShelves;

public class TransformationsCollection {
    public const string onDisplayTransform = "onDisplayTransform"; // Vanilla shelf
    public const string onPieShelfTransform = "onPieShelfTransform";
    public const string onBreadShelfTransform = "onBreadShelfTransform";
    public const string onBarShelfTransform = "onBarShelfTransform";
    public const string onSushiShelfTransform = "onSushiShelfTransform";
    public const string onEggShelfTransform = "onEggShelfTransform";
    public const string onSeedShelfTransform = "onSeedShelfTransform";
    public const string onTableWShelfTransform = "onTableWShelfTransform";
    public const string onFruitBasketTransform = "onFruitBasketTransform";
    public const string onVegetableBasketTransform = "onVegetableBasketTransform";
    public const string onBarrelRackTransform = "onHorizontalBarrelRackTransform";
    public const string onGlassFoodBlockTransform = "onGlassFoodBlockTransform";
    public const string onGlassFoodCaseTransform = "onGlassFoodCaseTransform";
    public const string onFridgeTransform = "onFridgeTransform";

    public static Dictionary<string, ModelTransform> FoodUniversalTransformations { get; set; } = new();
    public static Dictionary<string, ModelTransform> HolderUniversalTransformations { get; set; } = new();

    public static Dictionary<string, ModelTransform> PieShelfTransformations { get; set; } = new();
    public static Dictionary<string, ModelTransform> BreadShelfTransformations { get; set; } = new();
    public static Dictionary<string, ModelTransform> BarShelfTransformations { get; set; } = new();

    public static Dictionary<string, ModelTransform> FruitBasketTransformations { get; set; } = new();
    public static Dictionary<string, ModelTransform> VegetableBasketTransformations { get; set; } = new();
}
using static FoodShelves.RestrictionData;

namespace FoodShelves;

public static class RestrictionsCollection {
    public static FoodUniversalData FoodUniversalData { get; set; } = new();
    public static LiquidyStuffData LiquidyStuffData { get; set; } = new();

    public static PieShelfData PieShelfData { get; set; } = new();
    public static BreadShelfData BreadShelfData { get; set; } = new();
    public static BarShelfData BarShelfData { get; set; } = new();
    public static SushiShelfData SushiShelfData { get; set; } = new();
    public static EggShelfData EggShelfData { get; set; } = new();
    public static SeedShelfData SeedShelfData { get; set; } = new();
    public static GlassJarShelfData GlassJarShelfData { get; set; } = new();

    public static FruitBasketData FruitBasketData { get; set; } = new();
    public static VegetableBasketData VegetableBasketData { get; set; } = new();
    public static EggBasketData EggBasketData { get; set; } = new();

    public static BarrelRackData BarrelRackData { get; set; } = new();
    public static BarrelRackBigData BarrelRackBigData { get; set; } = new();

    public static PumpkinCaseData PumpkinCaseData { get; set; } = new();
}

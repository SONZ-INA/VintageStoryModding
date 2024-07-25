namespace FoodShelves;

public static class Restrictions {
    #region Shelveable

    public const string Shelvable = "shelvable";

    public static bool ShelvableCheck(this CollectibleObject obj) => obj?.Attributes?[Shelvable].AsBool() == true;
    public static bool ShelvableCheck(this ItemSlot slot) => slot?.Itemstack?.Collectible?.Attributes?[Shelvable].AsBool() == true;

    #endregion 

    #region PieShelf

    public const string PieShelf = "pieshelfcheck";

    public static bool PieShelfCheck(this CollectibleObject obj) => obj?.Attributes?[PieShelf].AsBool() == true;
    public static bool PieShelfCheck(this ItemSlot slot) => slot?.Itemstack?.Collectible?.Attributes?[PieShelf].AsBool() == true;

    public static readonly Type[] PieShelfTypes = new Type[] {
        typeof(BlockPie)
    };

    public static readonly string[] PieShelfCodes = new string[] {
        "*cheese-*"
    };

    #endregion

    #region BreadShelf

    public const string BreadShelf = "breadshelfcheck";

    public static bool BreadShelfCheck(this CollectibleObject obj) => obj?.Attributes?[BreadShelf].AsBool() == true;
    public static bool BreadShelfCheck(this ItemSlot slot) => slot?.Itemstack?.Collectible?.Attributes?[BreadShelf].AsBool() == true;

    public static readonly string[] BreadShelfCodes = new string[] {
        "*bread-*",
        "*dough-*",

        // Expanded foods
        "*muffin-*",
        "*breadedball-*",
        "*dumpling-*", 
        "*doughball-*",

        // Wildcraft: Fruits and Nuts
        "*halva",
        "*pacoca",
        "*marzipan"
    };

    #endregion

    #region BarShelf

    public const string BarShelf = "barshelfcheck";

    public static bool BarShelfCheck(this CollectibleObject obj) => obj?.Attributes[BarShelf].AsBool() == true;
    public static bool BarShelfCheck(this ItemSlot slot) => slot?.Itemstack?.Collectible?.Attributes?[BarShelf].AsBool() == true;

    public static readonly string[] BarShelfCodes = new string[] {
        "*fruitbar-*", // Expanded foods
        "*bar" // Wildcraft: Fruits and Nuts
    };

    #endregion

    #region SushiShelf

    public const string SushiShelf = "sushishelfcheck";

    public static bool SushiShelfCheck(this CollectibleObject obj) => obj?.Attributes?[SushiShelf].AsBool() == true;
    public static bool SushiShelfCheck(this ItemSlot slot) => slot?.Itemstack?.Collectible?.Attributes?[SushiShelf].AsBool() == true;

    public static readonly string[] SushiShelfCodes = new string[] {
        // Expanded foods
        "*sushi-*",
        "*sushiveg-*"
    };

    #endregion

    #region TableWShelf

    public const string TableWShelf = "tablewshelfcheck";

    public static bool TableWShelfCheck(this CollectibleObject obj) => obj?.Attributes?[TableWShelf].AsBool() == true;
    public static bool TableWShelfCheck(this ItemSlot slot) => slot?.Itemstack?.Collectible?.Attributes?[TableWShelf].AsBool() == true;

    public static readonly string[] TableWShelfCodes = new string[] {
        // Expanded foods
        "*claypot-*",
        "*crock-*",
        "*bowl-*"
    };

    #endregion

    #region EggShelf

    public const string EggShelf = "eggshelfcheck";

    public static bool EggShelfCheck(this CollectibleObject obj) => obj?.Attributes?[EggShelf].AsBool() == true;
    public static bool EggShelfCheck(this ItemSlot slot) => slot?.Itemstack?.Collectible?.Attributes?[EggShelf].AsBool() == true;

    public static readonly string[] EggShelfCodes = new string[] {
        "*egg-chicken-raw",
        "*egg-chicken-boiled"
    };

    #endregion

    #region FruitBasket

    public const string FruitBasket = "fruitbasketcheck";

    public static bool FruitBasketCheck(this CollectibleObject obj) => obj?.Attributes?[FruitBasket].AsBool() == true;
    public static bool FruitBasketCheck(this ItemSlot slot) => slot?.Itemstack?.Collectible?.Attributes?[FruitBasket].AsBool() == true;

    public static readonly string[] FruitBasketCodes = new string[] {
        "*fruit-*"
    };

    #endregion

    #region SeedShelf

    public const string SeedShelf = "seedshelfcheck";

    public static bool SeedShelfCheck(this CollectibleObject obj) => obj?.Attributes?[SeedShelf].AsBool() == true;
    public static bool SeedShelfCheck(this ItemSlot slot) => slot?.Itemstack?.Collectible?.Attributes?[SeedShelf].AsBool() == true;

    public static readonly string[] SeedShelfCodes = new string[] {
        "*seeds-*"
    };

    #endregion
}

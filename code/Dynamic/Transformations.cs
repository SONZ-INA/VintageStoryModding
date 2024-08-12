namespace FoodShelves;

public class Transformations {
    public const string onDisplayTransform = "onDisplayTransform"; // Vanilla shelf
    public const string onPieShelfTransform = "onPieShelfTransform";
    public const string onBreadShelfTransform = "onBreadShelfTransform";
    public const string onBarShelfTransform = "onBarShelfTransform";
    public const string onSushiShelfTransform = "onSushiShelfTransform";
    public const string onTableWShelfTransform = "onTableWShelfTransform";
    public const string onEggShelfTransform = "onEggShelfTransform";
    public const string onFruitBasketTransform = "onFruitBasketTransform";
    public const string onSeedShelfTransform = "onSeedShelfTransform";
    public const string onHorizontalBarrelRackTransform = "onHorizontalBarrelRackTransform";
    public const string onVegetableBasketTransform = "onVegetableBasketTransform";

    #region PieShelf

    public Dictionary<string, ModelTransform> PieShelfTransformations = new() {
        { "*cheese-*", new ModelTransform() {
            Origin = new() { X = 0.5f, Y = 0f, Z = 0.5f },
            Scale = 0.8f
        }}
    };

    #endregion

    #region BreadShelf

    public Dictionary<string, ModelTransform> BreadShelfTransformations = new() {
        { "*muffin-*", new ModelTransform() {
            Origin = new() { X = 0.48f, Y = 0f, Z = 0.4875f },
            Rotation = new() { X = 0f, Y = 90f, Z = 0f },
            Scale = 0.7f
        }},
        { "*dumpling-*", new ModelTransform() {
            Origin = new() { X = 0.38f, Y = 0f, Z = 0.4875f },
            Scale = 0.8f
        }},
        { "*doughball-*", new ModelTransform() {
            Origin = new() { X = 0.5f, Y = 0f, Z = 0.5f },
            Rotation = new() { X = 0f, Y = 45f, Z = 0f },
            Scale = 0.9f
        }},
        { "*breadedball-*", new ModelTransform() {
            Origin = new() { X = 0.5f, Y = 0f, Z = 0.5f },
            Rotation = new() { X = 0f, Y = -65f, Z = 0f },
            Scale = 0.45f
        }},
        { "*halva", new ModelTransform() {
            Origin = new() { X = 0.5f, Y = 0f, Z = 0.5f },
            Rotation = new() { X = 0f, Y = 35f, Z = 0f },
            Scale = 0.7f
        }},
        { "*pacoca", new ModelTransform() {
            Origin = new() { X = 0.5f, Y = 0f, Z = 0.47f },
            Rotation = new() { X = 0f, Y = -45f, Z = 0f },
            Scale = 0.65f
        }},
        { "*marzipan", new ModelTransform() {
            Origin = new() { X = 0.5f, Y = 0f, Z = 0.47f },
            ScaleXYZ = new() { X = 0.7f, Y = 0.7f, Z = 0.58f }
        }},
    };

    #endregion

    #region BarShelf

    public Dictionary<string, ModelTransform> BarShelfTransformations = new() {
        { "*fruitbar-*", new ModelTransform() {
            Origin = new() { X = 0.41f, Y = 0.03f, Z = 0.4875f },
            Scale = 0.65f
        }}
    };

    #endregion

    #region FruitBasket

    public Dictionary<string, ModelTransform> FruitBasketTransformations = new() {
        { "dehydratedfruit-*", new ModelTransform() {
            Origin = new() { X = 0.5f, Y = 0.0f, Z = 0.5f },
            Translation = new() { X = 0f, Y = 0.2f, Z = 0f }
        }},
        { "dryfruit-*", new ModelTransform() {
            Origin = new() { X = 0.5f, Y = 0.0f, Z = 0.5f },
            Translation = new() { X = 0f, Y = 0.2f, Z = 0f }
        }}
    };

    public Dictionary<string, ModelTransform> FruitBasketDomainTransformations = new() {
        { "wildcraftfruit:*", new ModelTransform() {
            Origin = new() { X = 0.5f, Y = 0.0f, Z = 0.5f },
            Scale = 0.9f,
            Translation = new() { X = 0f, Y = 0.1f, Z = 0f }
        }}
    };

    #endregion
}
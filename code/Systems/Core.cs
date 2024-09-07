using static FoodShelves.Patches;

[assembly: ModInfo(name: "Food Shelves", modID: "foodshelves")]

namespace FoodShelves;

public class Core : ModSystem {
    public override void Start(ICoreAPI api) {
        base.Start(api);

        api.RegisterBlockClass("FoodShelves.BlockPieShelf", typeof(BlockPieShelf));
        api.RegisterBlockEntityClass("FoodShelves.BlockEntityPieShelf", typeof(BlockEntityPieShelf));
        api.RegisterBlockClass("FoodShelves.BlockBreadShelf", typeof(BlockBreadShelf));
        api.RegisterBlockEntityClass("FoodShelves.BlockEntityBreadShelf", typeof(BlockEntityBreadShelf));
        api.RegisterBlockClass("FoodShelves.BlockBarShelf", typeof(BlockBarShelf));
        api.RegisterBlockEntityClass("FoodShelves.BlockEntityBarShelf", typeof(BlockEntityBarShelf));
        api.RegisterBlockClass("FoodShelves.BlockSushiShelf", typeof(BlockSushiShelf));
        api.RegisterBlockEntityClass("FoodShelves.BlockEntitySushiShelf", typeof(BlockEntitySushiShelf));
        api.RegisterBlockClass("FoodShelves.BlockEggShelf", typeof(BlockEggShelf));
        api.RegisterBlockEntityClass("FoodShelves.BlockEntityEggShelf", typeof(BlockEntityEggShelf));
        api.RegisterBlockClass("FoodShelves.BlockSeedShelf", typeof(BlockSeedShelf));
        api.RegisterBlockEntityClass("FoodShelves.BlockEntitySeedShelf", typeof(BlockEntitySeedShelf));

        api.RegisterBlockClass("FoodShelves.BlockTableWShelf", typeof(BlockTableWShelf));
        api.RegisterBlockEntityClass("FoodShelves.BlockEntityTableWShelf", typeof(BlockEntityTableWShelf));

        api.RegisterBlockClass("FoodShelves.BlockFruitBasket", typeof(BlockFruitBasket));
        api.RegisterBlockEntityClass("FoodShelves.BlockEntityFruitBasket", typeof(BlockEntityFruitBasket));
        api.RegisterBlockClass("FoodShelves.BlockVegetableBasket", typeof(BlockVegetableBasket));
        api.RegisterBlockEntityClass("FoodShelves.BlockEntityVegetableBasket", typeof(BlockEntityVegetableBasket));

        api.RegisterBlockClass("FoodShelves.BlockBarrelRack", typeof(BlockBarrelRack));
        api.RegisterBlockEntityClass("FoodShelves.BlockEntityBarrelRack", typeof(BlockEntityBarrelRack));
        api.RegisterBlockClass("FoodShelves.BlockBarrelRackBig", typeof(BlockBarrelRackBig));
        api.RegisterBlockEntityClass("FoodShelves.BlockEntityBarrelRackBig", typeof(BlockEntityBarrelRackBig));

        api.RegisterBlockClass("FoodShelves.BlockHorizontalBarrelBig", typeof(BlockHorizontalBarrelBig));
    }

    public override void AssetsLoaded(ICoreAPI api) {
        base.AssetsLoaded(api);

        PieShelfData = api.LoadAsset<RestrictionData.PieShelfData>("foodshelves:config/restrictions/shelves/pieshelf.json");
        PieShelfTransformations = api.LoadAsset<Dictionary<string, ModelTransform>>("foodshelves:config/transformations/shelves/pieshelf.json");
        BreadShelfData = api.LoadAsset<RestrictionData.BreadShelfData>("foodshelves:config/restrictions/shelves/breadshelf.json");
        BreadShelfTransformations = api.LoadAsset<Dictionary<string, ModelTransform>>("foodshelves:config/transformations/shelves/breadshelf.json");
        BarShelfData = api.LoadAsset<RestrictionData.BarShelfData>("foodshelves:config/restrictions/shelves/barshelf.json");
        BarShelfTransformations = api.LoadAsset<Dictionary<string, ModelTransform>>("foodshelves:config/transformations/shelves/barshelf.json");
        SushiShelfData = api.LoadAsset<RestrictionData.SushiShelfData>("foodshelves:config/restrictions/shelves/sushishelf.json");
        EggShelfData = api.LoadAsset<RestrictionData.EggShelfData>("foodshelves:config/restrictions/shelves/eggshelf.json");
        SeedShelfData = api.LoadAsset<RestrictionData.SeedShelfData>("foodshelves:config/restrictions/shelves/seedshelf.json");
        
        FruitBasketData = api.LoadAsset<RestrictionData.FruitBasketData>("foodshelves:config/restrictions/baskets/fruitbasket.json"); ;
        FruitBasketTransformations = api.LoadAsset<Dictionary<string, ModelTransform>>("foodshelves:config/transformations/baskets/fruitbasket.json");
        VegetableBasketData = api.LoadAsset<RestrictionData.VegetableBasketData>("foodshelves:config/restrictions/baskets/vegetablebasket.json");
        VegetableBasketTransformations = api.LoadAsset<Dictionary<string, ModelTransform>>("foodshelves:config/transformations/baskets/vegetablebasket.json");

        BarrelRackData = api.LoadAsset<RestrictionData.BarrelRackData>("foodshelves:config/restrictions/barrels/barrelrack.json");
        BarrelRackBigData = api.LoadAsset<RestrictionData.BarrelRackBigData>("foodshelves:config/restrictions/barrels/barrelrackbig.json");
    }

    public override void AssetsFinalize(ICoreAPI api) {
        base.AssetsFinalize(api);

        foreach (CollectibleObject obj in api.World.Collectibles) {
            PatchPieShelf(obj, PieShelfData);
            PatchBreadShelf(obj, BreadShelfData);
            PatchBarShelf(obj, BarShelfData);
            PatchSushiShelf(obj, SushiShelfData);
            PatchEggShelf(obj, EggShelfData);
            PatchSeedShelf(obj, SeedShelfData);

            PatchFruitBasket(obj, FruitBasketData);
            PatchVegetableBasket(obj, VegetableBasketData);
            
            PatchBarrelRack(obj, BarrelRackData);
            PatchBarrelRackBig(obj, BarrelRackBigData);
        }
    }
}

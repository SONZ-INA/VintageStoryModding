namespace FoodShelves;

public static class Rotation {
    public static void SetBlockMeshAngle(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel, bool val) {
        if (val && world.BlockAccessor.GetBlockEntity(blockSel.Position) is BlockEntityFruitBasket frbasket) {
            BlockPos targetPos = blockSel.DidOffset ? blockSel.Position.AddCopy(blockSel.Face.Opposite) : blockSel.Position;
            double dx = byPlayer.Entity.Pos.X - (targetPos.X + blockSel.HitPosition.X);
            double dz = byPlayer.Entity.Pos.Z - (targetPos.Z + blockSel.HitPosition.Z);
            float angleHor = (float)Math.Atan2(dx, dz);

            float deg22dot5rad = GameMath.PIHALF / 4;
            float roundRad = ((int)Math.Round(angleHor / deg22dot5rad)) * deg22dot5rad;
            frbasket.MeshAngle = roundRad;
        }
    }

    public static MeshData GenMesh(ICoreAPI Api, BlockEntity BE, ITesselatorAPI tesselator, Block block) {
        if (block == null) {
            block = Api.World.BlockAccessor.GetBlock(BE.Pos);
            if (block == null) return null;
        }

        string shapePath = block.Shape?.Base?.ToString();
        string blockName = block.Code?.ToString();
        string modDomain = null;
        int colonIndex = shapePath.IndexOf(':');

        if (colonIndex != -1) {
            blockName = blockName.Substring(colonIndex + 1);
            modDomain = shapePath.Substring(0, colonIndex);
            shapePath = shapePath.Substring(colonIndex + 1);
        }
        else {
            Api.Logger.Debug(modDomain + " - GenMesh: Indexing for shapePath failed.");
            return null;
        }

        string key = blockName + "Meshes" + block.Code.ToString();
        Dictionary<string, MeshData> meshes = ObjectCacheUtil.GetOrCreate(Api, key, () => {
            return new Dictionary<string, MeshData>();
        });

        int rndTexNum = 45652; // if buggy change this value
        if (rndTexNum > 0) rndTexNum = GameMath.MurmurHash3Mod(BE.Pos.X, BE.Pos.Y, BE.Pos.Z, rndTexNum);

        string meshKey = key + "-" + rndTexNum;
        if (meshes.TryGetValue(meshKey, out MeshData mesh)) return mesh;

        AssetLocation shapeLocation = new(modDomain + ":shapes/" + shapePath + ".json");

        Shape shape = Api.Assets.TryGet(shapeLocation)?.ToObject<Shape>();
        if (shape == null) return null;

        ITexPositionSource texSource = tesselator.GetTextureSource(block, rndTexNum);
        tesselator.TesselateBlock(block, out mesh); // Generate mesh data
        meshes[meshKey] = mesh; // Cache the generated mesh

        return mesh;
    }
}

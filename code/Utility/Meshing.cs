namespace FoodShelves;

public static class Meshing {
    public static float GetBlockMeshAngle(IPlayer byPlayer, BlockSelection blockSel, bool val) {
        if (val) {
            BlockPos targetPos = blockSel.DidOffset ? blockSel.Position.AddCopy(blockSel.Face.Opposite) : blockSel.Position;
            double dx = byPlayer.Entity.Pos.X - (targetPos.X + blockSel.HitPosition.X);
            double dz = byPlayer.Entity.Pos.Z - (targetPos.Z + blockSel.HitPosition.Z);
            float angleHor = (float)Math.Atan2(dx, dz);

            float deg22dot5rad = GameMath.PIHALF / 4;
            float roundRad = ((int)Math.Round(angleHor / deg22dot5rad)) * deg22dot5rad;
            return roundRad;
        }

        return 0;
    }

    public static MeshData GenBlockMesh(ICoreAPI Api, BlockEntity BE, ITesselatorAPI tesselator) {
        Block block = Api.World.BlockAccessor.GetBlock(BE.Pos);
        if (block == null) return null;

        string shapePath = block.Shape?.Base?.ToString();
        if (shapePath == null) return null;

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

        tesselator.TesselateBlock(block, out mesh); // Generate mesh data
        meshes[meshKey] = mesh; // Cache the generated mesh

        return mesh;
    }

    // Use unhashed block mesh when generating simple meshes that will in no way be different from any other block mesh (mainly focused on content display)
    // For example i cannot use this for FruitBasket because many FruitBaskets will have different contents and we need to correctly find the one to
    // render based on its content.
    public static MeshData GenBlockMeshUnhashed(ICoreAPI Api, BlockEntity BE, ITesselatorAPI tesselator) {
        Block block = Api.World.BlockAccessor.GetBlock(BE.Pos);
        if (block == null) return null;

        string shapePath = block.Shape?.Base?.ToString();
        if (shapePath == null) return null;

        string modDomain = null;
        int colonIndex = shapePath.IndexOf(':');

        if (colonIndex != -1) {
            modDomain = shapePath.Substring(0, colonIndex);
            shapePath = shapePath.Substring(colonIndex + 1);
        }
        else {
            Api.Logger.Debug(modDomain + " - GenMesh: Indexing for shapePath failed.");
            return null;
        }

        AssetLocation shapeLocation = new(modDomain + ":shapes/" + shapePath + ".json");

        Shape shape = Api.Assets.TryGet(shapeLocation)?.ToObject<Shape>();
        if (shape == null) return null;

        tesselator.TesselateBlock(block, out MeshData mesh); // Generate mesh data

        return mesh;
    }

    public static MeshData SubstituteBlockShape(ICoreAPI Api, ITesselatorAPI tesselator, string shapePath, Block texturesFromBlock) {
        string modDomain = null;
        int colonIndex = shapePath.IndexOf(':');

        if (colonIndex != -1) {
            modDomain = shapePath.Substring(0, colonIndex);
            shapePath = shapePath.Substring(colonIndex + 1);
        }
        else {
            Api.Logger.Debug(modDomain + " - GenMesh: Indexing for shapePath failed.");
            return null;
        }

        AssetLocation shapeLocation = new(modDomain + ":shapes/" + shapePath + ".json");
        ITexPositionSource texSource = tesselator.GetTextureSource(texturesFromBlock);
        Shape shape = Api.Assets.TryGet(shapeLocation)?.ToObject<Shape>();
        if (shape == null) return null;

        tesselator.TesselateShape(null, shape, out MeshData mesh, texSource);
        return mesh;
    }

    public static MeshData GenBlockWContentMesh(ICoreClientAPI capi, Block block, ItemStack[] contents, float[,] transformationMatrix, Dictionary<string, ModelTransform> modelTransformations = null) {
        if (block == null)
            return null;

        // Block Region
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
            capi.Logger.Debug(modDomain + " - GenMesh: Indexing for shapePath failed.");
            return null;
        }

        string key = blockName + "Meshes" + block.Code.ToString();
        Dictionary<string, MeshData> meshes = ObjectCacheUtil.GetOrCreate(capi, key, () => {
            return new Dictionary<string, MeshData>();
        });

        AssetLocation shapeLocation = new(modDomain + ":shapes/" + shapePath + ".json");

        Shape shape = capi.Assets.TryGet(shapeLocation)?.ToObject<Shape>();
        if (shape == null) return null;

        ITexPositionSource texSource = capi.Tesselator.GetTextureSource(block);
        capi.Tesselator.TesselateBlock(block, out MeshData basketMesh); // Generate mesh data

        // Content Region
        if (contents != null) {
            int offset = transformationMatrix.GetLength(1);

            for (int i = 0; i < contents.Length; i++) {
                if (contents[i] != null) {
                    if (contents[i].Item == null) continue; // To fix the damn pumpkin bug on existing worlds
                    capi.Tesselator.TesselateItem(contents[i].Item, out MeshData contentData);

                    if (i < offset) {
                        if (modelTransformations != null) {
                            ModelTransform transformation = contents[i].Item.GetTransformation(modelTransformations);
                            if (transformation != null) contentData.ModelTransform(transformation);
                        }

                        float[] matrixTransform =
                            new Matrixf()
                            .Translate(0.5f, 0, 0.5f)
                            .RotateXDeg(transformationMatrix[3, i])
                            .RotateYDeg(transformationMatrix[4, i])
                            .RotateZDeg(transformationMatrix[5, i])
                            .Scale(0.5f, 0.5f, 0.5f)
                            .Translate(transformationMatrix[0, i] - 0.84375f, transformationMatrix[1, i], transformationMatrix[2, i] - 0.8125f)
                            .Values;

                        contentData.MatrixTransform(matrixTransform);
                    }

                    basketMesh.AddMeshData(contentData);
                }
            }
        }

        return basketMesh;
    }

    // GeneralizedTexturedGenMesh written specifically for expanded foods, i might need it so it's here
    public static MeshData GeneralizedTexturedGenMesh(ICoreClientAPI capi, Item item) { // third passed attribute would be a Dictionary of keys and texture paths and
                                                                                        // then iterate through them after Textures.Clear()
        string shapePath = item.Shape?.Base?.ToString();
        string itemName = item.Code?.ToString();
        string modDomain = null;
        int colonIndex = shapePath.IndexOf(':');

        if (colonIndex != -1) {
            itemName = itemName.Substring(colonIndex + 1);
            modDomain = shapePath.Substring(0, colonIndex);
            shapePath = shapePath.Substring(colonIndex + 1);
        }
        else {
            capi.Logger.Debug(modDomain + " - GenMesh: Indexing for shapePath failed.");
            return null;
        }

        string key = itemName + "Meshes" + item.Code.ToString();
        Dictionary<string, MeshData> meshes = ObjectCacheUtil.GetOrCreate(capi, key, () => {
            return new Dictionary<string, MeshData>();
        });

        AssetLocation shapeLocation = new(modDomain + ":shapes/" + shapePath + ".json"); // A generalized shape would go here, like a berrybread for example

        Shape shape = capi.Assets.TryGet(shapeLocation)?.ToObject<Shape>();
        if (shape == null) return null;

        var keys = new List<string>(shape.Textures.Keys); // we can get all available keys here. IT PASSES A REFERENCE SO DON'T REMOVE THE 'List<string>' PART!!
        Shape shapeClone = shape.Clone(); // has to be cloned to work

        shapeClone.Textures.Clear(); // remove all keys and values

        // shapeClone.Textures.Remove("sides"); // Remove or .Clear() Textures to add new ones, 
                                                // These textures are contained within `shapes` .json file.
                                                // If it doesn't work, remove the "textures" {} from `blocktypes` .json files, i haven't tested it
        AssetLocation ass = new("game:item/food/fruit/cherry"); // path to desired texture
        foreach (var x in keys) {
            shapeClone.Textures.Add(x, ass); // apply desired texture to the key, make sure to add *all* keys as it might crash
        }

        ITexPositionSource texSource = new ShapeTextureSource(capi, shapeClone, null); // get texture source of the newly modified shape
        capi.Tesselator.TesselateShape(null, shapeClone, out MeshData block2, texSource); // tesselate shape here.
                                                                                          // this will use the texSource to apply textures, that's why we got it as a ShapeTextureSource

        return block2;
    }
}

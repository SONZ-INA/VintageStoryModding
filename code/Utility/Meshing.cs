namespace FoodShelves;

public static class Meshing {
    public static MeshData SubstituteBlockShape(ICoreAPI Api, ITesselatorAPI tesselator, string shapePath, Block texturesFromBlock) {
        AssetLocation shapeLocation = new(shapePath);
        ITexPositionSource texSource = tesselator.GetTextureSource(texturesFromBlock);
        Shape shape = Api.Assets.TryGet(shapeLocation)?.ToObject<Shape>();
        if (shape == null) return null;

        tesselator.TesselateShape(null, shape, out MeshData mesh, texSource);
        return mesh;
    }

    public static MeshData SubstituteItemShape(ICoreAPI Api, ITesselatorAPI tesselator, string shapePath, Item texturesFromItem) {
        AssetLocation shapeLocation = new(shapePath);
        ITexPositionSource texSource = tesselator.GetTextureSource(texturesFromItem);
        Shape shape = Api.Assets.TryGet(shapeLocation)?.ToObject<Shape>();
        if (shape == null) return null;

        tesselator.TesselateShape(null, shape, out MeshData mesh, texSource);
        return mesh;
    }

    public static MeshData GenBlockContentMesh(ICoreClientAPI capi, ItemStack[] contents, float[,] transformationMatrix, Dictionary<string, ModelTransform> modelTransformations = null) {
        MeshData contentMesh = null;
        
        if (contents != null) {
            int offset = transformationMatrix.GetLength(1);

            for (int i = 0; i < contents.Length; i++) {
                if (contents[i] != null) {
                    if (contents[i].Item == null) continue; // To fix the damn pumpkin bug on existing worlds
                    capi.Tesselator.TesselateItem(contents[i].Item, out MeshData itemMesh);

                    if (i < offset) {
                        if (modelTransformations != null) {
                            ModelTransform transformation = contents[i].Item.GetTransformation(modelTransformations);
                            if (transformation != null) itemMesh.ModelTransform(transformation);
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

                        itemMesh.MatrixTransform(matrixTransform);
                    }

                    if (contentMesh == null) contentMesh = itemMesh;
                    else contentMesh.AddMeshData(itemMesh);
                }
            }
        }

        return contentMesh;
    }

    // GeneralizedTexturedGenMesh written specifically for expanded foods, i might need it so it's here
    public static MeshData GeneralizedTexturedGenMesh(ICoreClientAPI capi, Item item) { // third passed attribute would be a Dictionary of keys and texture paths and
                                                                                        // then iterate through them after Textures.Clear()
        AssetLocation shapeLocation = item.Shape.Base.WithPathPrefixOnce("shapes/").WithPathAppendixOnce(".json"); // A generalized shape would go here, like a berrybread for example

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

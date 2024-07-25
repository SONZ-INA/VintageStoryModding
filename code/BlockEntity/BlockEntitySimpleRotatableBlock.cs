namespace FoodShelves;

public class BlockEntitySimpleRotatableBlock : BlockEntityDisplay {
    public float MeshAngle { get; set; }
    protected Block block;

    public override InventoryBase Inventory => throw new NotImplementedException();
    public override string InventoryClassName => throw new NotImplementedException();

    public override bool OnTesselation(ITerrainMeshPool mesher, ITesselatorAPI tesselator) {
        bool skipmesh = base.OnTesselation(mesher, tesselator);

        if (!skipmesh) {
            MeshData meshData = GenMesh(Api, this, tesselator, block);
            if (meshData == null) return false;

            mesher.AddMeshData(meshData.Clone().Rotate(new Vec3f(0.5f, 0.5f, 0.5f), 0, MeshAngle, 0));
        }

        return true;
    }

    public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldForResolving) {
        base.FromTreeAttributes(tree, worldForResolving);
        MeshAngle = tree.GetFloat("meshAngle", 0f);
        RedrawAfterReceivingTreeAttributes(worldForResolving);
    }

    public override void ToTreeAttributes(ITreeAttribute tree) {
        base.ToTreeAttributes(tree);
        tree.SetFloat("meshAngle", MeshAngle);
    }

    protected override float[][] genTransformationMatrices() {
        throw new NotImplementedException();
    }
}

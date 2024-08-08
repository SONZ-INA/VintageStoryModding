namespace FoodShelves;

public static class SelectionBoxReferences {
    #region BarrelRack

    public enum BarrelRackPart {
        Base = 0,
        Tap = 1,
        Hole = 2
    }

    public static readonly Dictionary<BarrelRackPart, Cuboidf> BarrelRackCuboids = new() {
        { BarrelRackPart.Base, new(0.01f, 0, 0, 1f, 0.999f, 1f) },
        { BarrelRackPart.Tap, new(0f, 0.1f, 0.32f, 0.125f, 0.3f, 0.68f) },
        { BarrelRackPart.Hole, new(0f, 0.75f, 0.4f, 0.125f, 0.88f, 0.6f) }
    };

    #endregion
}

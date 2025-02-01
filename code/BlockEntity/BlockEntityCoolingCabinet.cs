namespace FoodShelves;

public class BlockEntityCoolingCabinet : BlockEntityDisplay {
    private readonly InventoryGeneric inv;
    private Block block;
    
    public override InventoryBase Inventory => inv;
    public override string InventoryClassName => Block?.Attributes?["inventoryClassName"].AsString();
    public override string AttributeTransformCode => Block?.Attributes?["attributeTransformCode"].AsString();

    private const int shelfCount = 3;
    private const int segmentsPerShelf = 3;
    private const int itemsPerSegment = 4;
    private const int bonusSlots = 1;
    static readonly int slotCount = shelfCount * segmentsPerShelf * itemsPerSegment + bonusSlots;
    private float perishMultiplier = 0.75f;
    
    public bool CabinetOpen { get; set; }
    public bool drawerOpen = false;

    public BlockEntityCoolingCabinet() {
        inv = new InventoryGeneric(slotCount, InventoryClassName + "-0", Api, (id, inv) => {
            if (id != 36) return new ItemSlotHolderUniversal(inv);
            else return new ItemSlotCoolingOnly(inv);
        });
    }

    public override void Initialize(ICoreAPI api) {
        block = api.World.BlockAccessor.GetBlock(Pos);
        base.Initialize(api);

        if (CabinetOpen) perishMultiplier = 1f;
        inv.OnAcquireTransitionSpeed += Inventory_OnAcquireTransitionSpeed;
    }

    private float GetPerishRate() {
        return container.GetPerishRate() * perishMultiplier; // Slower perish rate
    }

    private float Inventory_OnAcquireTransitionSpeed(EnumTransitionType transType, ItemStack stack, float baseMul) {
        if (transType == EnumTransitionType.Dry || transType == EnumTransitionType.Melt) return container.Room?.ExitCount == 0 ? 2f : 0.5f;
        if (Api == null) return 0;

        if (transType == EnumTransitionType.Ripen) {
            return GameMath.Clamp((1 - GetPerishRate() - 0.5f) * 3, 0, 1);
        }

        return perishMultiplier;
    }

    internal bool OnInteract(IPlayer byPlayer, BlockSelection blockSel) {
        ItemSlot slot = byPlayer.InventoryManager.ActiveHotbarSlot;

        // Open/Close cabinet or drawer
        if (byPlayer.Entity.Controls.ShiftKey) {
            switch (blockSel.SelectionBoxIndex) {
                case 9:
                    if (!drawerOpen) OpenDrawer();
                    else CloseDrawer();
                    MarkDirty();
                    break;
                default:
                    if (!CabinetOpen) OpenCabinet();
                    else CloseCabinet();
                    MarkDirty();
                    break;
            }

            return true;
        }

        if (!CabinetOpen) return false;

        // Take/Put items
        if (slot.Empty) {
            return TryTake(byPlayer, blockSel);;
        }
        else if (slot.Itemstack is ILiquidSink) { // Take water from drawer
            Api.Logger.Debug("TODO");
            return false;
        }
        else {
            if (slot.HolderUniversalCheck()) {
                AssetLocation sound = slot.Itemstack?.Block?.Sounds?.Place;

                if (TryPut(slot, blockSel)) {
                    Api.World.PlaySoundAt(sound ?? new AssetLocation("sounds/player/build"), byPlayer.Entity, byPlayer, true, 16);
                    MarkDirty();
                    return true;
                }
            }

            if (slot.CoolingOnlyCheck()) { // Putting ice in the drawer

            }

            (Api as ICoreClientAPI)?.TriggerIngameError(this, "cantplace", Lang.Get("foodshelves:This item cannot be placed in this container."));
            return false;
        }
    }

    private bool TryPut(ItemSlot slot, BlockSelection blockSel) {
        int startIndex = blockSel.SelectionBoxIndex;
        if (startIndex > 8) return false; // If it's cabinet or drawer selection box, return

        startIndex *= itemsPerSegment;
        if (!inv[startIndex].Empty && (IsLargeItem(slot.Itemstack) || IsLargeItem(inv[startIndex].Itemstack))) return false;

        for (int i = 0; i < itemsPerSegment; i++) {
            int currentIndex = startIndex + i;
            if (inv[currentIndex].Empty) {
                int moved = slot.TryPutInto(Api.World, inv[currentIndex]);
                MarkDirty();
                (Api as ICoreClientAPI)?.World.Player.TriggerFpAnimation(EnumHandInteract.HeldItemInteract);
                return moved > 0;
            }
        }

        return false;
    }

    private bool TryTake(IPlayer byPlayer, BlockSelection blockSel) {
        int startIndex = blockSel.SelectionBoxIndex;
        if (startIndex > 8) return false; // If it's cabinet or drawer selection box, return

        startIndex *= itemsPerSegment;

        for (int i = itemsPerSegment - 1; i >= 0; i--) {
            int currentIndex = startIndex + i;
            if (!inv[currentIndex].Empty) {
                ItemStack stack = inv[currentIndex].TakeOut(1);
                if (byPlayer.InventoryManager.TryGiveItemstack(stack)) {
                    AssetLocation sound = stack.Block?.Sounds?.Place;
                    Api.World.PlaySoundAt(sound ?? new AssetLocation("sounds/player/build"), byPlayer.Entity, byPlayer, true, 16);
                }

                if (stack.StackSize > 0) {
                    Api.World.SpawnItemEntity(stack, Pos.ToVec3d().Add(0.5, 0.5, 0.5));
                }

                (Api as ICoreClientAPI)?.World.Player.TriggerFpAnimation(EnumHandInteract.HeldItemInteract);
                MarkDirty();
                return true;
            }
        }

        return false;
    }

    #region Animation & Meshing

    private MeshData ownMesh;

    BlockEntityAnimationUtil animUtil {
        get { return GetBehavior<BEBehaviorAnimatable>()?.animUtil; }
    }

    private void OpenCabinet() {
        if (animUtil?.activeAnimationsByAnimCode.ContainsKey("cabinetopen") == false) {
            animUtil?.StartAnimation(new AnimationMetaData() {
                Animation = "cabinetopen",
                Code = "cabinetopen",
                AnimationSpeed = 3f,
                EaseOutSpeed = 1,
                EaseInSpeed = 2
            });
        }

        perishMultiplier = 1f;
        CabinetOpen = true;
    }

    private void CloseCabinet() {
        if (animUtil?.activeAnimationsByAnimCode.ContainsKey("cabinetopen") == true) {
            animUtil?.StopAnimation("cabinetopen");
        }

        perishMultiplier = 0.75f;
        CabinetOpen = false;
    }

    private void OpenDrawer() {
        if (animUtil?.activeAnimationsByAnimCode.ContainsKey("draweropen") == false) {
            animUtil?.StartAnimation(new AnimationMetaData() {
                Animation = "draweropen",
                Code = "draweropen",
                AnimationSpeed = 3f,
                EaseOutSpeed = 1,
                EaseInSpeed = 2
            });
        }

        drawerOpen = true;
    }

    private void CloseDrawer() {
        if (animUtil?.activeAnimationsByAnimCode.ContainsKey("draweropen") == true) {
            animUtil?.StopAnimation("draweropen");
        }

        drawerOpen = false;
    }

    private MeshData GenMesh(ITesselatorAPI tesselator) {
        Block block = Block;
        if (Block == null) {
            block = Api.World.BlockAccessor.GetBlock(Pos);
            Block = block;
        }
        if (block == null) return null;

        int rndTexNum = GameMath.MurmurHash3Mod(Pos.X, Pos.Y, Pos.Z, 85378);

        string key = "coolingCabinetMeshes" + Block.Code;
        Dictionary<string, MeshData> meshes = ObjectCacheUtil.GetOrCreate(Api, key, () => {
            return new Dictionary<string, MeshData>();
        });

        string sKey = "coolingCabinetShape" + Block.Code;
        Dictionary<string, Shape> shapes = ObjectCacheUtil.GetOrCreate(Api, sKey, () => {
            return new Dictionary<string, Shape>();
        });

        if (!shapes.TryGetValue(sKey, out Shape shape)) {
            AssetLocation shapeLocation = new(ShapeReferences.CoolingCabinet);
            ITexPositionSource textureSource = tesselator.GetTextureSource(block);
            shapes[sKey] = Api.Assets.TryGet(shapeLocation)?.ToObject<Shape>();
        }

        string meshKey = block.Code + "-" + rndTexNum;
        if (meshes.TryGetValue(meshKey, out MeshData mesh)) {
            if (animUtil != null && animUtil.renderer == null) {
                animUtil.InitializeAnimator(key, mesh, shape, new Vec3f(0, GetRotationAngle(block), 0));
            }

            return mesh;
        }

        if (animUtil != null) {
            if (animUtil.renderer == null) {
                ITexPositionSource texSource = tesselator.GetTextureSource(block);
                mesh = animUtil.InitializeAnimator(key, shape, texSource, new Vec3f(0, GetRotationAngle(block), 0));
            }

            return meshes[meshKey] = mesh;
        }

        return null;
    }

    protected override float[][] genTransformationMatrices() {
        float[][] tfMatrices = new float[slotCount][];

        for (int shelf = 0; shelf < shelfCount; shelf++) {
            for (int segment = 0; segment < segmentsPerShelf; segment++) {
                for (int item = 0; item < itemsPerSegment; item++) {
                    int index = shelf * (segmentsPerShelf * itemsPerSegment) + segment * itemsPerSegment + item;

                    float y = shelf * 0.4921875f;
                    ModelTransform transformation = null;

                    if (HolderUniversalTransformations != null) {
                        transformation = inv[index].Itemstack?.Collectible.GetTransformation(HolderUniversalTransformations);
                    }

                    if ((index < itemsPerSegment && IsLargeItem(inv[index].Itemstack)) || (index >= itemsPerSegment && IsLargeItem(inv[index].Itemstack))) {
                        float x = segment * 0.65f;
                        float z = item * 0.65f;

                        var matrix =
                            new Matrixf()
                            .Translate(0.5f, 0, 0.5f)
                            .RotateYDeg(block.Shape.rotateY)
                            .Scale(0.95f, 0.95f, 0.95f)
                            .Translate(x - 0.625f, y + 0.66f, z - 0.5325f);

                        if (transformation != null) matrix.ApplyModelTransformToMatrixF(transformation);
                        tfMatrices[index] = matrix.Values;
                    }
                    else {
                        float x = segment * 0.65f + (index % (itemsPerSegment / 2) == 0 ? -0.16f : 0.16f);
                        float z = (index / (itemsPerSegment / 2)) % 2 == 0 ? -0.18f : 0.18f;

                        var matrix =
                            new Matrixf()
                            .Translate(0.5f, 0, 0.5f)
                            .RotateYDeg(block.Shape.rotateY)
                            .Scale(0.95f, 0.95f, 0.95f)
                            .Translate(x - 0.625f, y + 0.66f, z - 0.5325f);


                        if (transformation != null) matrix.ApplyModelTransformToMatrixF(transformation);
                        tfMatrices[index] = matrix.Values;
                    }
                }
            }
        }

        return tfMatrices;
    }

    public override bool OnTesselation(ITerrainMeshPool mesher, ITesselatorAPI tesselator) {
        bool skipmesh = base.OnTesselation(mesher, tesselator);

        if (!skipmesh) {
            if (ownMesh == null) {
                ownMesh = GenMesh(tesselator);
                if (ownMesh == null) return false;
            }

            mesher.AddMeshData(ownMesh.Clone().Rotate(new Vec3f(.5f, .5f, .5f), 0, GameMath.DEG2RAD * GetRotationAngle(block), 0));
            if (CabinetOpen) OpenCabinet();
        }

        return true;
    }

    #endregion

    public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldForResolving) {
        base.FromTreeAttributes(tree, worldForResolving);
        CabinetOpen = tree.GetBool("cabinetOpen", false);
        RedrawAfterReceivingTreeAttributes(worldForResolving);
    }

    public override void ToTreeAttributes(ITreeAttribute tree) {
        base.ToTreeAttributes(tree);
        tree.SetBool("cabinetOpen", CabinetOpen);
    }

    public override void GetBlockInfo(IPlayer forPlayer, StringBuilder sb) {
        DisplayPerishMultiplier(GetPerishRate(), sb);

        float ripenRate = GameMath.Clamp((1 - GetPerishRate() - 0.5f) * 3, 0, 1);
        if (ripenRate > 0) sb.Append(Lang.Get("Suitable spot for food ripening."));

        DisplayInfo(forPlayer, sb, inv, InfoDisplayOptions.BySegment, slotCount, segmentsPerShelf, itemsPerSegment);
    }
}

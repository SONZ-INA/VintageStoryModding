namespace FoodShelves;

public class BlockEntityCoolingCabinet : BlockEntityDisplay {
    private readonly InventoryGeneric inv;
    private Block block;
    
    public override InventoryBase Inventory => inv;
    public override string InventoryClassName => Block?.Attributes?["inventoryClassName"].AsString();
    public override string AttributeTransformCode => Block?.Attributes?["attributeTransformCode"].AsString();

    private const int shelfCount = 3;
    private const int segmentsPerShelf = 5;
    private const int itemsPerSegment = 2;
    static readonly int slotCount = shelfCount * segmentsPerShelf * itemsPerSegment;

    #region Animations

    private MeshData ownMesh;
    public bool CabinetOpen { get; set; }
    private bool drawerOpen = false;

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

        CabinetOpen = true;
    }

    private void CloseCabinet() {
        if (animUtil?.activeAnimationsByAnimCode.ContainsKey("cabinetopen") == true) {
            animUtil?.StopAnimation("cabinetopen");
        }
        
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

        Shape shape = null;
        if (animUtil != null) {
            string skeydict = "coolingCabinetMeshes";
            Dictionary<string, Shape> shapes = ObjectCacheUtil.GetOrCreate(Api, skeydict, () => {
                return new Dictionary<string, Shape>();
            });

            string skey = Block.Code + "-random-" + rndTexNum;
            if (!shapes.TryGetValue(skey, out shape)) {
                AssetLocation shapeLocation = new(ShapeReferences.CoolingCabinet);
                ITexPositionSource textureSource = tesselator.GetTextureSource(block);
                shape = Api.Assets.TryGet(shapeLocation)?.ToObject<Shape>();

                shapes[skey] = shape;
            }
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

    #endregion

    public BlockEntityCoolingCabinet() { inv = new InventoryGeneric(slotCount, InventoryClassName + "-0", Api, (_, inv) => new ItemSlotHolderUniversal(inv)); }

    public override void Initialize(ICoreAPI api) {
        block = api.World.BlockAccessor.GetBlock(Pos);
        base.Initialize(api);

        inv.OnAcquireTransitionSpeed += Inventory_OnAcquireTransitionSpeed;
    }

    private float Inventory_OnAcquireTransitionSpeed(EnumTransitionType transType, ItemStack stack, float baseMul) {
        if (transType == EnumTransitionType.Dry || transType == EnumTransitionType.Melt) return container.Room?.ExitCount == 0 ? 2f : 0.5f;
        if (Api == null) return 0;

        if (transType == EnumTransitionType.Ripen) {
            float perishRate = container.GetPerishRate();
            return GameMath.Clamp((1 - perishRate - 0.5f) * 3, 0, 1);
        }

        return 1;
    }

    internal bool OnInteract(IPlayer byPlayer, BlockSelection blockSel) {
        ItemSlot slot = byPlayer.InventoryManager.ActiveHotbarSlot;

        if (slot.Empty) {
            if (blockSel.SelectionBoxIndex == 1) {
                if (!drawerOpen)
                    OpenDrawer();
                else
                    CloseDrawer();
            }
            else {
                if (!CabinetOpen)
                    OpenCabinet();
                else
                    CloseCabinet();
            }

            return TryTake(byPlayer, blockSel);
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

            (Api as ICoreClientAPI)?.TriggerIngameError(this, "cantplace", Lang.Get("foodshelves:This item cannot be placed in this container."));
            return false;
        }
    }

    private bool TryPut(ItemSlot slot, BlockSelection blockSel) {
        int index = blockSel.SelectionBoxIndex;

        if (inv[index].Empty) {
            int moved = slot.TryPutInto(Api.World, inv[index]);
            MarkDirty();
            (Api as ICoreClientAPI)?.World.Player.TriggerFpAnimation(EnumHandInteract.HeldItemInteract);
            return moved > 0;
        }

        return false;
    }

    private bool TryTake(IPlayer byPlayer, BlockSelection blockSel) {
        int index = blockSel.SelectionBoxIndex;

        if (!inv[index].Empty) {
            ItemStack stack = inv[index].TakeOut(1);
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

        return false;
    }

    protected override float[][] genTransformationMatrices() {
        float[][] tfMatrices = new float[slotCount][];

        for (int i = 0; i < slotCount; i++) {
            tfMatrices[i] =
                new Matrixf()
                .Translate(0.5f, 0, 0.5f)
                .RotateYDeg(block.Shape.rotateY)
                .Translate(- 0.5f, i * 0.313f + 0.0525f, - 0.5f)
                .Values;
        }

        return tfMatrices;
    }

    public override bool OnTesselation(ITerrainMeshPool mesher, ITesselatorAPI tesselator) {
        bool skipmesh = base.OnTesselation(mesher, tesselator);

        if (!skipmesh) {
            if (ownMesh == null) {
                ownMesh = GenMesh(tesselator);
                if (ownMesh == null) return false;
                if (CabinetOpen) OpenCabinet();
            }

            mesher.AddMeshData(ownMesh.Clone().Rotate(new Vec3f(.5f, .5f, .5f), 0, GameMath.DEG2RAD * GetRotationAngle(block), 0));
        }

        return true;
    }

    public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldForResolving) {
        base.FromTreeAttributes(tree, worldForResolving);
        CabinetOpen = tree.GetBool("cabinetOpen", false);
        RedrawAfterReceivingTreeAttributes(worldForResolving);
    }

    public override void ToTreeAttributes(ITreeAttribute tree) {
        base.ToTreeAttributes(tree);
        tree.SetBool("cabinetOpen", CabinetOpen);
        Api.Logger.Debug($"Saved: \"{CabinetOpen}\" for the cabinet.");
    }

    public override void GetBlockInfo(IPlayer forPlayer, StringBuilder sb) {
        base.GetBlockInfo(forPlayer, sb);

        float ripenRate = GameMath.Clamp((1 - container.GetPerishRate() - 0.5f) * 3, 0, 1);
        if (ripenRate > 0) sb.Append(Lang.Get("Suitable spot for food ripening."));

        DisplayInfo(forPlayer, sb, inv, InfoDisplayOptions.BySegment, slotCount, segmentsPerShelf, itemsPerSegment);
    }
}

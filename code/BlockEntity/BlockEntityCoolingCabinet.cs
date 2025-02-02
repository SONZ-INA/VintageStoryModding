﻿using Vintagestory.API.Common.Entities;
using static Vintagestory.GameContent.BlockLiquidContainerBase;

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

        if (!inv[36].Empty && WildcardUtil.Match(CoolingOnlyData.CollectibleCodes, inv[36].Itemstack.Collectible.Code)) perishMultiplier = 0.4f;
        if (CabinetOpen) perishMultiplier = 1f;
        inv.OnAcquireTransitionSpeed += Inventory_OnAcquireTransitionSpeed;
    }

    private float GetPerishRate() {
        return container.GetPerishRate() * perishMultiplier; // Slower perish rate
    }

    private float Inventory_OnAcquireTransitionSpeed(EnumTransitionType transType, ItemStack stack, float baseMul) {
        if (!inv[36].Empty && perishMultiplier < 0.75f && !WildcardUtil.Match(CoolingOnlyData.CollectibleCodes, inv[36].Itemstack.Collectible.Code)) {
            if (CabinetOpen) perishMultiplier = 1f;
            else perishMultiplier = 0.75f;
            MarkDirty();
        }

        if (transType == EnumTransitionType.Dry) return container.Room?.ExitCount == 0 ? 2f : 0.5f;
        if (Api == null) return 0;

        if (transType == EnumTransitionType.Ripen) {
            return GameMath.Clamp((1 - GetPerishRate() - 0.5f) * 3, 0, 1);
        }

        if (transType == EnumTransitionType.Melt) {
            // Single cut ice will last for ~12 hours. However a stack of them will also last ~12 hours, so a multiplier depending on them is needed. 
            return (float)((float)1 / inv[36].Itemstack?.StackSize ?? 1) * 4; // A stack would last about 32 days which is 8 ice blocks
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

        // Take/Put items
        if (CabinetOpen && slot.Empty) {
            return TryTake(byPlayer, blockSel); ;
        }
        else if (drawerOpen && slot.Itemstack?.Collectible is ILiquidSink) {
            return TryTakeWater(byPlayer, slot, blockSel);
        }
        else {
            if (CabinetOpen && slot.HolderUniversalCheck()) {
                AssetLocation sound = slot.Itemstack?.Block?.Sounds?.Place;

                if (TryPut(slot, blockSel)) {
                    Api.World.PlaySoundAt(sound ?? new AssetLocation("sounds/player/build"), byPlayer.Entity, byPlayer, true, 16);
                    MarkDirty();
                    return true;
                }
            }

            if (drawerOpen && slot.CoolingOnlyCheck()) {
                AssetLocation sound = slot.Itemstack?.Block?.Sounds?.Place;

                if (TryPutIce(byPlayer, slot, blockSel)) {
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

    private bool TryPutIce(IPlayer byPlayer, ItemSlot slot, BlockSelection selection) {
        if (selection.SelectionBoxIndex != 9) return false;
        ItemStack stack = inv[36].Itemstack;

        if (inv[36].Empty 
            || (stack.StackSize < stack.Collectible.MaxStackSize
            && WildcardUtil.Match(CoolingOnlyData.CollectibleCodes, stack.Collectible.Code))
        ) {
            int quantity = byPlayer.Entity.Controls.CtrlKey ? slot.Itemstack.StackSize : 1;
            int moved = slot.TryPutInto(Api.World, inv[36], quantity);

            if (moved == 0 && slot.Itemstack != null) { // Attempt to merge if it fails
                ItemStackMergeOperation op = new(Api.World, EnumMouseButton.Left, 0, EnumMergePriority.ConfirmedMerge, quantity) {
                    SourceSlot = new DummySlot(slot.Itemstack),
                    SinkSlot = new DummySlot(stack)
                };
                stack.Collectible.TryMergeStacks(op);
            }

            MarkDirty();
            (Api as ICoreClientAPI)?.World.Player.TriggerFpAnimation(EnumHandInteract.HeldItemInteract);
            return moved > 0;
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

    private bool TryTakeWater(IPlayer byPlayer, ItemSlot hotbarSlot, BlockSelection selection) {
        if (selection.SelectionBoxIndex != 9) return false;
        ILiquidSink objLsi = hotbarSlot.Itemstack.Collectible as ILiquidSink;

        if (!objLsi.AllowHeldLiquidTransfer) return false;
        if (inv[36].Itemstack == null) return false;

        ItemStack ownContentStack = inv[36].Itemstack;
        if (ownContentStack == null) return false;

        ItemStack contentStack = ownContentStack.Clone();
        int num = SplitStackAndPerformAction(byPlayer.Entity, hotbarSlot, (ItemStack stack) => objLsi.TryPutLiquid(stack, ownContentStack, objLsi.CapacityLitres));
        if (num > 0) {
            TryTakeContent(num);
            DoLiquidMovedEffects(byPlayer, contentStack, num, EnumLiquidDirection.Fill);
            return true;
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
        
        if (!drawerOpen && !inv[36].Empty) {
            if (WildcardUtil.Match(CoolingOnlyData.CollectibleCodes, inv[36].Itemstack.Collectible.Code)) 
                perishMultiplier = 0.4f;
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

        if (!CabinetOpen) perishMultiplier = 0.75f;
        drawerOpen = true;
    }

    private void CloseDrawer() {
        if (animUtil?.activeAnimationsByAnimCode.ContainsKey("draweropen") == true) {
            animUtil?.StopAnimation("draweropen");
        }

        if (!CabinetOpen && !inv[36].Empty) {
            if (WildcardUtil.Match(CoolingOnlyData.CollectibleCodes, inv[36].Itemstack.Collectible.Code)) 
                perishMultiplier = 0.4f;
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

        tfMatrices[36] = new Matrixf().Scale(0.01f, 0.01f, 0.01f).Values; // Hide original cut ice shape

        return tfMatrices;
    }

    public override bool OnTesselation(ITerrainMeshPool mesher, ITesselatorAPI tesselator) {
        bool skipmesh = false;

        if (!inv[36].Empty) {
            Shape shape = Api.Assets.TryGet(ShapeReferences.CoolingCabinetUtil).ToObject<Shape>();
            if (shape != null) {
                ShapeTextureSource iceTS = new(Api as ICoreClientAPI, shape, "ccIceTexSource");
                tesselator.TesselateShape("ccIceMeshing", shape, out MeshData iceMesh, iceTS);

                mesher.AddMeshData(iceMesh);
            }
        }

        if (!skipmesh) skipmesh = base.OnTesselation(mesher, tesselator);

        if (!skipmesh) {
            Api.Logger.Debug("skipmesh");
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

    #region Liquid Handlers

    public int SplitStackAndPerformAction(Entity byEntity, ItemSlot slot, System.Func<ItemStack, int> action) {
        if (slot.Itemstack == null) return 0;

        if (slot.Itemstack.StackSize == 1) {
            int num = action(slot.Itemstack);
            if (num > 0) {
                if (byEntity is not EntityPlayer obj) return num;

                obj.WalkInventory(delegate (ItemSlot pslot) {
                    if (pslot.Empty || pslot is ItemSlotCreative || pslot.StackSize == pslot.Itemstack.Collectible.MaxStackSize) {
                        return true;
                    }

                    int mergableQuantity = slot.Itemstack.Collectible.GetMergableQuantity(slot.Itemstack, pslot.Itemstack, EnumMergePriority.DirectMerge);
                    if (mergableQuantity == 0) return true;

                    BlockLiquidContainerBase obj3 = slot.Itemstack.Collectible as BlockLiquidContainerBase;
                    BlockLiquidContainerBase blockLiquidContainerBase = pslot.Itemstack.Collectible as BlockLiquidContainerBase;
                    if ((obj3?.GetContent(slot.Itemstack)?.StackSize).GetValueOrDefault() != (blockLiquidContainerBase?.GetContent(pslot.Itemstack)?.StackSize).GetValueOrDefault()) {
                        return true;
                    }

                    slot.Itemstack.StackSize += mergableQuantity;
                    pslot.TakeOut(mergableQuantity);
                    slot.MarkDirty();
                    pslot.MarkDirty();

                    return true;
                });
            }

            return num;
        }

        ItemStack itemStack = slot.Itemstack.Clone();
        itemStack.StackSize = 1;
        int num2 = action(itemStack);
        if (num2 > 0) {
            slot.TakeOut(1);
            if (byEntity is not EntityPlayer obj2 || !obj2.Player.InventoryManager.TryGiveItemstack(itemStack, slotNotifyEffect: true)) {
                Api.World.SpawnItemEntity(itemStack, byEntity.SidedPos.XYZ);
            }

            slot.MarkDirty();
        }

        return num2;
    }

    public void DoLiquidMovedEffects(IPlayer player, ItemStack contentStack, int moved, EnumLiquidDirection dir) {
        if (player != null) {
            WaterTightContainableProps containableProps = GetContainableProps(contentStack);
            float num = moved / containableProps.ItemsPerLitre;
            (player as IClientPlayer)?.TriggerFpAnimation(EnumHandInteract.HeldItemInteract);
            Api.World.PlaySoundAt((dir == EnumLiquidDirection.Fill) ? containableProps.FillSound : containableProps.PourSound, player.Entity, player, randomizePitch: true, 16f, GameMath.Clamp(num / 5f, 0.35f, 1f));
            Api.World.SpawnCubeParticles(player.Entity.Pos.AheadCopy(0.25).XYZ.Add(0.0, player.Entity.SelectionBox.Y2 / 2f, 0.0), contentStack, 0.75f, (int)num * 2, 0.45f);
        }
    }

    public ItemStack TryTakeContent(int quantityItem) {
        ItemStack itemstack = inv[36].Itemstack;
        if (itemstack == null) return null;

        ItemStack itemStack = inv[36].Itemstack.Clone();
        itemStack.StackSize = quantityItem;
        itemstack.StackSize -= quantityItem;

        if (itemstack.StackSize <= 0) {
            inv[36].Itemstack = null;
        }
        else {
            inv[36].Itemstack = itemstack;
        }

        inv[36].MarkDirty();
        MarkDirty(true);
        return itemStack;
    }

    #endregion
}

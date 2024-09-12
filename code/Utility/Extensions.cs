using System.Linq;

namespace FoodShelves; 

public static class Extensions {
    public static void EnsureAttributesNotNull(this CollectibleObject obj) => obj.Attributes ??= new JsonObject(new JObject());

    public static T LoadAsset<T>(this ICoreAPI api, string path) => api.Assets.Get(new AssetLocation(path)).ToObject<T>();

    public static ModelTransform GetTransformation(this CollectibleObject obj, Dictionary<string, ModelTransform> transformations) {
        foreach (KeyValuePair<string, ModelTransform> transformation in transformations) {
            if (WildcardUtil.Match(transformation.Key, obj.Code.ToString())) return transformation.Value;
        }

        return null;
    }

    public static string FirstCharToUpper(this string input) {
        if (input == null) throw new ArgumentNullException(nameof(input));
        if (string.IsNullOrEmpty(input)) throw new ArgumentException($"{nameof(input)} cannot be empty", nameof(input));
        return string.Concat(input[0].ToString().ToUpper(), input.AsSpan(1));
    }

    public static float[,] GenTransformationMatrix(float[] x, float[] y, float[] z, float[] rX, float[] rY, float[] rZ) {
        float[,] transformationMatrix = new float[6, x.Length];

        for (int i = 0; i < x.Length; i++) {
            transformationMatrix[0, i] = x[i];
            transformationMatrix[1, i] = y[i];
            transformationMatrix[2, i] = z[i];
            transformationMatrix[3, i] = rX[i];
            transformationMatrix[4, i] = rY[i];
            transformationMatrix[5, i] = rZ[i];
        }

        return transformationMatrix;
    }

    public static MeshData BlockYRotation(this MeshData obj, BlockEntity BE) {
        Block block = BE.Api.World.BlockAccessor.GetBlock(BE.Pos);
        return obj.Rotate(new Vec3f(0.5f, 0.5f, 0.5f), 0, block.Shape.rotateY * GameMath.DEG2RAD, 0);
    }

    public static T GetBlockEntityExt<T>(this IBlockAccessor blockAccessor, BlockPos pos) where T : BlockEntity {
        if (blockAccessor.GetBlockEntity<T>(pos) is T blockEntity) {
            return blockEntity;
        }

        if (blockAccessor.GetBlock(pos) is BlockMultiblock multiblock) {
            BlockPos multiblockPos = new(pos.X + multiblock.OffsetInv.X, pos.Y + multiblock.OffsetInv.Y, pos.Z + multiblock.OffsetInv.Z, pos.dimension);
            return blockAccessor.GetBlockEntity<T>(multiblockPos);
        }
        return null;
    }

    public static void SetTreeAttributeContents(this ItemStack stack, InventoryGeneric inv, string attributeName, int index = 1) {
        TreeAttribute stacksTree = new();

        for (; index < inv.Count; index++) {
            if (inv[index].Itemstack == null) break;
            stacksTree[index + ""] = new ItemstackAttribute(inv[index].Itemstack);
        }

        stack.Attributes[$"{attributeName}"] = stacksTree;
    }

    public static ItemStack[] GetTreeAttributeContents(this ItemStack itemStack, ICoreClientAPI capi, string attributeName, int index = 1) {
        ITreeAttribute tree = itemStack?.Attributes?.GetTreeAttribute($"{attributeName}");
        List<ItemStack> contents = new();

        if (tree != null) {
            for (; index < tree.Count + 1; index++) {
                ItemStack stack = tree.GetItemstack(index + "");
                stack?.ResolveBlockOrItem(capi.World);
                contents.Add(stack);
            }
        }

        return contents.ToArray();
    }

    public static int GetStackCacheHashCodeFNV(ItemStack[] contentStack) {
        if (contentStack == null) return 0;

        unchecked {
            // FNV-1 hash since any other simpler one ends up colliding, fuck data structures & algorithms btw
            const uint FNV_OFFSET_BASIS = 2166136261;
            const uint FNV_32_PRIME = 16777619;

            uint hash = FNV_OFFSET_BASIS;

            hash = (hash ^ (uint)contentStack.Length.GetHashCode()) * FNV_32_PRIME;

            for (int i = 0; i < contentStack.Length; i++) {
                if (contentStack[i] == null) continue;

                uint collectibleHash = (uint)(contentStack[i].Collectible != null ? contentStack[i].Collectible.Code.GetHashCode() : 0);
                hash = (hash ^ collectibleHash) * FNV_32_PRIME;
            }

            return (int)hash;
        }
    }

    private static char GetFacingFromBlockCode(BlockEntity block) {
        string codePath = block.Block.Code.ToString();
        if (codePath == null) return 'n';

        string[] parts = codePath.Split('-');
        string facingStr = parts.Last().ToLowerInvariant();

        return facingStr switch {
            "north" => 'n',
            "east" => 'e',
            "south" => 's',
            "west" => 'w',
            _ => 'n'
        };
    }

    // currently hardcoded for BarrelRackBig
    public static int[] GetMultiblockIndex(Vec3i offset, BlockEntity block) {
        char facing = GetFacingFromBlockCode(block);

        int transformedX = offset.X;
        int transformedY = offset.Y;
        int transformedZ = offset.Z;

        switch (facing) {
            case 'n':
                break; // No change needed for North
            case 's':
                transformedX -= 1;
                transformedZ += 1;
                break;
            case 'e':
                transformedZ += 1;
                break;
            case 'w':
                transformedX -= 1;
                break;
        }

        return new int[3] { transformedX, transformedY, transformedZ };
    }
}

using System;
using Fries.Data;
using UnityEngine;

namespace Fries.BlockGrid {
    public enum DirectionalType {
        // 非方向性方块，对方向参数无感
        NonDirectional = 0,
        // 单向方向性方块，指四条边上只有一条边有特征的方块
        SingleDirectional = 1,
        // 双向方向性方块，指四条边上只有两条边有特征的方块
        DoubleDirectional = 2,
        NwBasedDoubleDirectional = 3
    }
    
    public static class DirectioonalBlockApplier {
        private static void nonDirectional(Transform transform, Facing facing) { }

        private static void singleDirectional(Transform transform, Facing facing) {
            switch (facing) {
                // 所有方块 Prefab 默认面朝北面
                case Facing.north:
                    break;
                case Facing.south:
                    transform.localScale = transform.localScale.multiply(0f._ff(-1f));
                    break;
                // 所有方块 Prefab 默认旋转为 0 0 0
                case Facing.east:
                    transform.localEulerAngles = 0f.f_f(-90f);
                    break;
                case Facing.west:
                    transform.localEulerAngles = 0f.f_f(-90f);
                    transform.localScale = transform.localScale.multiply(0f._ff(-1f));
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(facing), facing, null);
            }
        }
        
        private static void doubleDirectional(Transform transform, Facing facing, bool isNWBased = false) {
            if (isNWBased) {
                switch (facing) {
                    // 方块 Prefab 默认是西北方向块
                    case Facing.north | Facing.west:
                        break;
                    case Facing.south | Facing.west:
                        transform.localScale = transform.localScale.multiply(0f._ff(-1f));
                        break;
                    case Facing.north | Facing.east:
                        transform.localScale = transform.localScale.multiply(0f.ff_(-1f));
                        break;
                    case Facing.south | Facing.east:
                        transform.localScale = transform.localScale.multiply(0f._f_(-1f, -1f));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(facing), facing, null);
                }
                return;
            }
            
            switch (facing) {
                // 所有方块 Prefab 默认面朝北面
                case Facing.north:
                    break;
                case Facing.south:
                    transform.localScale = transform.localScale.multiply(0f._ff(-1f));
                    break;
                case Facing.east:
                    transform.localScale = transform.localScale.multiply(0f.ff_(-1f));
                    break;
                case Facing.west:
                    transform.localScale = transform.localScale.multiply(0f._f_(-1f, -1f)); 
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(facing), facing, null);
            }
        }

        public static void apply<T>(T blkType, Transform transform, Facing facing) where T : Enum {
            BlockData data = BlockData.GetBlockData(blkType);
            switch (data.DirectionalType) {
                case DirectionalType.NonDirectional:
                    nonDirectional(transform, facing);
                    break;
                case DirectionalType.SingleDirectional:
                    singleDirectional(transform, facing);
                    break;
                case DirectionalType.DoubleDirectional:
                    doubleDirectional(transform, facing);
                    break;
                case DirectionalType.NwBasedDoubleDirectional:
                    doubleDirectional(transform, facing, true);
                    break;
            }
        }
    }
}
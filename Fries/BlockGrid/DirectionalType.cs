using System;
using System.Collections.Generic;
using Fries.Data;
using Fries.EvtSystem;
using UnityEngine;

namespace Fries.BlockGrid {
    public enum DirectionalType {
        // 非方向性方块，对方向参数无感
        NA = 0,
        // 单向方向性方块，指四条边上只有一条边有特征的方块
        Single = 1,    // 通过旋转与缩放还原四个朝向
        SingleRot = 2, // 只通过旋转来还原四个朝向
        // 双向方向性方块，指四条边上只有两条边有特征的方块
        Double = 3,
        NwDouble = 4
    }

    [EvtDeclarer]
    public partial class DirectionalProcessorModifyPhase { Action<int, Action<Transform, Facing>, Action<List<Facing>>, Func<Facing, Facing>> register; }

    public struct ProcessorEntry {
        public Func<Facing, Facing> translator { get; private set; }
        public Action<Transform, Facing> processor { get; private set; }
        public Action<List<Facing>> potentialFacing { get; private set; }

        public ProcessorEntry(Action<Transform, Facing> processor, Action<List<Facing>> potentialFacing, Func<Facing, Facing> translator) {
            this.processor = processor;
            this.potentialFacing = potentialFacing;
            this.translator = translator;
        }
    }
    
    public static class DirectioonalBlockApplier {
        private static Dictionary<int, ProcessorEntry> processorMap = new();

        public static void NsweList(List<Facing> list) {
            list.Add(Facing.north);
            list.Add(Facing.south);
            list.Add(Facing.east);
            list.Add(Facing.west);
        }
        public static void NwNsweList(List<Facing> list) {
            list.Add(Facing.north | Facing.west);
            list.Add(Facing.south | Facing.west);
            list.Add(Facing.north | Facing.east);
            list.Add(Facing.south | Facing.east);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void reset() {
            registerProcessor((int)DirectionalType.NA, nonDirectional, list => list.Add(Facing.north));
            registerProcessor((int)DirectionalType.Single, singleDirectional, NsweList);
            registerProcessor((int)DirectionalType.Double, doubleDirectional, NsweList);
            registerProcessor((int)DirectionalType.NwDouble, nwDoubleDirectional, NwNsweList, N2nwTranslator);
            registerProcessor((int)DirectionalType.SingleRot, singleRot, NsweList);
        }

        [EvtListener(typeof(Events.OnEvtsysLoaded))]
        private static void trigger() => DirectionalProcessorModifyPhase.TriggerNonAlloc(registerProcessor);
        
        private static void registerProcessor(int directionalType, Action<Transform, Facing> processor, Action<List<Facing>> potentialFacing, 
            Func<Facing, Facing> translator = null) {
            translator ??= NoTranslator;
            processorMap[directionalType] = new ProcessorEntry(processor, potentialFacing, translator);
        }

        private static void nonDirectional(Transform transform, Facing facing) { }

        private static void singleRot(Transform transform, Facing facing) {
            switch (facing) {
                case Facing.north:
                    transform.localEulerAngles = new Vector3(0, 0, 0);
                    break;
                case Facing.east:
                    transform.localEulerAngles = new Vector3(0, 90, 0);
                    break;
                case Facing.south:
                    transform.localEulerAngles = new Vector3(0, 180, 0);
                    break;
                case Facing.west:
                    transform.localEulerAngles = new Vector3(0, 270, 0);
                    break;
            }
        }
        
        private static void singleDirectional(Transform transform, Facing facing) {
            switch (facing) {
                // 所有方块 Prefab 默认面朝北面
                case Facing.north:
                    break;
                case Facing.south:
                    transform.localScale = transform.localScale.multiply(1f.ff_(-1f));
                    break;
                // 所有方块 Prefab 默认旋转为 0 0 0
                case Facing.east:
                    transform.localEulerAngles = 0f.f_f(-90f);
                    transform.localScale = transform.localScale.multiply(1f.ff_(-1f));
                    break;
                case Facing.west:
                    transform.localEulerAngles = 0f.f_f(-90f);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(facing), facing, null);
            }
        }

        public static void nwDoubleDirectional(Transform transform, Facing facing) {
            switch (facing) {
                // 方块 Prefab 默认是西北方向块
                case Facing.north | Facing.west:
                    break;
                case Facing.south | Facing.west:
                    transform.localScale = transform.localScale.multiply(1f.ff_(-1f));
                    break;
                case Facing.north | Facing.east:
                    transform.localScale = transform.localScale.multiply(1f._ff(-1f));
                    break;
                case Facing.south | Facing.east:
                    transform.localScale = transform.localScale.multiply(1f._f_(-1f, -1f));
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(facing), facing, null);
            }
        }
        
        public static void doubleDirectional(Transform transform, Facing facing) {
            switch (facing) {
                // 所有方块 Prefab 默认面朝北面
                case Facing.north:
                    break;
                case Facing.south:
                    transform.localScale = transform.localScale.multiply(1f.ff_(-1f));
                    break;
                case Facing.east:
                    transform.localScale = transform.localScale.multiply(1f._ff(-1f));
                    break;
                case Facing.west:
                    transform.localScale = transform.localScale.multiply(1f._f_(-1f, -1f)); 
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(facing), facing, null);
            }
        }

        public static void getPotentialFacing(int directionalType, List<Facing> list) {
            if (processorMap.TryGetValue(directionalType, out var entry)) entry.potentialFacing(list);
            else Debug.LogError($"Cannot find processor for directional type {directionalType}");
        }

        public static void apply<T>(T blkType, Transform transform, Facing facing) where T : Enum {
            apply(blkType, transform, facing);
        }
        
        internal static void apply(object blkType, Transform transform, Facing facing) {
            BlockData data = BlockData.GetBlockData(blkType);
            if (processorMap.TryGetValue(data.directionalType, out var entry)) {
                try { entry.processor(transform, entry.translator(facing)); } 
                catch (Exception e) {
                    Debug.LogError($"Failed to apply processor for block {blkType} with facing {facing}: {e}");
                }
            }
        }

        public static Facing NoTranslator(Facing facing) => facing;
        public static Facing N2nwTranslator(Facing facing) {
            if (facing == Facing.north) return Facing.north | Facing.west;
            if (facing == Facing.east) return Facing.east | Facing.north;
            if (facing == Facing.south) return Facing.south | Facing.east;
            if (facing == Facing.west) return Facing.west | Facing.north;
            return facing;
        }
    }
}
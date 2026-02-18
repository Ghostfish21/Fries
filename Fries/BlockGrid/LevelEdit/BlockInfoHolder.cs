using System;
using Fries.CompCache;
using UnityEngine;

namespace Fries.BlockGrid.LevelEdit {
    [TypeTag]
    public class BlockInfoHolder : MonoBehaviour {
        public static bool IsBlockPrefabImmutable = false;
        public static bool LoadThroughPrefab = false;
        [SerializeField] internal BlockKey blockKey;
        public BlockKey BlockKey => blockKey;

        private void Awake() {
            if (LoadThroughPrefab) {
                TaskPerformer.TaskPerformer.callOnConstruct(() => {
                    TaskPerformer.TaskPerformer.inst().scheduleTaskWhen((Action)(() => {
                        if (IsBlockPrefabImmutable) {
                            LevelEditor.Inst.BlockMap.AddPoolElem(blockKey.BlockTypeId, gameObject);
                            object blockEnum = BlockRegistry.GetEnum(blockKey.BlockTypeId);
                            LevelEditor.Inst.BlockMap.SetBlock(BlockKey.Position, blockKey.Position,
                                blockEnum, blockKey.Facing, onBlockCreation:LevelEditor.OnBlockCreation);
                        }
                        else {
                            object blockEnum = BlockRegistry.GetEnum(blockKey.BlockTypeId);
                            LevelEditor.Inst.BlockMap.SetBlock(BlockKey.Position, blockKey.Position,
                                blockEnum, blockKey.Facing, onBlockCreation:LevelEditor.OnBlockCreation);
                            Destroy(gameObject);
                        }
                    }), () => LevelEditor.Inst.BlockMap.HasEverythingPool);
                });
            }
        }

        private void OnDestroy() { }
    }
}
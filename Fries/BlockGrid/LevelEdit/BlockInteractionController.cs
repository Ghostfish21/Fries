using Fries.CompCache;
using Fries.Data;
using Fries.InputDispatch;
using Fries.PhysicsFunctions;
using UnityEngine;

namespace Fries.BlockGrid.LevelEdit {
    public class BlockInteractionController : MonoBehaviour {
        private InputLayer gameplay;
        private InputId LMB;
        private InputId RMB;
        private InputId MMB;
        
        private void Awake() {
            gameplay = InputLayer.get("Gameplay");
            LMB = MouseButton.Left;
            MMB = MouseButton.Middle;
            RMB = MouseButton.Right;
        }

        [SerializeField] private float armReachLength = 5;
        [SerializeField] private bool isInternalPlacement;

        private bool getPointingBlock(out BlockInfoHolder pointingAtBlockInfo, out RaycastHit hit) {
            pointingAtBlockInfo = null;
            hit = default;
            
            Vector3 pos = SimpleMovementController.player.transform.position;
            Vector3 dir = LevelEditor.Inst.CameraController.transform.forward;
            int count = RaycastNonAlloc.Try(pos, dir, out var hits, maxDistance:armReachLength, sortedCloseToFar: true, sortPos: pos);

            RaycastHit? blockHit = null;
            BlockInfoHolder blockInfoHolder = null;
            for (int i = 0; i < count; i++) {
                RaycastHit hit1 = hits[i];
                GameObject go = hit1.collider.gameObject;
                blockInfoHolder = go.GetTaggedObject<BlockInfoHolder>();
                if (!blockInfoHolder) {
                    while (go.transform.parent) {
                        go = go.transform.parent.gameObject;
                        blockInfoHolder = go.GetTaggedObject<BlockInfoHolder>();
                        if (blockInfoHolder) break;
                    }
                }

                if (!blockInfoHolder) continue;
                blockHit = hit1;
                break;
            }

            if (!blockInfoHolder) return false;
            
            pointingAtBlockInfo = blockInfoHolder;
            if (blockHit != null) hit = blockHit.Value;
            return true;
        }
        
        private void externalPlacementMode(BlockInfoHolder info, object holdItem) {
            Vector3Int pointingBlockPos = info.BlockKey.Position;

            Facing playerFacing = LevelEditor.Inst.CameraController.transform.GetFacing(out Facing horizontal);
            Facing antiPlayerFacing = playerFacing.GetOpposite();
            Vector3Int placementPosition = pointingBlockPos + antiPlayerFacing.ToUnitVector();

            LevelEditor.Inst.BlockMap.SetBlock(placementPosition, holdItem, horizontal, onBlockCreation:static (gobj, blkKey) => {
                var bih = gobj.GetTaggedObject<BlockInfoHolder>();
                if (!bih) bih = gobj.AddComponent<BlockInfoHolder>();
                bih.blockKey = blkKey;
            });
        }

        private void internalPlacementMode(BlockInfoHolder info, object holdItem) {
            Vector3Int pointingBlockPos = info.BlockKey.Position;
            Facing playerFacing = LevelEditor.Inst.CameraController.transform.GetFacing();
            LevelEditor.Inst.BlockMap.SetBlock(pointingBlockPos, holdItem, playerFacing, onBlockCreation:static (gobj, blkKey) => {
                var bih = gobj.GetTaggedObject<BlockInfoHolder>();
                if (!bih) bih = gobj.AddComponent<BlockInfoHolder>();
                bih.blockKey = blkKey;
            });
        }

        private bool lmb;
        private bool mmb;
        private bool rmb;
        private void Update() {
            if (!LevelEditor.Inst.isValid) return;
            
            lmb = gameplay.isDown(LMB);
            rmb = gameplay.isDown(RMB);
            mmb = gameplay.isDown(MMB);

            if (!getPointingBlock(out var info, out _)) {
                LevelEditor.Inst.CrosshairDisplayer.pointingGrid = null;
                return;
            }
            
            LevelEditor.Inst.CrosshairDisplayer.pointingGrid = info.BlockKey.Position;
            
            if (!lmb && !rmb && !mmb) return;

            if (LevelEditor.Inst.PlayerBackpack.GetBlockOnHand() is ITool iTool) {
                if (lmb) iTool.OnLMBClicked(info.BlockKey);
                else if (rmb) iTool.OnRMBClicked(info.BlockKey);
                else if (mmb) iTool.OnMBBClicked(info.BlockKey);
                return;
            }
            
            if (lmb) {
                ListSet<BlockKey> ls = LevelEditor.Inst.EverythingPool.ActivateObject<ListSet<BlockKey>>();
                ls.Add(info.BlockKey);
                LevelEditor.Inst.BlockMap.RemoveBlocks(ls, null);
                LevelEditor.Inst.EverythingPool.DeactivateObject(ls);
                return;
            }

            if (mmb) {
                LevelEditor.writer.write($"/give {info.BlockKey.BlockTypeId}");
                return;
            }

            if (rmb) {
                object holdItem = LevelEditor.Inst.PlayerBackpack.GetBlockOnHand();
                if (holdItem == null) return;

                if (!isInternalPlacement) externalPlacementMode(info, holdItem);
                else internalPlacementMode(info, holdItem);
            }
        }
    }
}
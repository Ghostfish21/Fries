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
        public void SetArmReachLength(float reachLength) => armReachLength = reachLength;
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
        
        private bool getPointingPart(out PartInfoHolder pointingAtPartInfo, out RaycastHit hit) {
            pointingAtPartInfo = null;
            hit = default;
            
            Vector3 pos = SimpleMovementController.player.transform.position;
            Vector3 dir = LevelEditor.Inst.CameraController.transform.forward;
            int count = RaycastNonAlloc.Try(pos, dir, out var hits, maxDistance:armReachLength, sortedCloseToFar: true, sortPos: pos);

            RaycastHit? blockHit = null;
            PartInfoHolder partInfoHolder = null;
            for (int i = 0; i < count; i++) {
                RaycastHit hit1 = hits[i];
                GameObject go = hit1.collider.gameObject;
                partInfoHolder = go.GetTaggedObject<PartInfoHolder>();
                if (!partInfoHolder) {
                    while (go.transform.parent) {
                        go = go.transform.parent.gameObject;
                        partInfoHolder = go.GetTaggedObject<PartInfoHolder>();
                        if (partInfoHolder) break;
                    }
                }

                if (!partInfoHolder) continue;
                blockHit = hit1;
                break;
            }

            if (!partInfoHolder) return false;
            
            pointingAtPartInfo = partInfoHolder;
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

            bool hasBlock = getPointingBlock(out var blockInfo, out RaycastHit hit1);
            bool hasPart = getPointingPart(out var partInfo, out RaycastHit hit2);

            RaycastHit placeAt;
            float dist1 = (hit1.point - LevelEditor.Inst.CameraController.transform.position).magnitude;
            if (!hasBlock) dist1 = 10000f;
            float dist2 = (hit2.point - LevelEditor.Inst.CameraController.transform.position).magnitude;
            if (!hasPart) dist2 = 10000f;
            if (dist1 < dist2) placeAt = hit1;
            else placeAt = hit2;
            
            if (!hasBlock && !hasPart) {
                LevelEditor.Inst.CrosshairDisplayer.pointingGrid = null;
                return;
            }

            bool holdingPart = LevelEditor.Inst.PlayerBackpack.IsItemOnHandAPart(out string stringPartId);
            // 如果有 Part 那么左键是打掉 Part；且中键获取 Part
            if (hasPart) {
                if (lmb) {
                    LevelEditor.Inst.PartModelCache.Deactivate(partInfo.gameObject);
                    LevelEditor.Inst.MarkAsDirty();
                    return;
                }
                if (mmb) {
                    LevelEditor.writer.write($"//give {partInfo.partIdInGiveComm}");
                    return;
                }
            }
            // 如果拿着 Part 那么右键将 Part 放置下来
            if (holdingPart) {
                if (rmb) {
                    partPlacement(placeAt, stringPartId);
                    return;
                }
            }
            
            LevelEditor.Inst.CrosshairDisplayer.pointingGrid = blockInfo.BlockKey.Position;
            
            if (!lmb && !rmb && !mmb) return;

            if (LevelEditor.Inst.PlayerBackpack.GetItemOnHand() is ITool iTool) {
                if (lmb) iTool.OnLMBClicked(blockInfo.BlockKey);
                else if (rmb) iTool.OnRMBClicked(blockInfo.BlockKey);
                else if (mmb) iTool.OnMBBClicked(blockInfo.BlockKey);
                return;
            }
            
            if (lmb) {
                ListSet<BlockKey> ls = LevelEditor.Inst.EverythingPool.ActivateObject<ListSet<BlockKey>>();
                ls.Add(blockInfo.BlockKey);
                LevelEditor.Inst.MarkAsDirty();
                LevelEditor.Inst.BlockMap.RemoveBlocks(ls, null);
                LevelEditor.Inst.EverythingPool.DeactivateObject(ls);
                return;
            }

            if (mmb) {
                LevelEditor.writer.write($"/give {blockInfo.BlockKey.BlockTypeId}");
                return;
            }

            if (rmb) {
                object holdItem = LevelEditor.Inst.PlayerBackpack.GetItemOnHand();
                if (holdItem == null) return;

                LevelEditor.Inst.MarkAsDirty();
                if (!isInternalPlacement) externalPlacementMode(blockInfo, holdItem);
                else internalPlacementMode(blockInfo, holdItem);
            }
        }

        private void partPlacement(RaycastHit hit, string partIdInGiveComm) {
            GameObject part = LevelEditor.Inst.PartModelCache.Activate(
                LevelEditor.Inst.PlayerBackpack.GetItemOnHand(),
                out GameObject prefab, out int partId);
            
            Transform camT = LevelEditor.Inst.CameraController.transform;
            Quaternion playerYaw = Quaternion.Euler(0f, camT.eulerAngles.y, 0f);
            Quaternion prefabRot = prefab ? prefab.transform.localRotation : Quaternion.identity;
            Quaternion finalRot = playerYaw * prefabRot;

            Facing playerFacing = camT.GetFacing(out _);

            PartBounds pb = part.GetTaggedObject<PartBounds>();
            Vector3 localAnchor = Vector3.zero;
            localAnchor = pb.GetFaceCenterLocal(playerFacing);

            part.transform.SetPositionAndRotation(hit.point, finalRot);

            Vector3 worldAnchor = part.transform.TransformPoint(localAnchor);
            Vector3 offset = hit.point - worldAnchor;
            part.transform.position += offset;
            part.transform.SetParent(LevelEditor.Inst.BlockMap.transform);

            var pih = part.GetTaggedObject<PartInfoHolder>();
            pih.partId = partId;
            pih.partIdInGiveComm = partIdInGiveComm;
            pih.blockMapLocalPos = part.transform.position - LevelEditor.Inst.BlockMap.transform.position;
            
            LevelEditor.Inst.MarkAsDirty();
        }
    }
}
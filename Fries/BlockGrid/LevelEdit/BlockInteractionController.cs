using Fries.CompCache;
using Fries.Data;
using Fries.EvtSystem;
using Fries.InputDispatch;
using Fries.PhysicsFunctions;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Fries.BlockGrid.LevelEdit {
    public class BlockInteractionController : MonoBehaviour {
        private InputLayer partManipulate;
        private InputLayer gameplay;
        private InputId LMB;
        private InputId RMB;
        private InputId MMB;
        
        // 锁定物体移动等
        private InputId M;
        private static InputId Horizontal;
        private static InputId Vertical;
        private InputId Q;
        private InputId E;
        
        [EvtListener(typeof(InputEvents.BeforeKeyboardAxisSetup))]
        private static void loadInputId(KeyboardAxisInputModule module) {
            Vertical = KeyboardAxisInputModule.get(module.getAxisCode("Vertical"));
            Horizontal = KeyboardAxisInputModule.get(module.getAxisCode("Horizontal"));
        }

        // 锁定物体旋转等
        private InputId R;
        private InputId G;
        private InputId L;
        private bool isGlobalManipulation = false;

        // 切换 Internal Placement
        private InputId I;
        
        private void Awake() {
            partManipulate = InputLayer.get("Part Manipulate");
            gameplay = InputLayer.get("Gameplay");
            LMB = MouseButton.Left;
            MMB = MouseButton.Middle;
            RMB = MouseButton.Right;

            M = Key.M;
            R = Key.R;
            G = Key.G;
            L = Key.L;
            I = Key.I;
            Q = Key.Q;
            E = Key.E;
        }

        [SerializeField] private float armReachLength = 5;
        public void SetArmReachLength(float reachLength) => armReachLength = reachLength;
        [SerializeField] private bool isInternalPlacement;
        
        [SerializeField] private Camera cam;

        private bool getPointingBlock(out BlockInfoHolder pointingAtBlockInfo, out RaycastHit hit) {
            pointingAtBlockInfo = null;
            hit = default;
            
            Vector3 pos = cam.transform.position;
            Vector3 dir = cam.transform.forward;
            int count = RaycastNonAlloc.Try(pos, dir, out var hits, maxDistance:armReachLength, sortedCloseToFar: true, sortPos: pos);

            RaycastHit? blockHit = null;
            BlockInfoHolder blockInfoHolder = null;
            for (int i = 0; i < count; i++) {
                RaycastHit hit1 = hits[i];
                GameObject go = hit1.collider.gameObject;
                if (go.GetTaggedObject<SimpleMovementController>()) continue;
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
            
            Vector3 pos = cam.transform.position;
            Vector3 dir = cam.transform.forward;
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
            Facing playerFacing = LevelEditor.Inst.CameraController.GetFacing();
            LevelEditor.Inst.BlockMap.SetBlock(pointingBlockPos, holdItem, playerFacing, onBlockCreation:static (gobj, blkKey) => {
                var bih = gobj.GetTaggedObject<BlockInfoHolder>();
                if (!bih) bih = gobj.AddComponent<BlockInfoHolder>();
                bih.blockKey = blkKey;
            });
        }

        private GameObject lockedAt;
        private bool lockModeIsRotation;

        [SerializeField] private float partMovementSpeed = 1;
        [SerializeField] private float partRotationSpeed = 20;

        private bool lmb;
        private bool mmb;
        private bool rmb;
        private void Update() {
            // 运行初始条件检查
            if (!LevelEditor.Inst.isValid) return;
            
            // =================================================================================
            // 抓取输入
            lmb = gameplay.isDown(LMB);
            rmb = gameplay.isDown(RMB);
            mmb = gameplay.isDown(MMB);
            bool i = gameplay.isDown(I);
            bool g = gameplay.isDown(G);
            bool l = gameplay.isDown(L);
            bool m = gameplay.isDown(M);
            bool r = gameplay.isDown(R);

            bool hasBlock = getPointingBlock(out var blockInfo, out RaycastHit hit1);
            bool hasPart = getPointingPart(out var partInfo, out RaycastHit hit2);

            RaycastHit placeAt;
            float dist1 = (hit1.point - LevelEditor.Inst.CameraController.transform.position).magnitude;
            if (!hasBlock) dist1 = 10000f;
            float dist2 = (hit2.point - LevelEditor.Inst.CameraController.transform.position).magnitude;
            if (!hasPart) dist2 = 10000f;
            if (dist1 < dist2) placeAt = hit1;
            else placeAt = hit2;
            
            // =================================================================================
            // 执行无检查快捷键逻辑
            if (i) isInternalPlacement = !isInternalPlacement;
            if (g) isGlobalManipulation = true;
            if (l) isGlobalManipulation = false;
            // 执行非方块移动或旋转解锁
            if (lockedAt) {
                if (m && !lockModeIsRotation) {
                    lockedAt = null;
                    LevelEditor.Inst.LockDisplayer.SetLocked(null, false);
                    partManipulate.disable();
                    return;
                }
                if (r && lockModeIsRotation) {
                    lockedAt = null;
                    LevelEditor.Inst.LockDisplayer.SetLocked(null, false);
                    partManipulate.disable();
                    return;
                }
            }
            // 执行非方块移动与旋转控制逻辑
            if (lockedAt && !lockModeIsRotation) {
                Vector3 forwardAxis = Vector3.forward;
                if (!isGlobalManipulation) forwardAxis = lockedAt.transform.forward;
                Vector3 rightAxis = Vector3.right;
                if (!isGlobalManipulation) rightAxis = lockedAt.transform.right;
                Vector3 upAxis = Vector3.up;
                if (!isGlobalManipulation) upAxis = lockedAt.transform.up;

                float horizontal = partManipulate.getFloat(Horizontal);
                float vertical = partManipulate.getFloat(Vertical);
                float up = 0;
                if (partManipulate.isHeld(Q)) up = 1;
                if (partManipulate.isHeld(E)) up = -1;
                Vector3 forwardOffset = vertical * partMovementSpeed * Time.deltaTime * forwardAxis;
                Vector3 rightOffset = horizontal * partMovementSpeed * Time.deltaTime * rightAxis;
                Vector3 upOffset = up * partMovementSpeed * Time.deltaTime * upAxis;
                lockedAt.transform.position += forwardOffset + rightOffset + upOffset;
                lockedAt.GetTaggedObject<PartInfoHolder>().blockMapLocalPos = lockedAt.transform.position - LevelEditor.Inst.BlockMap.transform.position;
                LevelEditor.Inst.MarkAsDirty();
            }
            if (lockedAt && lockModeIsRotation) {
                float horizontal = partManipulate.getFloat(Horizontal);
                float vertical   = partManipulate.getFloat(Vertical);

                float rollInput = 0f;
                if (partManipulate.isHeld(Q)) rollInput = 1f;
                if (partManipulate.isHeld(E)) rollInput = -1f;

                float yaw   =  horizontal * partRotationSpeed * Time.deltaTime; // 绕 up
                float pitch = -vertical   * partRotationSpeed * Time.deltaTime; // 绕 right（加个负号更符合“推上抬头”习惯）
                float roll  =  rollInput  * partRotationSpeed * Time.deltaTime; // 绕 forward

                Space space = isGlobalManipulation ? Space.World : Space.Self;

                lockedAt.transform.Rotate(Vector3.up,      yaw,   space);
                lockedAt.transform.Rotate(Vector3.right,   pitch, space);
                lockedAt.transform.Rotate(Vector3.forward, roll,  space);
                LevelEditor.Inst.MarkAsDirty();
            }
            
            // =================================================================================
            // 运行条件检查
            if (!hasBlock && !hasPart) {
                LevelEditor.Inst.CrosshairDisplayer.pointingGrid = null;
                LevelEditor.Inst.CrosshairDisplayer.partBounds = null;
                return;
            }

            // =================================================================================
            // 执行非方块物体交互逻辑
            bool holdingPart = LevelEditor.Inst.PlayerBackpack.IsItemOnHandAPart(out string stringPartId);
            Bounds? partBounds = null;
            // 如果有 Part 那么左键是打掉 Part；且中键获取 Part
            if (hasPart) {
                var partBoundsMonoBehaviour = partInfo.gameObject.GetTaggedObject<PartBounds>();
                partBounds = partBoundsMonoBehaviour.CalcWorldAabb();
                // 进入物体锁定模式
                if (!lockedAt) {
                    if (m) {
                        lockedAt = partInfo.gameObject;
                        lockModeIsRotation = false;
                        LevelEditor.Inst.LockDisplayer.SetLocked(partBoundsMonoBehaviour, lockModeIsRotation);
                        partManipulate.enable();
                    }
                    else if (r) {
                        lockedAt = partInfo.gameObject;
                        lockModeIsRotation = true;
                        LevelEditor.Inst.LockDisplayer.SetLocked(partBoundsMonoBehaviour, lockModeIsRotation);
                        partManipulate.enable();
                    }
                }

                if (lmb) {
                    LevelEditor.Inst.PartModelCache.Deactivate(partInfo.partId, partInfo.gameObject);
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
                    LevelEditor.Inst.SetPartWithFacing(placeAt.point, stringPartId);
                    return;
                }
            }

            // =================================================================================
            // 显示指针线框
            if (dist1 < dist2) {
                LevelEditor.Inst.CrosshairDisplayer.pointingGrid = blockInfo.BlockKey.Position;
                LevelEditor.Inst.CrosshairDisplayer.partBounds = null;
            }
            else {
                LevelEditor.Inst.CrosshairDisplayer.pointingGrid = null;
                if (partBounds != null) LevelEditor.Inst.CrosshairDisplayer.partBounds = partBounds.Value;
            }
            
            // =================================================================================
            // 执行条件检查
            if (!lmb && !rmb && !mmb) return;

            // =================================================================================
            // 执行物体交互逻辑
            if (LevelEditor.Inst.PlayerBackpack.GetItemOnHand() is ITool iTool) {
                if (lmb) iTool.OnLMBClicked(blockInfo.BlockKey);
                else if (rmb) iTool.OnRMBClicked(blockInfo.BlockKey);
                else if (mmb) iTool.OnMBBClicked(blockInfo.BlockKey);
                return;
            }
            
            // =================================================================================
            // 执行方块交互逻辑
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
    }
}
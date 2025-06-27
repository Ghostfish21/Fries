using System;
using System.Collections.Generic;
using Fries.Data;
using UnityEngine;

namespace Fries.GoButtons {
    public enum ButtonApproach {
        IPointer,
        Raycast
    }

    public class RaycastManager : MonoBehaviour {
        public Action<GameObject> onMouseExit = (gObj) => { };
        public Action<GameObject> onMouseEnter = (gObj) => { };
        
        [Range(2, 3)]
        public int dimension;
        public List<GameObject> objects = new();
        public Dictionary<GameObject, RaycastHit> hits3D = new ();

        // 控制射线检测频率的变量
        [Range(0, 1)]
        public float raycastInterval = 0.1f;
        private float lastRaycastTime = 0f;

        // Update 方法：每帧执行
        void Update() {
            // 检查是否到了进行射线检测的时机
            if (optimizeRaycastFrequency()) {
                // 根据摄像机模式与鼠标位置计算射线
                (Ray r3, Ray2D r2) ray = computeRayFromCamera();

                // 根据 dimension 参数执行射线检测并返回被击中的物体列表
                hits3D.Clear();
                ICollection<GameObject> hitObjects = performRaycast(ray);
                int llTime = hitObjects.Count * objects.Count;
                int dlTime = hitObjects.Count + objects.Count;
                if (dlTime < llTime) hitObjects = new DictList<GameObject>(hitObjects);
                clearObjects(hitObjects);
                
                // 将检测到的物体加入 objects 列表
                objects.AddRange(hitObjects);
            }
            
            ButtonType buttonsDown = ButtonType.None;
            if (Input.GetMouseButtonDown(0)) buttonsDown |= ButtonType.Left;
            if (Input.GetMouseButtonDown(1)) buttonsDown |= ButtonType.Right;
            if (Input.GetMouseButtonDown(2)) buttonsDown |= ButtonType.Middle;
            MouseEventData medDown = new MouseEventData {
                position = Input.mousePosition.xy(),
                button = buttonsDown
            };
            ButtonType buttonsUp = ButtonType.None;
            if (Input.GetMouseButtonUp(0)) buttonsUp |= ButtonType.Left;
            if (Input.GetMouseButtonUp(1)) buttonsUp |= ButtonType.Right;
            if (Input.GetMouseButtonUp(2)) buttonsUp |= ButtonType.Middle;
            MouseEventData medUp = new MouseEventData {
                position = Input.mousePosition.xy(),
                button = buttonsUp
            };

            bool anyDown = buttonsDown != ButtonType.None;
            bool anyUp = buttonsUp != ButtonType.None;

            objects.For((i, item) => {
                if (anyDown) {
                    MouseEventData medDownInner = new MouseEventData(medDown) { index = i };
                    if (i == 0) triggerEvent(item, "down", medDownInner);
                    else triggerEvent(item, "down-c", medDownInner);
                }

                if (anyUp) {
                    MouseEventData medUpInner = new MouseEventData(medUp) { index = i };
                    if (i == 0) triggerEvent(item, "up", medUpInner);
                    else triggerEvent(item, "up-c", medUpInner);
                }
            });
        }

        private void triggerEvent(GameObject go, string type, MouseEventData med) {
            // Mouse Exit 2D
            if (dimension == 2) {
                GoButton2D go2 = go.getComponent<GoButton2D>();
                if (!go2) return;
                
                if (go2.detectionApproach != ButtonApproach.Raycast)
                    return;

                if (type == "enter") {
                    go2.onMouseEnter.Invoke(new MouseEventData { 
                        position = Input.mousePosition.xy(), 
                        button = ButtonType.None
                    });
                } else if (type == "exit") {
                    go2.onMouseExit.Invoke(new MouseEventData {
                        position = Input.mousePosition.xy(),
                        button = ButtonType.None
                    });
                } else if (type == "down") 
                    go2.onMouseDown.Invoke(med);
                else if (type == "up") 
                    go2.onMouseUp.Invoke(med);
                else if (type == "down-c")
                    go2.onMouseDownCovered.Invoke(med);
                else if (type == "up-c")
                    go2.onMouseUpCovered.Invoke(med);
                else if (type == "drag") {}
            }
            // Mouse Exit 3D
            if (dimension == 3) {
                GoButton go3 = go.getComponent<GoButton>();
                if (!go3) return;

                if (go3.detectionApproach != ButtonApproach.Raycast)
                    return;
                
                if (type == "enter") {
                    go3.onMouseEnter.Invoke(new MouseEventData { 
                        position = Input.mousePosition.xy(), 
                        button = ButtonType.None
                    });
                } else if (type == "exit") {
                    go3.onMouseExit.Invoke(new MouseEventData {
                        position = Input.mousePosition.xy(),
                        button = ButtonType.None
                    });
                } else if (type == "down") 
                    go3.onMouseDown.Invoke(med);
                else if (type == "up") 
                    go3.onMouseUp.Invoke(med);
                else if (type == "down-c")
                    go3.onMouseDownCovered.Invoke(med);
                else if (type == "up-c")
                    go3.onMouseUpCovered.Invoke(med);
                else if (type == "drag") {}
            }
        }

        // 方法：清空 objects 列表
        private void clearObjects(ICollection<GameObject> newItems) {
            // 检查 Objects 中有的但是 NewItems 里没有的（消失的物品）
            objects.ForEach(go => {
                if (newItems.Contains(go)) return;
                onMouseExit?.Invoke(go);
                if (!go.hasTag("GoButton")) return;
                triggerEvent(go, "exit", null);
            });
            
            // 检查 NewItems 中有的但是 Objects 里没有的（新增的物品）
            newItems.ForEach(go => {
                if (objects.Contains(go)) return;
                onMouseEnter?.Invoke(go);
                if (!go.hasTag("GoButton")) return;
                triggerEvent(go, "enter", null);
            });
            
            objects = new List<GameObject>();
        }

        // 方法：根据当前摄像机和鼠标位置计算射线
        private (Ray r3, Ray2D r2) computeRayFromCamera() {
            if (Camera.main == null) {
                Debug.LogError("Camera.main is not found!");
                return (new Ray(), new Ray2D());
            }

            // 获取鼠标当前屏幕位置
            Vector3 mousePos = Input.mousePosition;
    
            // 对透视摄像机，利用 ScreenPointToRay 生成 3D 射线
            Ray ray3 = Camera.main.ScreenPointToRay(mousePos);
    
            // 对于 2D 检测，将鼠标屏幕位置转换为世界坐标
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(mousePos);
            // 假定所有 2D 物体位于 z=0 平面上
            worldPos.z = 0;
            // 构造 Ray2D（方向无需关心，因为后续采用 OverlapPointAll 检测）
            Ray2D ray2 = new Ray2D(new Vector2(worldPos.x, worldPos.y), Vector2.zero);

            return (ray3, ray2);
        }

        // 方法：根据传入的射线和 dimension 参数执行射线检测
        private List<GameObject> performRaycast((Ray r3, Ray2D r2) ray) {
            List<GameObject> hitObjects = new List<GameObject>();

            if (Camera.main == null) {
                Debug.LogError("Camera.main 不存在！");
                return hitObjects;
            }

            if (dimension == 3) {
                // 获取摄像机的 forward 方向，用于计算物体与摄像机之间沿射线方向的距离
                Vector3 camForward = Camera.main.transform.forward;

                if (Camera.main.orthographic) {
                    // 正交模式下，利用 OverlapPointAll 检测鼠标点击处所有碰撞到的 2D 碰撞体
                    RaycastHit[] hits = Physics.RaycastAll(ray.r3);
                    // 排序：按照从摄像机沿 forward 方向的距离（即投影距离）排序
                    Array.Sort(hits, (h1, h2) => {
                        float d1 = Vector3.Dot(h1.transform.position - Camera.main.transform.position, camForward);
                        float d2 = Vector3.Dot(h2.transform.position - Camera.main.transform.position, camForward);
                        return d1.CompareTo(d2);
                    });
                    foreach (var hit in hits) {
                        hits3D[hit.collider.gameObject] = hit;
                        hitObjects.Add(hit.collider.gameObject);
                    }
                }
                else {
                    // 透视模式下，使用 RaycastAll 获取射线沿途所有碰撞到的 3D 物体
                    RaycastHit[] hits = Physics.RaycastAll(ray.r3);
                    // 按照 hit.distance 排序，hit.distance 就是从射线起点到碰撞点的距离
                    Array.Sort(hits, (h1, h2) => h1.distance.CompareTo(h2.distance));
                    foreach (var hit in hits) {
                        hits3D[hit.collider.gameObject] = hit;
                        hitObjects.Add(hit.collider.gameObject);
                    }
                }
            }
            else {
                // 获取鼠标2D检测点：使用 ray.r2.origin（已在 z=0 平面上）
                Vector2 point = ray.r2.origin;
                // 检测该点处所有碰撞到的 2D 碰撞体
                Collider2D[] hits = Physics2D.OverlapPointAll(point);
    
                // 获取摄像机的 forward 方向及位置，用于计算距离
                Vector3 camForward = Camera.main.transform.forward;
                Vector3 camPos = Camera.main.transform.position;
    
                // 按照从摄像机沿 forward 方向的投影距离排序
                Array.Sort(hits, (hit1, hit2) => {
                    float d1 = Vector3.Dot(hit1.transform.position - camPos, camForward);
                    float d2 = Vector3.Dot(hit2.transform.position - camPos, camForward);
                    return d1.CompareTo(d2);
                });
    
                // 将排序后的检测到的 GameObject 加入 hitObjects 列表
                foreach (var hit in hits) hitObjects.Add(hit.gameObject);
            }

            return hitObjects;
        }

        private bool optimizeRaycastFrequency() {
            // 使用计时器检查是否已超过预设的时间间隔
            if (Time.time - lastRaycastTime >= raycastInterval) {
                lastRaycastTime = Time.time;
                return true;
            }
            return false;
        }
    }
}
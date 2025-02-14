using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Fries {
    [RequireComponent(typeof(Collider))]
    public class GoButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler {
        public UnityEvent<PointerEventData> onMouseDown;
        public UnityEvent<PointerEventData> onDrag;
        public UnityEvent<PointerEventData> onMouseUp;

        private void Start() {
            if (Camera.main == null) {
                Debug.LogError("There is no main camera in your scene!");
                return;
            }
            var rc = Camera.main.GetComponent<PhysicsRaycaster>();
            if (rc == null) Debug.LogError("There is no Physics Raycaster on your camera!"); 
            if (EventSystem.current == null) Debug.LogError("There is no Event System in your scene!");
            
        }

        public void OnPointerDown(PointerEventData eventData) {
            onMouseDown.Invoke(eventData);
        }

        public void OnPointerUp(PointerEventData eventData) {
            onMouseUp.Invoke(eventData);
        }

        public void OnDrag(PointerEventData eventData) {
            onDrag.Invoke(eventData);
        }
    }
}
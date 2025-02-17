using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Fries.GoButtons {
    [RequireComponent(typeof(Collider2D))]
    public class GoButton2D : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler, IPointerEnterHandler, IPointerExitHandler {
        public ButtonApproach detectionApproach;
        
        public UnityEvent<MouseEventData> onMouseDown;
        public UnityEvent<MouseEventData> onDrag;
        public UnityEvent<MouseEventData> onMouseUp;
        public UnityEvent<MouseEventData> onMouseEnter;
        public UnityEvent<MouseEventData> onMouseExit;
        public UnityEvent<MouseEventData> onMouseDownCovered;
        public UnityEvent<MouseEventData> onMouseUpCovered;

        private void Start() {
            gameObject.addTag("GoButton");
            if (Camera.main == null) {
                Debug.LogError("There is no main camera in your scene!");
                return;
            }
            var rc = Camera.main.GetComponent<Physics2DRaycaster>();
            if (rc == null) Debug.LogError("There is no Physics 2D Raycaster on your camera!"); 
            if (EventSystem.current == null) Debug.LogError("There is no Event System in your scene!");
            
        }

        public void OnPointerDown(PointerEventData eventData) {
            if (detectionApproach == ButtonApproach.IPointer)
                onMouseDown.Invoke(eventData);
        }

        public void OnPointerUp(PointerEventData eventData) {
            if (detectionApproach == ButtonApproach.IPointer)
                onMouseUp.Invoke(eventData);
        }

        public void OnDrag(PointerEventData eventData) {
            if (detectionApproach == ButtonApproach.IPointer)
                onDrag.Invoke(eventData);
        }

        public void OnPointerEnter(PointerEventData eventData) {
            if (detectionApproach == ButtonApproach.IPointer)
                onMouseEnter.Invoke(eventData);
        }

        public void OnPointerExit(PointerEventData eventData) {
            if (detectionApproach == ButtonApproach.IPointer)
                onMouseExit.Invoke(eventData);
        }
    }
}
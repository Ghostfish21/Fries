using System;
using Fries.EvtSystem;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Fries.InputDispatch.KeyInputDemo {
    public class FGetter : MonoBehaviour {
        [EvtListener(typeof(InputEvents.BeforeInputDispatcherSetup))]
        private static void onDispatcherAwake(InputDispatcher dispatcher) {
            dispatcher.addModule(new KeyboardAxisInputModule());
            dispatcher.addModule(new MouseAxisInputModule());
        }

        public InputLayer layer;

        public bool isFHeld;

        public bool isLMBHeld;
        public bool isLMBDown;
        public bool isLMBUp;

        public bool isGHeld;
        public bool isGUp;
        public bool isGDown;

        public float horizontal;
        public float vertical;
        public float mouseX;
        public float mouseY;
        
        private void Update() {
            if (layer.getFloat(Key.F) == 0) 
                isFHeld = false;
            else isFHeld = true;
            
            if (layer.isHeld(MouseButton.Left)) isLMBHeld = true;
            else isLMBHeld = false;
            if (layer.isUp(MouseButton.Left)) {
                Debug.Log("Mouse Up");
                isLMBUp = true;
            }
            else isLMBUp = false;

            if (layer.isDown(MouseButton.Left)) {
                Debug.Log("Mouse Down");
                isLMBDown = true;
            }
            else isLMBDown = false;
            
            if (layer.isHeld(Key.G)) isGHeld = true;
            else isGHeld = false;

            if (layer.isUp(Key.G)) {
                isGUp = true;
                Debug.Log("G Up");
            }
            else isGUp = false;

            if (layer.isDown(Key.G)) {
                Debug.Log("G Down");
                isGDown = true;
            }
            else isGDown = false;
            
            horizontal = layer.getFloat(KeyboardAxisInputModule.get(0));
            vertical = layer.getFloat(KeyboardAxisInputModule.get(1));
            mouseX = layer.getFloat(MouseAxisInputModule.get(0));
            mouseY = layer.getFloat(MouseAxisInputModule.get(1));
        }
    }
}
using System;
using Fries.Chat;
using Fries.EvtSystem;
using Fries.InputDispatch;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Fries.BlockGrid.LevelEdit {
    public class SimpleMovementController : MonoBehaviour {
	    private InputLayer gameplay;

	    internal static SimpleMovementController player;
        private static KeyboardAxisInputModule KAIM;
		private static InputId horizontal;
		private static InputId vertical;
		private InputId shift;
		private InputId space;
		
		[SerializeField] private float speed = 5;
		
		[EvtListener(typeof(InputEvents.BeforeKeyboardAxisSetup))]
		private static void loadInputId(KeyboardAxisInputModule module) {
			vertical = KeyboardAxisInputModule.get(KAIM.getAxisCode("Vertical"));
			horizontal = KeyboardAxisInputModule.get(KAIM.getAxisCode("Horizontal"));
		}
		
		[EvtListener(typeof(InputEvents.BeforeInputDispatcherSetup))]
		private static void dispatcherSetup(InputDispatcher dispatcher) {
			KAIM = new KeyboardAxisInputModule();
			dispatcher.addModule(KAIM);
		}
		
		private void Awake() {
			shift = Key.Q;
			space = Key.E;
			gameplay = InputLayer.get("Gameplay");
			
			
			if (!player) player = this;
			else Destroy(gameObject);
		}

		private void Update() {
			if (!gameplay) return;
			
			float hor = gameplay.getFloat(horizontal);
			float ver = gameplay.getFloat(vertical);
			bool isShiftHeld = gameplay.isHeld(shift);
			bool isSpaceHeld = gameplay.isHeld(space);
			if (hor == 0 && ver == 0 && !isShiftHeld && !isSpaceHeld) return;

			if (hor != 0 || ver != 0) {
				Vector3 horizontalAxisUnitVector = transform.right;
				Vector3 verticalAxisUnitVector = transform.forward;
				Vector3 horizontalOffset = horizontalAxisUnitVector * hor;
				Vector3 verticalOffset = verticalAxisUnitVector * ver;
				Vector3 offset = horizontalOffset + verticalOffset;
				offset = offset.normalized;
				transform.position += offset * (Time.deltaTime * speed);
			}

			if (isSpaceHeld) 
				transform.position += Vector3.up * (Time.deltaTime * speed);
			
			if (isShiftHeld)
				transform.position -= Vector3.up * (Time.deltaTime * speed);
		}
    }
}
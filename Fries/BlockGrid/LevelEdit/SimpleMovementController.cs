using System;
using Fries.Chat;
using Fries.EvtSystem;
using Fries.InputDispatch;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Fries.BlockGrid.LevelEdit {
    public class SimpleMovementController : MonoBehaviour {
	    private InputLayer gameplay;

	    [SerializeField] private Rigidbody rigidbody;
	    
	    internal static SimpleMovementController player;
        private static KeyboardAxisInputModule KAIM;
		private static InputId horizontal;
		private static InputId vertical;
		private InputId shift;
		private InputId space;
		
		[SerializeField] private float speed = 5;
		public float GetSpeed() => speed;
		internal void SetSpeed(float speed) => this.speed = speed;
		private float defaultSpeed = 5;

		public void ChangeDefaultSpeed(float speed) {
			this.defaultSpeed = speed * 10;
			this.speed = speed * 50;
		}
		
		[EvtListener(typeof(InputEvents.BeforeKeyboardAxisSetup))]
		private static void loadInputId(KeyboardAxisInputModule module) {
			vertical = KeyboardAxisInputModule.get(module.getAxisCode("Vertical"));
			horizontal = KeyboardAxisInputModule.get(module.getAxisCode("Horizontal"));
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
				rigidbody.AddForce(offset * (speed * Time.deltaTime));
			}

			if (isSpaceHeld) 
				rigidbody.AddForce(Vector3.up * (speed * Time.deltaTime));
			
			if (isShiftHeld)
				rigidbody.AddForce(-Vector3.up * (speed * Time.deltaTime));
		}

		public void ResetSpeed() => speed = defaultSpeed;
    }
}
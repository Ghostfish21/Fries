using System;
using Fries.Chat;
using Fries.EvtSystem;
using Fries.InputDispatch;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace Fries.BlockGrid.LevelEdit {
    public class SimpleMovementController : MonoBehaviour {
	    private InputLayer gameplay;

	    [SerializeField] private Rigidbody rigidbody;
	    [SerializeField] private Collider collider;

	    public const int SPECTATOR = 3;
	    public const int CREATIVE = 1;
	    public const int SURVIVAL = 0;
	    private int gamemode = 1;
	    public void SetGamemode(int gamemode) {
		    this.gamemode = gamemode;
		    if (gamemode == SPECTATOR) 
			    collider.enabled = false;
		    else collider.enabled = true;
		    
		    if (gamemode == SURVIVAL) 
			    rigidbody.useGravity = true;
		    else rigidbody.useGravity = false;
	    }

	    internal static SimpleMovementController player;
        private static KeyboardAxisInputModule KAIM;
		private static InputId horizontal;
		private static InputId vertical;
		private InputId shift;
		private InputId space;
		private InputId spaceBar;
		
		[SerializeField] private float flySpeed = 5;
		[SerializeField] private float walkSpeed = 1.25f;
		public float GetFlySpeed() => flySpeed;
		public float GetWalkSpeed() => walkSpeed;
		internal void SetFlySpeed(float speed) => flySpeed = speed;
		internal void SetWalkSpeed(float speed) => walkSpeed = speed;
		private float defaultSpeed = 5;

		public void ChangeDefaultSpeed(float speed) {
			this.defaultSpeed = speed;
			this.flySpeed = speed * 200;
			this.walkSpeed = speed * 50;
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
			spaceBar = Key.Space;
			gameplay = InputLayer.get("Gameplay");
			
			rigidbody ??= GetComponent<Rigidbody>();
			collider ??= GetComponent<Collider>();
			
			if (!player) player = this;
			else Destroy(gameObject);
		}

		private void Update() {
			if (!gameplay) return;
			
			checkSpaceDoubleClick();
			
			float hor = gameplay.getFloat(horizontal);
			float ver = gameplay.getFloat(vertical);
			bool isShiftHeld = gameplay.isHeld(shift);
			bool isSpaceHeld = gameplay.isHeld(space);
			if (hor == 0 && ver == 0 && !isShiftHeld && !isSpaceHeld) return;

			float speed = flySpeed;
			if (gamemode == SURVIVAL) speed = walkSpeed;

			if (hor != 0 || ver != 0) {
				Vector3 horizontalAxisUnitVector = transform.right;
				Vector3 verticalAxisUnitVector = transform.forward;
				Vector3 horizontalOffset = horizontalAxisUnitVector * hor;
				Vector3 verticalOffset = verticalAxisUnitVector * ver;
				Vector3 offset = horizontalOffset + verticalOffset;
				offset = offset.x_z(0);
				offset = offset.normalized;
				rigidbody.AddForce(offset * (speed * Time.deltaTime));
			}

			if (gamemode == SURVIVAL) return;
			
			if (isSpaceHeld)
				rigidbody.AddForce(Vector3.up * (speed * Time.deltaTime));

			if (isShiftHeld)
				rigidbody.AddForce(-Vector3.up * (speed * Time.deltaTime));
		}

		private float currentSpaceTimer = 0;
		private float spaceTimerReset = 0.5f;
		private bool isSpaceClicked = false;
		private void checkSpaceDoubleClick() {
			if (isSpaceClicked && currentSpaceTimer > 0) 
				currentSpaceTimer -= Time.deltaTime;
			if (!gameplay.isDown(spaceBar)) {
				if (currentSpaceTimer <= 0 && isSpaceClicked) 
					isSpaceClicked = false;
				return;
			}
			
			if (!isSpaceClicked) {
				isSpaceClicked = true;
				currentSpaceTimer = spaceTimerReset;
			}
			else if (currentSpaceTimer > 0) {
				isSpaceClicked = false;
				if (gamemode == SURVIVAL) SetGamemode(CREATIVE);
				else if (gamemode == CREATIVE) SetGamemode(SURVIVAL);
			}
			
			if (currentSpaceTimer <= 0 && isSpaceClicked) 
				isSpaceClicked = false;
		}

		public void ResetFlySpeed() => flySpeed = defaultSpeed * 200;
		public void ResetWalkSpeed() => walkSpeed = defaultSpeed * 50;
    }
}
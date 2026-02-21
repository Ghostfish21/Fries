using Fries.Data;
using Fries.EvtSystem;
using Fries.InputDispatch;
using UnityEngine;

namespace Fries.BlockGrid.LevelEdit {
    public class SimpleCameraController : MonoBehaviour {
        private InputLayer gameplay;

        private static MouseAxisInputModule MAIM;
        private static InputId horizontal;
        private static InputId vertical;

        [EvtListener(typeof(InputEvents.BeforeMouseAxisSetup))]
        private static void loadInputId(MouseAxisInputModule module) {
            horizontal = MouseAxisInputModule.get(module.getAxisCode("Mouse X"));
            vertical = MouseAxisInputModule.get(module.getAxisCode("Mouse Y"));
        }
        
        [EvtListener(typeof(InputEvents.BeforeInputDispatcherSetup))]
        private static void dispatcherSetup(InputDispatcher dispatcher) {
            MAIM = new MouseAxisInputModule();
            dispatcher.addModule(MAIM);
        }

        [SerializeField] private Camera cam;
        [SerializeField] private float maxCameraPitch;
        [SerializeField] private float minCameraPitch;
        [SerializeField] private float mouseSensitivity = 3f;

        private float yaw;
        private float pitch;

        private static float angle0360(float a) {
            a %= 360f;
            if (a > 180f) a -= 360f;
            if (a < -180f) a += 360f;
            return a;
        }

        private void Awake() {
            var e = transform.rotation.eulerAngles;
            pitch = angle0360(e.x);
            yaw = angle0360(e.y);
            gameplay = InputLayer.get("Gameplay");
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void Update() {
            if (!gameplay) return;

            float hor = gameplay.getFloat(horizontal);
            float ver = gameplay.getFloat(vertical);

            yaw += hor * mouseSensitivity;
            pitch -= ver * mouseSensitivity;

            float minP = Mathf.Min(minCameraPitch, maxCameraPitch);
            float maxP = Mathf.Max(minCameraPitch, maxCameraPitch);
            pitch = Mathf.Clamp(pitch, minP, maxP);

            cam.transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
        }

        public float GetCameraYaw() => cam.transform.eulerAngles.y;
        public Facing GetFacing() => cam.transform.GetFacing();
        public Facing GetFacing(out Facing horizontal) => cam.transform.GetFacing(out horizontal);
    }
}
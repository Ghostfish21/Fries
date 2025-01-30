using UnityEngine;

namespace Fries.FbxPreviewFixer {

    public class FbxPreviewCorrectionSystem : MonoBehaviour {

        public void OnValidate() {
            # if UNITY_EDITOR
            ProjectWindowIconDrawer.setup();
            # endif
        }

    }


}


using UnityEngine;

namespace Fries.FbxFunctions.FbxPreviewFixer {

    public class FbxPreviewCorrectionSystem : MonoBehaviour {

        public void OnValidate() {
            # if UNITY_EDITOR
            ProjectWindowIconDrawer.setup();
            # endif
        }

    }


}


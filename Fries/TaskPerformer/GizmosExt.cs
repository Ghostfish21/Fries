using UnityEngine;

namespace Fries.TaskPerformer {
    public static class GizmosExt {
        public static void drawSphere(this TaskPerformer tp, Vector3 pos, float radiu) {
            tp.addGizmosTask(() => Gizmos.DrawSphere(pos, radiu));
        }
    }
}
using System;
using UnityEngine;

namespace Fries.BlockGrid.LevelEdit {
    public class SelectionDisplayer : MonoBehaviour {
        private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");
        [SerializeField] internal LineRenderer lineRenderer;
        
        private readonly Vector3[] _pts = new Vector3[18];

        private void Awake() {
            if (!lineRenderer) return;
            lineRenderer.useWorldSpace = true;
            lineRenderer.loop = false;
            lineRenderer.material.color = Color.red;
            lineRenderer.material.EnableKeyword("_EMISSION");
            lineRenderer.material.SetColor(EmissionColor, Color.red);
        }

        private void Start() {
            lineRenderer.startWidth = LevelEditor.Inst.BlockMap.UnitLength / 48f;
        }

        private bool shouldDraw() {
            if (BlockSelection.pos1 != null || BlockSelection.pos2 != null) return true;
            return false;
        }

        private void Update() {
            if (!lineRenderer) return;

            if (!shouldDraw()) {
                lineRenderer.enabled = false;
                return;
            }

            Vector3Int pos1, pos2;
            if (BlockSelection.pos1 != null) {
                pos1 = BlockSelection.pos1.Value;
                if (BlockSelection.pos2 != null) pos2 = BlockSelection.pos2.Value;
                else pos2 = pos1;
            } else if (BlockSelection.pos2 != null) {
                pos1 = BlockSelection.pos2.Value;
                if (BlockSelection.pos1 != null) pos2 = BlockSelection.pos1.Value;
                else pos2 = pos1;
            } else {
                lineRenderer.enabled = false;
                return;
            }

            lineRenderer.enabled = true;

            Vector3Int minGrid = new Vector3Int(
                Mathf.Min(pos1.x, pos2.x),
                Mathf.Min(pos1.y, pos2.y),
                Mathf.Min(pos1.z, pos2.z)
            );
            Vector3Int maxGrid = new Vector3Int(
                Mathf.Max(pos1.x, pos2.x),
                Mathf.Max(pos1.y, pos2.y),
                Mathf.Max(pos1.z, pos2.z)
            );

            Bounds bMin = LevelEditor.Inst.BlockMap.GetCellWorldPosBoundary(minGrid);
            Bounds bMax = LevelEditor.Inst.BlockMap.GetCellWorldPosBoundary(maxGrid);

            Bounds bounds = new Bounds();
            bounds.SetMinMax(bMin.min, bMax.max);

            Vector3 bmin = bounds.min;
            Vector3 bmax = bounds.max;

            Vector3 pxpypz = new Vector3(bmax.x, bmax.y, bmax.z);
            Vector3 mxpypz = new Vector3(bmin.x, bmax.y, bmax.z);
            Vector3 mxmypz = new Vector3(bmin.x, bmin.y, bmax.z);
            Vector3 pxmypz = new Vector3(bmax.x, bmin.y, bmax.z);

            Vector3 pxpymz = new Vector3(bmax.x, bmax.y, bmin.z);
            Vector3 mxpymz = new Vector3(bmin.x, bmax.y, bmin.z);
            Vector3 mxmymz = new Vector3(bmin.x, bmin.y, bmin.z);
            Vector3 pxmymz = new Vector3(bmax.x, bmin.y, bmin.z);

            _pts[0] = pxpypz;
            _pts[1] = mxpypz;
            _pts[2] = mxmypz;
            _pts[3] = pxmypz;
            _pts[4] = pxpypz;

            _pts[5] = pxpymz;

            _pts[6] = mxpymz;
            _pts[7] = mxmymz;
            _pts[8] = pxmymz;
            _pts[9] = pxpymz;

            _pts[10] = pxpypz;
            _pts[11] = mxpypz;
            _pts[12] = mxpymz;
            _pts[13] = mxmymz;
            _pts[14] = mxmypz;
            _pts[15] = pxmypz;
            _pts[16] = pxmymz;
            _pts[17] = pxpymz;

            lineRenderer.positionCount = _pts.Length;
            lineRenderer.SetPositions(_pts);
        }
    }
}
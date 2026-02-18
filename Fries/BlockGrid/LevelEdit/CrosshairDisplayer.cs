using System;
using UnityEngine;

namespace Fries.BlockGrid.LevelEdit {
    public class CrosshairDisplayer : MonoBehaviour {
        private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");
        [SerializeField] internal LineRenderer lineRenderer;
        
        internal Vector3Int? pointingGrid;
        internal Bounds? partBounds;
        
        private readonly Vector3[] _pts = new Vector3[18];

        private void Awake() {
            if (!lineRenderer) return;
            lineRenderer.useWorldSpace = true;
            lineRenderer.loop = false;
            lineRenderer.material.color = Color.white;
            lineRenderer.material.EnableKeyword("_EMISSION");
            lineRenderer.material.SetColor(EmissionColor, Color.white);
        }

        private void Start() {
            lineRenderer.startWidth = LevelEditor.Inst.BlockMap.UnitLength / 36f;
        }

        private void Update() {
            if (!lineRenderer) return;

            if (pointingGrid == null) {
                lineRenderer.enabled = false;
                return;
            }

            lineRenderer.enabled = true;

            Bounds bounds;
            if (partBounds != null) bounds = partBounds.Value;
            else bounds = LevelEditor.Inst.BlockMap.GetCellWorldPosBoundary(pointingGrid.Value);
            
            Vector3 extents = bounds.extents;
            Vector3 pxpypz = bounds.center + extents;
            Vector3 mxpypz = bounds.center + extents._yz(-extents.x);
            Vector3 mxmypz = bounds.center + extents.__z(-extents.x, -extents.y);
            Vector3 mxpymz = bounds.center + extents._y_(-extents.x, -extents.z);
            Vector3 mxmymz = bounds.center - extents;
            Vector3 pxmymz = bounds.center + extents.x__(-extents.y, -extents.z);
            Vector3 pxpymz = bounds.center + extents.xy_(-extents.z);
            Vector3 pxmypz = bounds.center + extents.x_z(-extents.y);

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
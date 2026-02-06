// using UnityEngine;
//
// namespace Fries.BlockGrid {
//     public class BlockMapQuery {
//         private BlockMap blockMap;
//         public BlockMapQuery(BlockMap blockMap) {
//             this.blockMap = blockMap;
//         }
//         
//         private const bool AT = true;
//         private const bool RANGE = false;
//
//         # region 位置检索参数
//         public void SetPositionRange(Vector3Int from, Vector3Int to) {
//             this.at = from;
//             this.to = to;
//             usingAtOrRangePos = RANGE;
//         }
//         public void SetPositionAt(Vector3Int at) {
//             this.at = at;
//             usingAtOrRangePos = AT;
//         }
//         private bool? usingAtOrRangePos = RANGE;
//         private Vector3Int? at;
//         private Vector3Int? to;
//
//         private void resetPositionParameters() {
//             at = null;
//             to = null;
//             usingAtOrRangePos = null;
//         }
//         # endregion
//         # region 方块类型检索参数
//         public void SetBlockTypeRange(int from, int to) {
//             blkTypeAt = from;
//             blkTypeTo = to;
//             usingAtOrRangeBlkType = RANGE;
//         }
//         public void SetBlockTypeAt(int at) {
//             blkTypeAt = at;
//             usingAtOrRangeBlkType = AT;
//         }
//         private int? blkTypeAt = null;
//         private int? blkTypeTo = null;
//         private bool? usingAtOrRangeBlkType = null;
//
//         public void resetBlockTypeParameters() {
//             blkTypeAt = null;
//             blkTypeTo = null;
//             usingAtOrRangeBlkType = null;
//         }
//         # endregion
//         # region 方块朝向检索参数
//         
//         # endregion
//         # region 自定义检索参数
//         # endregion
//         # region 顶部参数
//         # endregion
//         
//         public void ResetParameters() {
//             resetPositionParameters();
//             resetBlockTypeParameters();
//         }
//     }
// }
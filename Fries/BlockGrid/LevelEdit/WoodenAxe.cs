using UnityEngine;

namespace Fries.BlockGrid.LevelEdit {
    public class WoodenAxe : ITool {
        public override string ToString() => "WOODEN AXE";

        public int GetId() => 0;

        public void OnLMBClicked(BlockKey blockKey) => LevelEditor.writer.write($"//pos1 {blockKey.Position.ToString().Replace(" ", "")}");
        public void OnRMBClicked(BlockKey blockKey) => LevelEditor.writer.write($"//pos2 {blockKey.Position.ToString().Replace(" ", "")}");
        public void OnMBBClicked(BlockKey blockKey) { }
    }
}
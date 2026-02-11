using Fries.Chat;
using UnityEngine;

namespace Fries.BlockGrid.LevelEdit {
    public static class BlockSelection {
        internal static Vector3Int? pos1;
        internal static Vector3Int? pos2;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void reset() {
            pos1 = null;
            pos2 = null;
        }

        public static bool AssertSelectionIsComplete(out Vector3Int pos1, out Vector3Int pos2) {
            pos1 = Vector3Int.zero;
            pos2 = Vector3Int.zero;
            
            if (BlockSelection.pos1 == null || BlockSelection.pos2 == null) {
                LevelEditor.writer.write("The selection is not complete! Please set pos1 and pos2 first!");
                return false;
            }

            pos1 = BlockSelection.pos1.Value;
            pos2 = BlockSelection.pos2.Value;
            return true;
        }

        public static int GetSelectionSize(out int width, out int height, out int length) {
            width = 0;
            height = 0;
            length = 0;
            if (pos1 == null || pos2 == null) return 0;
            width = Mathf.Abs(pos1.Value.x - pos2.Value.x) + 1;
            height = Mathf.Abs(pos1.Value.y - pos2.Value.y) + 1;
            length = Mathf.Abs(pos1.Value.z - pos2.Value.z) + 1;
            return width * height * length;
        }
    }

    public class Pos1Comm : CommandBase {
        public Pos1Comm() : base("/pos1", "/Pos1") {}

        protected override void execute(string senderId, string[] args) {
            if (AssertArgumentLengthEquals(args, 1, out _)) {
                string[] posRaw = args[0].TrimStart('(').TrimEnd(')').Split(',');
                BlockSelection.pos1 = new Vector3Int(int.Parse(posRaw[0]), int.Parse(posRaw[1]), int.Parse(posRaw[2]));
            }
            else BlockSelection.pos1 = LevelEditor.Inst.BlockMap.GetCellPos(SimpleMovementController.player.transform.position);
            LevelEditor.writer.write($"Position 1 is set to ({BlockSelection.pos1.Value.x}, {BlockSelection.pos1.Value.y}, {BlockSelection.pos1.Value.z}) [{BlockSelection.GetSelectionSize(out int width, out int height, out int length)}: {width}, {height}, {length}]");
        }
    }
    
    public class Pos2Comm : CommandBase {
        public Pos2Comm() : base("/pos2", "/Pos2") {}

        protected override void execute(string senderId, string[] args) {
            if (AssertArgumentLengthEquals(args, 1, out _)) {
                string[] posRaw = args[0].TrimStart('(').TrimEnd(')').Split(',');
                BlockSelection.pos2 = new Vector3Int(int.Parse(posRaw[0]), int.Parse(posRaw[1]), int.Parse(posRaw[2]));
            }
            else BlockSelection.pos2 = LevelEditor.Inst.BlockMap.GetCellPos(SimpleMovementController.player.transform.position);
            LevelEditor.writer.write($"Position 2 is set to ({BlockSelection.pos2.Value.x}, {BlockSelection.pos2.Value.y}, {BlockSelection.pos2.Value.z}) [{BlockSelection.GetSelectionSize(out int width, out int height, out int length)}: {width}, {height}, {length}]");
        }
    }
}
using System;

namespace Fries.Data {
    [Flags]
    public enum Facing {
        none = 0,

        north = 1 << 0, // 1
        south = 1 << 1, // 2
        east = 1 << 2, // 4
        west = 1 << 3, // 8
        
        up = 1 << 4, // 16
        down = 1 << 5, // 32
        neither = 1 << 6 // 64
    }

    public static class FacingExt {
        private const Facing HorizontalMask = Facing.north | Facing.south | Facing.east | Facing.west;
        private const Facing VerticalMask   = Facing.up | Facing.down;
        private const Facing NeitherMask = Facing.neither;

        public static Facing combine(Facing horizontal, Facing vertical, bool isNeither) {
            if (isNeither) return NeitherMask | horizontal | vertical;
            return horizontal | vertical;
        }
    
        public static (Facing vertical, Facing horizontal) breakDown(this Facing combinedFacing) {
            Facing vertical = combinedFacing & VerticalMask;
            Facing horizontal = combinedFacing & HorizontalMask;
            Facing neither = combinedFacing & Facing.neither;
            if (neither == Facing.neither) 
                vertical |= neither;
            return (vertical, horizontal);
        }

        public static bool isNeither(Facing facing) {
            if ((facing & NeitherMask) == NeitherMask) return true;
            return false;
        }

        public static Facing getPitchDetail(Facing neither) {
            if ((neither & Facing.up) == Facing.up) return Facing.up;
            if ((neither & Facing.down) == Facing.down) return Facing.down;
            return Facing.none;
        }
    }
}
# if InputSys
using System;
using UnityEngine.InputSystem;

namespace Fries.InputDispatch {
    public readonly struct InputKind : IEquatable<InputKind> {
        private readonly RuntimeTypeHandle handle;

        private InputKind(RuntimeTypeHandle handle) => this.handle = handle;

        public static InputKind Of<TModule>() => new(typeof(TModule).TypeHandle);

        public bool Equals(InputKind other) => handle.Value == other.handle.Value;
        public override bool Equals(object obj) => obj is InputKind other && Equals(other);
        public override int GetHashCode() => handle.GetHashCode();
        public override string ToString() => Type.GetTypeFromHandle(handle).Name ?? "<UnknownKind>";
    }

    public readonly struct InputId : IEquatable<InputId> {
        public readonly InputKind kind;
        public readonly int code;

        public InputId(InputKind kind, int code) { this.kind = kind; this.code = code; }

        public bool Equals(InputId other) => kind.Equals(other.kind) && code == other.code;
        public override bool Equals(object obj) => obj is InputId other && Equals(other);
        public override int GetHashCode() => (kind.GetHashCode() * 397) ^ code;
        public override string ToString() => $"{kind}:{code}";
        
        public static implicit operator InputId(Key key) => new(InputKind.Of<KeyInputModule>(), (int)key);
        public static implicit operator InputId(MouseButton key) => new(InputKind.Of<MouseInputModule>(), (int)key);

        public DisplayInputId toDisplayInputId() {
            return new DisplayInputId {
                kind = kind.ToString(),
                code = code
            };
        }
    }

    [Serializable]
    public struct DisplayInputId : IEquatable<DisplayInputId> {
        public string kind;
        public int code;

        public bool Equals(DisplayInputId other) => kind == other.kind && code == other.code;
        public override bool Equals(object obj) => obj is DisplayInputId other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(kind, code);
    }
}
# endif
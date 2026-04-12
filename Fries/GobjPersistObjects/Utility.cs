using System;
using System.Globalization;
using UnityEngine;

namespace Fries.GobjPersistObjects {
    public static class Utility {
        public static ISaveSystem save { get; private set; } = null;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void reset() => save = null;
        public static void SetSaveSystem(ISaveSystem system) {
            if (save != null) return;
            save = system;
        }
    }
}
using System;
using System.Globalization;
using UnityEngine;

namespace Fries.GobjPersistObjects.Csv {
    public static class Utility {
        public static string ParseVec2(Vector2 vec) => $"{vec.x.ToString("R", CultureInfo.InvariantCulture)},{vec.y.ToString("R", CultureInfo.InvariantCulture)}";
        public static string ParseVec3(Vector3 vec) => $"{vec.x.ToString("R", CultureInfo.InvariantCulture)},{vec.y.ToString("R", CultureInfo.InvariantCulture)},{vec.z.ToString("R", CultureInfo.InvariantCulture)}";
        public static Vector2 ParseVec2(string raw) {
            string[] comps = raw.Split(',');
            if (comps.Length != 2) throw new FormatException($"Invalid Vector2: '{raw}'");
                
            float x = float.Parse(comps[0], CultureInfo.InvariantCulture);
            float y = float.Parse(comps[1], CultureInfo.InvariantCulture);
            return new Vector2(x, y);
        }
            
        public static Vector3 ParseVec3(string raw) {
            string[] comps = raw.Split(',');
            if (comps.Length != 3) throw new FormatException($"Invalid Vector3: '{raw}'");
                
            float x = float.Parse(comps[0], CultureInfo.InvariantCulture);
            float y = float.Parse(comps[1], CultureInfo.InvariantCulture);
            float z = float.Parse(comps[2], CultureInfo.InvariantCulture);
            return new Vector3(x, y, z);
        }
    }
}
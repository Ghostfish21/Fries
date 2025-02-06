using System;
using System.IO;
using UnityEngine;

namespace Fries {
    public class SystemUtils {
        public static long currentTimeMillis() {
            return DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
        }

        public static string projectName() {
            string dataPath = Application.dataPath;
            string projectName = new DirectoryInfo(dataPath).Parent.Name;
            return projectName;
        }
    }
}
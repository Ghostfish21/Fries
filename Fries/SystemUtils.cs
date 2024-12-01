using System;

namespace Fries {
    public class SystemUtils {
        public static long currentTimeMillis() {
            return DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
        }
    }
}
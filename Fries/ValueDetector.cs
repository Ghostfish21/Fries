using System;

namespace Fries {
    public class ValueDetector {
        private const int DIFF_DETECT_INT = 0;
        private const int DIFF_DETECT_FLOAT = 1;
        private int mode = -1;
        
        private float thresholdFloat;
        private int thresholdInt;
        private Func<float> valueGetterFloat;
        private Func<int> valueGetterInt;
        private float previousDetectedValueFloat = 0f;
        private int previousDetectedValueInt = 0;

        public void startDiffDetect(float threshold, Func<float> valueGetter) {
            mode = DIFF_DETECT_FLOAT;
            thresholdFloat = threshold;
            valueGetterFloat = valueGetter;
        }
        
        public void startDiffDetect(int threshold, Func<int> valueGetter) {
            mode = DIFF_DETECT_INT;
            thresholdInt = threshold;
            valueGetterInt = valueGetter;
        }

        public bool check() {
            switch (mode) {
                case DIFF_DETECT_FLOAT:
                    float currentValue = valueGetterFloat();
                    if (!(Math.Abs(currentValue - previousDetectedValueFloat) > thresholdFloat)) return false;
                    previousDetectedValueFloat = currentValue;
                    return true;
                case DIFF_DETECT_INT:
                    int currentValueInt = valueGetterInt();
                    if (!(Math.Abs(currentValueInt - previousDetectedValueInt) > thresholdInt)) return false;
                    previousDetectedValueInt = currentValueInt;
                    return true;
            }

            return false;
        }
    }
}
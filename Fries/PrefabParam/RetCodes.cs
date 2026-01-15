namespace Fries.PrefabParam {
    public static class RetCodes {
        // Unexpected behaviours, aka bugs
        public const int EBUG = -4;
        // Parameter with incorrect type exists
        public const int EPIT = -3;
        // Parameters do not exist, this prefab doesn't have its parameters
        public const int EPNE = -2;
        // Index out of range
        public const int EIOoR = -1;
        // Null Parameter
        public const int NP = 0;
        public const int SUCCESS = 1;
    }
}
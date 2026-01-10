using System.Collections.Generic;

namespace Fries.PrefabParam {
    internal class ShortTermParams {
        internal int paramsId;
        internal List<object> parameters;

        public ShortTermParams(int instanceId, List<object> objects) {
            paramsId = instanceId;
            parameters = objects;
        }
    }
}
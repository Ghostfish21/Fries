using System.Collections.Generic;

namespace Fries.Inspector.CustomDataRows {
    public class ListWrapper<T> : Unwrapper {
        public List<T> list = new();
        public object unwrap() {
            return list;
        }
    }
}
using UnityEngine;

namespace Fries {
    public class UidIncrementor {
        private ulong uid = 0;

        public UidIncrementor(ulong initUid = 0) {
            this.uid = initUid;
        }

        private ulong GetUid() {
            uid++;
            return uid;
        }

    }
}
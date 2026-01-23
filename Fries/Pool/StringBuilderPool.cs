using System;
using System.Collections.Generic;
using System.Text;

namespace Fries.Pool {
    public class StringBuilderPool : APool<StringBuilder> {
        public StringBuilderPool(int size) : base(() => new StringBuilder(), size) {
        }

        protected override void deactivateCore(StringBuilder what) {
            what.Clear();
        }

        protected override void activateCore(StringBuilder what) {
            what.Clear();
        }
    }
}
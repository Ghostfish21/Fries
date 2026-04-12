using System.Text;

namespace Fries.GobjPersistObjects.Csv.CsvSaveFormat {
    public interface IRow {
        void writeInto(StringBuilder sb);
    }

    public static class IRowExt {
        public static void writeInto(this IRow row, StringBuilder sb, ISlot[] slots) {
            if (slots == null) return;
            int len = slots.Length;
            if (len == 0) return;
            
            slots[0].writeInto(sb);
            for (int i = 1; i < len; i++) {
                sb.Append(',');
                slots[i].writeInto(sb);
            }
        }
    }
}
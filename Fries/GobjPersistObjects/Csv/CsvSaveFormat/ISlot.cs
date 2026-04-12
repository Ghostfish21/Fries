using System.Text;

namespace Fries.GobjPersistObjects.Csv.CsvSaveFormat {
    public interface ISlot {
        void writeInto(StringBuilder sb);
    }

    public static class ISlotExt {
        public static void writeInto(this ISlot slot, StringBuilder sb, string toWrite) {
            if (string.IsNullOrWhiteSpace(toWrite)) return;

            string content = toWrite.Trim();

            bool mustQuote = false;
            for (int i = 0; i < content.Length; i++) {
                char c = content[i];
                if (c == ',' || c == '"' || c == '\r' || c == '\n') {
                    mustQuote = true;
                    break;
                }
            }

            if (!mustQuote) {
                sb.Append(content);
                return;
            }

            sb.Append('"');
            for (int i = 0; i < content.Length; i++) {
                char c = content[i];
                if (c == '"') sb.Append("\"\""); // escape quote
                else sb.Append(c);
            }
            sb.Append('"');
        }
    }
}
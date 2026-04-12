using System;
using System.Collections;
using System.Text;

namespace Fries.GobjPersistObjects.Csv.Serialize {
    public static class ListSerializer {
        public static string SerializeList(object list, Func<object, string> serializer) {
            if (list == null) return "";

            if (list is not IList ilist)
                throw new ArgumentException("list must be a List<T> (IList).", nameof(list));

            var sb = new StringBuilder(2 + ilist.Count * 8);
            sb.Append('[');

            for (int i = 0; i < ilist.Count; i++) {
                if (i != 0) sb.Append(',');

                object elem = ilist[i];
                if (elem == null) {
                    sb.Append("null");
                    continue;
                }
                
                string ser = serializer(elem) ?? string.Empty;

                sb.Append('"');
                appendJsonEscaped(sb, ser);
                sb.Append('"');
            }

            sb.Append(']');
            return sb.ToString();
        }
        
        private static void appendJsonEscaped(StringBuilder sb, string s) {
            for (int i = 0; i < s.Length; i++) {
                char c = s[i];
                switch (c) {
                    case '\\': sb.Append("\\\\"); break;
                    case '"':  sb.Append("\\\""); break;
                    case '\b': sb.Append("\\b"); break;
                    case '\f': sb.Append("\\f"); break;
                    case '\n': sb.Append("\\n"); break;
                    case '\r': sb.Append("\\r"); break;
                    case '\t': sb.Append("\\t"); break;
                    default:
                        if (c < 0x20) {
                            sb.Append("\\u");
                            sb.Append(((int)c).ToString("x4"));
                        } else {
                            sb.Append(c);
                        }
                        break;
                }
            }
        }
    }
}
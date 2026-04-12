using System.Collections.Generic;
using System;
using System.Collections;
using System.Text;

namespace Fries.GobjPersistObjects.Csv.Serialize {
    public static class ListDeserializer {
        public static object DeserializeList(string raw, Type elemType, Func<string, object> deserializer) {
            // create List<T>
            var listType = typeof(List<>).MakeGenericType(elemType);
            var list = (IList)Activator.CreateInstance(listType);

            if (raw == null) return list;
            
            int i = 0;
            skipWs(raw, ref i);
            expect(raw, ref i, '[');
            skipWs(raw, ref i);

            if (tryConsume(raw, ref i, ']')) {
                skipWs(raw, ref i);
                if (i != raw.Length) throwAt("Trailing characters", raw, i);
                return list;
            }

            while (true) {
                skipWs(raw, ref i);

                if (tryConsumeLiteralNull(raw, ref i)) {
                    object elemObj = deserializer("null");
                    list.Add(elemObj);
                } else {
                    if (i >= raw.Length || raw[i] != '"') throwAt("Expected '\"' or null", raw, i);
                    string elemString = readJsonString(raw, ref i);      // unescaped
                    object elemObj = deserializer(elemString);
                    list.Add(elemObj);
                }

                skipWs(raw, ref i);

                if (tryConsume(raw, ref i, ',')) continue;
                if (tryConsume(raw, ref i, ']')) break;

                throwAt("Expected ',' or ']'", raw, i);
            }

            skipWs(raw, ref i);
            if (i != raw.Length) throwAt("Trailing characters", raw, i);
            return list;
        }

        private static void skipWs(string s, ref int i) {
            while (i < s.Length) {
                char c = s[i];
                if (c == ' ' || c == '\t' || c == '\r' || c == '\n') i++;
                else break;
            }
        }

        private static bool tryConsume(string s, ref int i, char ch) {
            if (i < s.Length && s[i] == ch) {
                i++;
                return true;
            }

            return false;
        }

        private static void expect(string s, ref int i, char ch) {
            if (!tryConsume(s, ref i, ch)) throwAt($"Expected '{ch}'", s, i);
        }

        private static bool tryConsumeLiteralNull(string s, ref int i) {
            // expects: null
            if (i + 4 <= s.Length && s[i] == 'n' && s[i + 1] == 'u' && s[i + 2] == 'l' && s[i + 3] == 'l') {
                i += 4;
                return true;
            }

            return false;
        }

        private static string readJsonString(string s, ref int i) {
            // expects s[i] == '"'
            i++; // skip opening "
            var sb = new StringBuilder();

            while (i < s.Length) {
                char c = s[i++];

                if (c == '"') return sb.ToString();

                if (c != '\\') {
                    sb.Append(c);
                    continue;
                }

                if (i >= s.Length) throwAt("Invalid escape sequence", s, i);

                char esc = s[i++];
                switch (esc) {
                    case '\\': sb.Append('\\'); break;
                    case '"':  sb.Append('"'); break;
                    case 'b':  sb.Append('\b'); break;
                    case 'f':  sb.Append('\f'); break;
                    case 'n':  sb.Append('\n'); break;
                    case 'r':  sb.Append('\r'); break;
                    case 't':  sb.Append('\t'); break;
                    case 'u': {
                        if (i + 4 > s.Length) throwAt("Invalid \\u escape", s, i);
                        int code = (hex(s[i])   << 12) | (hex(s[i + 1]) << 8)  | (hex(s[i + 2]) << 4)  | hex(s[i + 3]);
                        i += 4;
                        sb.Append((char)code);
                        break;
                    }
                    default: throwAt($"Unknown escape '\\{esc}'", s, i - 1); break;
                }
            }

            throwAt("Unterminated string", s, i);
            return null;
        }

        private static int hex(char c) {
            if (c >= '0' && c <= '9') return c - '0';
            if (c >= 'a' && c <= 'f') return 10 + (c - 'a');
            if (c >= 'A' && c <= 'F') return 10 + (c - 'A');
            throw new FormatException($"Invalid hex digit: '{c}'");
        }

        private static void throwAt(string msg, string s, int i) =>
            throw new FormatException($"{msg} at index {i} in: {s}");
    }
}
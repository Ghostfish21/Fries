namespace Fries.Inspector {
    public class StringSso : SerializableSysObject {
        public string content;
        
        public StringSso(string content) {
            this.content = content;
        }

        public static implicit operator StringSso(string s) {
            return new StringSso(s);
        }
        
        public override bool Equals(object obj) {
            if (obj is not StringSso other)
                return false;
            return content == other.content;
        }

        public override int GetHashCode() {
            int hash = 17;
            hash = hash * 23 + (content != null ? content.GetHashCode() : 0);
            return hash;
        }

        public static bool operator ==(StringSso a, StringSso b) {
            if (ReferenceEquals(a, b))
                return true;
            if (a is null || b is null)
                return false;
            return a.Equals(b);
        }

        public static bool operator !=(StringSso a, StringSso b) {
            return !(a == b);
        }

        public override string ToString() {
            return content;
        }

        public override T get<T>() {
            return (T)(object)content;
        }
        
        public override void set<T>(T value) {
            content = (string)(object)value;
        }
    }
}
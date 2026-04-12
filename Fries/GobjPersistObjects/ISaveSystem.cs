using System.Collections.Generic;

namespace Fries.GobjPersistObjects {
    public interface ISaveSystem {
        internal static ISaveSystem saveSystem;

        void PutMetaData(string key, object value);
        void PutEntry(string uid, Dictionary<string, (bool, object)> data);
        string Serialize();
        void Deserialize(string csvStr);
        void Flush();
        void LoadScene(GpoManager gpoManager);
        void RemoveEntry(string uid);
    }
}
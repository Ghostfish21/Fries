namespace Fries.GobjPersistObjects.Csv.Serialize {
    public interface ISaveSerializable {
        string GetTypeName();
        string Serialize();
        object Deserialize(string value);
    }
}
namespace Fries.EventFunctions {
    public interface EventFunctionSubscriber { }

    public static class EventFunctionSubscriberExtensions {
        public static void subscribe(this EventFunctionSubscriber subscriber) {
            EventFunctionSystem.inst.record(subscriber);
        }
        public static void unsubscribe(this EventFunctionSubscriber subscriber) {
            EventFunctionSystem.inst.remove(subscriber);
        }
    }
}
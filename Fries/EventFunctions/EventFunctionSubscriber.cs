namespace Fries.EventFunctions {
    public interface EventFunctionSubscriber {
        void onSubscribe();
        void onUnsubscribe();
    }

    public static class EventFunctionSubscriberExtensions {
        public static void subscribe(this EventFunctionSubscriber subscriber) {
            EventFunctionSystem.inst.record(subscriber);
            subscriber.onSubscribe();
        }
        public static void unsubscribe(this EventFunctionSubscriber subscriber) {
            EventFunctionSystem.inst.remove(subscriber);
            subscriber.onUnsubscribe();
        }
    }
}
namespace Shared.Settings
{
    public static class RabbitMQSettings
    {
        public const string StateMachineQueue = "state-machine-queue";
        public const string Stock_OrderCreatedEventQueue = "stock-order-created-event-queue";
        public const string Order_OrderCompletedEventQueue = "order-order-completed-event-queue";
        public const string Order_OrderFailedEventQueue = "order-order-failed-evenet-queue";
        public const string Stock_RollBackMessageQueue = "stock-rollback-message-queue";
        public const string Payment_StartedEventQueue = "payment-started-event-queue";
    }
}

using Contracts.Events;
using MassTransit;


namespace CarsTradeWorker.Consumers
{
    /// <summary>
    /// Консумер для обработки события "Удалён заказ"
    /// </summary>
    public class OrderDeletedConsumer : IConsumer<OrderDeleted>
    {
        private readonly ILogger<OrderDeletedConsumer> _logger;
        public OrderDeletedConsumer(ILogger<OrderDeletedConsumer> logger)
        {
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<OrderDeleted> context)
        {
            OrderDeleted message = context.Message;

            _logger.LogInformation("Получено событие: заказ удалён! ID заказа = {OrderId}, Покупатель ID = {BuyerId}, Сумма = {TotalAmount}", message.OrderId, message.BuyerId, message.TotalAmount);

            await Task.CompletedTask;
        }
    }
}

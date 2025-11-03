using MassTransit;
using Contracts.Events;


namespace CarsTradeWorker.Consumers
{
    /// <summary>
    /// Консумер для обработки события "Создан заказ"
    /// </summary>
    public class OrderCreatedConsumer : IConsumer<OrderCreated>
    {
        private readonly ILogger<OrderCreatedConsumer> _logger;
        public OrderCreatedConsumer(ILogger<OrderCreatedConsumer> logger) 
        {
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<OrderCreated> context) 
        {
            OrderCreated message = context.Message;

            _logger.LogInformation("Получено событие: заказ создан! ID заказа = {OrderId}, Покупатель ID = {BuyerId}, Сумма = {TotalAmount}", message.OrderId, message.BuyerId, message.TotalAmount);

            await Task.CompletedTask;
        }
    }
}

using CarsTradeAPI.Entities;
using CarsTradeAPI.Data;
using Microsoft.EntityFrameworkCore;


namespace CarsTradeAPI.Features.OrdersOperation.OrdersRepository
{
    /// <summary>
    /// Реализация репозитория для работы с заказами в базе данных
    /// </summary>
    public class ImplementationOrderRepository : IOrderRepository
    {
        private readonly CarsTradeDbContext _carsTradeDbContext;
        private readonly ILogger<ImplementationOrderRepository> _logger; 
        public ImplementationOrderRepository(CarsTradeDbContext carsTradeDbContext, ILogger<ImplementationOrderRepository> logger)
        {
            _carsTradeDbContext = carsTradeDbContext;
            _logger = logger;
        }


        /// <inheritdoc/>
        public async Task<Order> CreateOrderAsync(Order order)
        {
            _logger.LogInformation("Этап создания заказа в репозитории");
            try
            {
                await _carsTradeDbContext.Orders.AddAsync(order);
                await _carsTradeDbContext.SaveChangesAsync();
                _logger.LogInformation("Заказ успешно создан с ID: {OrderId}", order.OrderId);
                return order;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при попытке создать заказ в базе данных");
                throw new Exception("Ошибка при попытке создать заказ в базе данных", ex);
            }
        }


        /// <inheritdoc/>
        public async Task<List<Order>> GetAllOrdersAsync()
        {
            _logger.LogInformation("Этап получения всех заказов в репозитории");
            try
            {
                List<Order> orders = await _carsTradeDbContext.Orders.Include(o => o.OrderItems).ThenInclude(oi => oi.CarModel).ToListAsync();
                _logger.LogInformation("Все заказы успешно получены, количество: {Count}", orders.Count);
                return orders;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении всех заказов");
                throw new Exception("Ошибка при получении всех заказов", ex);
            }
        }


        /// <inheritdoc/>
        public async Task<Order?> GetOrderByIdAsync(Guid orderId)
        {
            _logger.LogInformation("Этап получения Заказа с ID: {orderId} в репозитории", orderId);
            try
            {
                Order? order = await _carsTradeDbContext.Orders.Include(o => o.OrderItems).ThenInclude(oi => oi.CarModel).FirstOrDefaultAsync(o => o.OrderId == orderId);
                if (order == null)
                {
                    _logger.LogWarning("Заказ с ID: {orderId} не найден", orderId);
                    return null;
                }
                _logger.LogInformation("Заказ с ID: {orderId} успешно получен", orderId);
                return order;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,"Ошибка при получении заказа с ID {orderId}", orderId);
                throw new Exception($"Ошибка при получении заказа с ID {orderId}", ex); ;
            }
        }


        /// <inheritdoc/>
        public async Task<Order?> DeleteOrderAsync(Guid orderId)
        {
            _logger.LogInformation("Этап удаления заказа с ID {OrderId} в репозитории", orderId);
            // Проверка существования заказа
            Order? order = await _carsTradeDbContext.Orders.FirstOrDefaultAsync(o => o.OrderId == orderId);
            if (order == null)
            {
                _logger.LogWarning("Попытка удалить несуществующий заказ с ID: {orderId}", orderId);
                return null;
            }
            try
            {
                _carsTradeDbContext.Orders.Remove(order!);
                await _carsTradeDbContext.SaveChangesAsync();
                _logger.LogInformation("Заказ успешно удален с ID: {orderId}", orderId);
                return order;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при удалении заказа с ID: {OrderId}", orderId);
                throw;
            }
        }


        /// <inheritdoc/>
        public async Task<bool> CheckOrderBuyerAsync(Guid buyerId)
        {
            _logger.LogInformation("Этап проверки существования покупателя с ID {buyerId} в репозитории", buyerId);
            try
            {
                bool result = await _carsTradeDbContext.Buyers.AnyAsync(o => o.BuyerId == buyerId);
                _logger.LogInformation("Проверка существования прошла успешно, результат {result}", result);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,"Ошибка при получении информации о существовании покупателя с ID: {buyerId}", buyerId);
                throw new Exception($"Ошибка при получении информации о существовании покупателя с ID:{buyerId}", ex);
            }
        }


        /// <inheritdoc/>
        public async Task<bool> CheckOrderEmployeeAsync(Guid employeeId)
        {
            _logger.LogInformation("Этап проверки существования работника с ID {employeeId} в репозитории", employeeId);
            try
            {
                bool result = await _carsTradeDbContext.Employees.AnyAsync(o => o.EmployeeId == employeeId);
                _logger.LogInformation("Проверка существования прошла успешно, результат {result}", result);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении информации о существовании работника с ID: {employeeId}", employeeId);
                throw new Exception($"Ошибка при получении информации о существовании работника с ID:{employeeId}", ex);
            }
        }


        /// <inheritdoc/>
        public async Task<bool> CheckCarModelAsync(Guid? carModelId)
        {
            _logger.LogInformation("Этап проверки существования машины с ID {carModelId} в репозитории", carModelId);
            try
            {
                bool result =  await _carsTradeDbContext.CarModels.AnyAsync(cm => cm.CarModelId == carModelId);
                _logger.LogInformation("Проверка существования прошла успешно, результат {result}", result);
                return result;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex,"Ошибка при получении информации о существовании машины с ID: {carModelId}", carModelId);
                throw new Exception($"Ошибка при получении информации о существовании машины с ID:{carModelId}", ex);
            }
        }


        /// <inheritdoc/>
        public async Task<Order> EditOrderAsync(Order order)
        {
            _logger.LogInformation("Этап обновления заказа с ID: {OrderId} в репозитории", order.OrderId);
            try
            {
                _carsTradeDbContext.Orders.Update(order);
                await _carsTradeDbContext.SaveChangesAsync();
                _logger.LogInformation("Заказ с ID: {OrderId} успешно обновлена", order.OrderId);
                return order;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обновлении Заказа с ID: {OrderId}", order.OrderId);
                throw new Exception($"Ошибка при обновлении Заказа с ID:{order.OrderId}", ex);
            }
        }


        /// <inheritdoc/>
        public async Task<decimal> GetCarPrice(Guid? carModelId)
        {
            _logger.LogInformation("Этап получения цены машины с ID: {carModelId} в репозитории", carModelId);
            try
            {
                CarModel? carModel = await _carsTradeDbContext.CarModels.FirstOrDefaultAsync(cm => cm.CarModelId == carModelId);
                if (carModel == null)
                {
                    _logger.LogWarning("Модель с ID: {CarModelId} не найдена", carModelId);
                    throw new Exception("Модель автомобиля с таким ID не найдена");
                }
                _logger.LogInformation("Цена машины с ID: {CarModelId} успешно получена", carModelId);
                return carModel.CarPrice;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,"Ошибка при получении цены машины с ID: {CarModelId}", carModelId);
                throw new Exception($"Ошибка при получении цены машины с ID: {carModelId}", ex);
            }
        }


        /// <inheritdoc/>
        public async Task<Order> AddOrderItem(Guid orderId, OrderItem newItem)
        {
            _logger.LogInformation("Этап добавления нового элемента заказа в заказ с ID: {orderId} в репозитории", orderId);
            try
            {
                Order? order = await _carsTradeDbContext.Orders.Include(o => o.OrderItems).FirstOrDefaultAsync(o => o.OrderId == orderId);
                if (order == null)
                {
                    _logger.LogWarning("Заказ с ID: {orderId} не найден", orderId);
                    throw new Exception("Заказа с таким ID не был найден");
                }

                newItem.OrderId = orderId;
                order.OrderItems.Add(newItem);
                order.OrderPrice = order.OrderItems?.Sum(item => item.UnitPrice * item.OrderQuantity) ?? 0;
                await _carsTradeDbContext.SaveChangesAsync();
                _logger.LogInformation("Элемент заказа был уcпешно добавлен в заказ с ID: {orderId}", orderId);
                return order;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,"Ошибка при попытке добавить элемент заказа в заказ с ID: {orderId}", orderId);
                throw new Exception($"Ошибка при попытке добавить элемент заказа в заказ с ID: {orderId}", ex);
            }
        }


        /// <inheritdoc/>
        public async Task<Order> AddOrderItems(Order order) 
        {
            _logger.LogInformation("Этап добавления новых элементов заказа в заказ с ID: {OrderId} в репозитории", order.OrderId);
            try
            {
                _carsTradeDbContext.Orders.Update(order);
                order.OrderPrice = order.OrderItems?.Sum(item => item.UnitPrice * item.OrderQuantity) ?? 0;
                await _carsTradeDbContext.SaveChangesAsync();
                _logger.LogInformation("Элементы заказа были уcпешно добавлены в заказ с ID: {OrderId}", order.OrderId);
                return order;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,"Ошибка при попытке добавить элементы заказа в заказ с ID: {OrderId}", order.OrderId);
                throw new Exception($"Ошибка при попытке добавить элементы заказа в заказ с ID: {order.OrderId}", ex);
            }
        }
    }
}

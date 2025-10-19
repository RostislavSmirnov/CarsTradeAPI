using CarsTradeAPI.Entities;

namespace CarsTradeAPI.Features.OrdersOperation.OrdersRepository
{
    /// <summary>
    /// Интерфейс, определяющий контракт для работы с заказами.
    /// Содержит методы для создания, изменения, удаления и проверки заказов.
    /// </summary>
    public interface IOrderRepository
    {
        /// <summary>
        /// Создаёт новый заказ в базе данных.
        /// </summary>
        /// <param name="order">Объект заказа для создания</param>
        /// <returns>Созданный объект заказа</returns>
        /// <exception cref="Exception">При ошибке сохранения заказа</exception>
        Task<Order> CreateOrderAsync(Order order);


        /// <summary>
        /// Удаляет заказ по его ID.
        /// </summary>
        /// <param name="orderId">ID заказа для удаления</param>
        /// <returns>Удалённый заказ или null, если заказ не найден</returns>
        /// <exception cref="Exception">При ошибке удаления</exception>
        Task<Order?> DeleteOrderAsync(Guid orderId);


        /// <summary>
        /// Обновляет данные существующего заказа.
        /// </summary>
        /// <param name="order">Обновлённый объект заказа</param>
        /// <returns>Обновлённый заказ</returns>
        /// <exception cref="Exception">При ошибке обновления</exception>
        Task<Order> EditOrderAsync(Order order);


        /// <summary>
        /// Получает заказ по его ID.
        /// </summary>
        /// <param name="orderId">ID заказа</param>
        /// <returns>Объект заказа или null, если не найден</returns>
        /// <exception cref="Exception">При ошибке поиска</exception>
        Task<Order?> GetOrderByIdAsync(Guid orderId);


        /// <summary>
        /// Возвращает список всех заказов.
        /// </summary>
        /// <returns>Список заказов</returns>
        /// <exception cref="Exception">При ошибке выборки</exception>
        Task<List<Order>> GetAllOrdersAsync();


        /// <summary>
        /// Проверяет существование покупателя по его ID.
        /// </summary>
        /// <param name="buyerId">ID покупателя</param>
        /// <returns>true — если покупатель существует, иначе false</returns>
        /// <exception cref="Exception">При ошибке поиска</exception>
        Task<bool> CheckOrderBuyerAsync(Guid buyerId);


        /// <summary>
        /// Проверяет существование сотрудника по его ID.
        /// </summary>
        /// <param name="employeeId">ID сотрудника</param>
        /// <returns>true — если сотрудник существует, иначе false</returns>
        /// <exception cref="Exception">При ошибке поиска</exception>
        Task<bool> CheckOrderEmployeeAsync(Guid employeeId);


        /// <summary>
        /// Проверяет существование модели автомобиля по её ID.
        /// </summary>
        /// <param name="carModelId">ID модели автомобиля</param>
        /// <returns>true — если модель существует, иначе false</returns>
        /// <exception cref="Exception">При ошибке поиска</exception>
        Task<bool> CheckCarModelAsync(Guid? carModelId);


        /// <summary>
        /// Получает цену автомобиля по ID модели.
        /// </summary>
        /// <param name="carModelId">ID модели автомобиля</param>
        /// <returns>Цена автомобиля</returns>
        /// <exception cref="Exception">Если модель не найдена или при ошибке поиска</exception>
        Task<decimal> GetCarPrice(Guid? carModelId);


        /// <summary>
        /// Добавляет новый элемент заказа в существующий заказ.
        /// </summary>
        /// <param name="orderId">ID заказа</param>
        /// <param name="orderItem">Новый элемент заказа</param>
        /// <returns>Обновлённый заказ</returns>
        /// <exception cref="Exception">Если заказ не найден или при ошибке сохранения</exception>
        Task<Order> AddOrderItem(Guid orderId, OrderItem orderItem);


        /// <summary>
        /// Добавляет список элементов в заказ.
        /// </summary>
        /// <param name="order">Объект заказа с элементами</param>
        /// <returns>Обновлённый заказ</returns>
        /// <exception cref="Exception">При ошибке добавления</exception>
        Task<Order> AddOrderItems(Order order);
    }
}
using CarsTradeAPI.Entities;


namespace CarsTradeAPI.Features.BuyerOperation.BuyerRepository
{
    /// <summary>
    /// Репозиторий для работы с покупателями в базе данных
    /// Обеспечивает абстракцию над EF Core для операций CRUD
    /// </summary>
    public interface IBuyerRepository
    {
        /// <summary>
        /// Создает нового покупателя в системе
        /// </summary>
        /// <param name="buyer">Данные покупателя для создания</param>
        /// <returns>Созданный покупатель с заполненным BuyerId</returns>
        Task<Buyer> CreateBuyerAsync(Buyer buyer);


        /// <summary>
        /// Удаляет покупателя по идентификатору
        /// </summary>
        /// <param name="buyerId">UUID покупателя для удаления</param>
        /// <returns>Удаленный покупатель или null если не найден</returns>
        /// <exception cref="ArgumentException">Если покупатель не существует</exception>
        Task<Buyer?> DeleteBuyerAsync(Guid buyerId);


        /// <summary>
        /// Обновляет данные существующего покупателя
        /// </summary>
        /// <param name="buyer">Обновленные данные покупателя</param>
        /// <returns>Обновленный покупатель</returns>
        /// <exception cref="ArgumentException">Если покупатель не существует</exception>
        Task<Buyer> UpdateBuyerAsync(Buyer buyer);


        /// <summary>
        /// Проверяет, существует ли покупатель с указанным номером телефона
        /// Используется для обеспечения уникальности телефонных номеров
        /// </summary>
        /// <param name="phoneNumber">Номер телефона для проверки</param>
        /// <returns>True если номер уже занят, иначе False</returns>
        Task<bool> CheckBuyerPhoneNumberAsync(string phoneNumber);


        /// <summary>
        /// Проверяет, существует ли покупатель с указанным email
        /// Используется для обеспечения уникальности email адресов
        /// </summary>
        /// <param name="email">Email для проверки</param>
        /// <returns>True если email уже занят, иначе False</returns>
        Task<bool> CheckBuyerEmailAsync(string email); // 👈 исправлена опечатка в названии


        /// <summary>
        /// Получает покупателя по идентификатору
        /// </summary>
        /// <param name="buyerId">UUID покупателя</param>
        /// <returns>Найденный покупатель или null</returns>
        Task<Buyer?> GetBuyerByIdAsync(Guid buyerId);


        /// <summary>
        /// Получает всех покупателей из системы
        /// </summary>
        /// <returns>Список всех покупателей</returns>
        Task<List<Buyer>> GetAllBuyersAsync();
    }
}

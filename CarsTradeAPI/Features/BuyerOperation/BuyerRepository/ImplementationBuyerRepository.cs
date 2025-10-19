using CarsTradeAPI.Entities;
using CarsTradeAPI.Data;
using Microsoft.EntityFrameworkCore;


namespace CarsTradeAPI.Features.BuyerOperation.BuyerRepository
{
    /// <summary>
    /// Реализация репозитория для работы с покупателями в базе данных
    /// </summary>
    public class ImplementationBuyerRepository : IBuyerRepository
    {
        private readonly CarsTradeDbContext _carsTradeDbContext;
        private readonly ILogger<ImplementationBuyerRepository> _logger;
        public ImplementationBuyerRepository(CarsTradeDbContext carsTradeDbContext, ILogger<ImplementationBuyerRepository> logger)
        {
            _carsTradeDbContext = carsTradeDbContext;
            _logger = logger;
        }


        /// <inheritdoc/>
        public async Task<Buyer> CreateBuyerAsync(Buyer buyer)
        {
            _logger.LogInformation("Этап создания покупателя с ID: {BuyerId} в репозитории", buyer.BuyerId);
            try
            {
                await _carsTradeDbContext.Buyers.AddAsync(buyer);
                await _carsTradeDbContext.SaveChangesAsync();
                _logger.LogInformation("Покупатель успешно создан с ID: {BuyerId}", buyer.BuyerId);
                return buyer;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при создании покупателя с ID: {BuyerId}", buyer.BuyerId);
                throw new Exception("Ошибка при создании покупателя", ex);
            }
        }


        /// <inheritdoc/>
        public async Task<bool> CheckBuyerPhoneNumberAsync(string buyerPhoneNumber)
        {
            _logger.LogInformation("Этап проверки уникальности номера телефона: {PhoneNumber} в репозитории", buyerPhoneNumber);
            try
            {
                bool result = await _carsTradeDbContext.Buyers.AnyAsync(x => x.PhoneNumber == buyerPhoneNumber);
                _logger.LogInformation("Проверка уникальности номера телефона {PhoneNumber} завершена. Результат: {Result}", buyerPhoneNumber, result);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при проверке уникальности номера телефона: {PhoneNumber}", buyerPhoneNumber);
                throw new Exception("Ошибка при проверке уникальности номера телефона", ex);
            }
        }


        /// <inheritdoc/>
        public async Task<bool> CheckBuyerEmailAsync(string buyerEmail) 
        {
            _logger.LogInformation("Этап проверки уникальности Email: {buyerEmail} в репозитории", buyerEmail);
            try
            {
                bool result = await _carsTradeDbContext.Buyers.AnyAsync(x => x.BuyerEmail == buyerEmail);
                _logger.LogInformation("Проверка уникальности Email {buyerEmail} завершена. Результат: {Result}", buyerEmail, result);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при проверке уникальности Email: {buyerEmail}", buyerEmail);
                throw new Exception("Ошибка при проверке уникальности Email", ex);
            }
        }


        /// <inheritdoc/>
        public async Task<Buyer?> DeleteBuyerAsync(Guid buyerId)
        {
            _logger.LogInformation("Этап удаления покупателя с ID: {BuyerId} в репозитории", buyerId);
            try
            {
                Buyer? buyer = await _carsTradeDbContext.Buyers.FirstOrDefaultAsync(x => x.BuyerId == buyerId);
                if (buyer == null)
                {
                    _logger.LogWarning("Попытка удалить несуществующего пользователя с ID: {BuyerId}", buyerId);
                    throw new Exception($"Пользователь c ID: {buyerId} не найден");
                }

                _carsTradeDbContext.Buyers.Remove(buyer);
                await _carsTradeDbContext.SaveChangesAsync();
                _logger.LogInformation("Покупатель с ID: {BuyerId} успешно удален", buyerId);
                return buyer;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при удалении покупателя с ID: {BuyerId}", buyerId);
                throw new Exception("Ошибка при удалении покупателя", ex);
            }
        }


        /// <inheritdoc/>
        public async Task<Buyer?> GetBuyerByIdAsync(Guid buyerId)
        {
            _logger.LogInformation("Этап получения покупателя с ID: {BuyerId} в репозитории", buyerId);
            try
            {
                Buyer? buyer = await _carsTradeDbContext.Buyers.FirstOrDefaultAsync(x => x.BuyerId == buyerId);
                if (buyer == null)
                {
                    _logger.LogWarning("Пользователь с ID: {BuyerId} не найден", buyerId);
                    return null;
                }
                _logger.LogInformation("Покупатель с ID: {BuyerId} успешно получен", buyerId);
                return buyer;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении покупателя с ID: {BuyerId}", buyerId);
                throw new Exception("Ошибка при получении покупателя", ex);
            }
        }


        /// <inheritdoc/>
        public async Task<Buyer> UpdateBuyerAsync(Buyer buyer)
        {
            _logger.LogInformation("Этап обновления покупателя с ID: {BuyerId} в репозитории", buyer.BuyerId);
            try
            {
                _carsTradeDbContext.Buyers.Update(buyer);
                await _carsTradeDbContext.SaveChangesAsync();
                _logger.LogInformation("Покупатель с ID: {BuyerId} успешно обновлен", buyer.BuyerId);
                return buyer;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обновлении покупателя с ID: {BuyerId}", buyer.BuyerId);
                throw new Exception("Ошибка при обновлении покупателя", ex);
            }
        }


        /// <inheritdoc/>
        public async Task<List<Buyer>> GetAllBuyersAsync()
        {
            _logger.LogInformation("Этап получения всех покупателей в репозитории");
            try
            {
                List<Buyer> buyers = await _carsTradeDbContext.Buyers.ToListAsync();
                _logger.LogInformation("Все покупатели успешно получены. Количество: {Count}", buyers.Count);
                return buyers;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении всех покупателей");
                throw new Exception("Ошибка при получении всех покупателей", ex);
            }
        }
    }
}

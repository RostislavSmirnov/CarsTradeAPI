using CarsTradeAPI.Data;
using CarsTradeAPI.Entities;
using Microsoft.EntityFrameworkCore;


namespace CarsTradeAPI.Features.CarInventoryOperation.CarInventoryRepository
{
    /// <summary>
    /// Реализация репозитория для работы с объектами CarInventory
    /// Использует EF Core для доступа к базе данных
    /// </summary>
    public class ImplementationCarInventoryRepository : ICarInventoryRepository
    {
        private readonly CarsTradeDbContext _context;
        private readonly ILogger<ImplementationCarInventoryRepository> _logger;
        public ImplementationCarInventoryRepository(CarsTradeDbContext context, ILogger<ImplementationCarInventoryRepository> logger)
        {
            _context = context;
            _logger = logger;
        }


        /// <inheritdoc/>
        public async Task<CarInventory> CreateCarInventoryAsync(CarInventory inventory)
        {
            _logger.LogInformation("Этап создания CarInventory с ID: {InventoryId} в репозитории", inventory.InventoryId);
            try
            {
                CarInventory? carInventory = await _context.CarInventories.FirstOrDefaultAsync(c => c.InventoryId == inventory.InventoryId);
                if (carInventory != null)
                {
                    _logger.LogWarning("Попытка создать объект с уже существующим ID: {InventoryId}", inventory.InventoryId);
                    throw new Exception("Объект с таким ID уже найден");
                }

                await _context.CarInventories.AddAsync(inventory);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Объект CarInventory успешно создан с ID: {InventoryId}", inventory.InventoryId);
                return inventory;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при создании CarInventory с ID: {InventoryId}", inventory.InventoryId);
                throw new Exception("Ошибка при создании CarInventory", ex);
            }
        }


        /// <inheritdoc/>
        public async Task<CarInventory?> DeleteCarInventoryAsync(Guid carInventoryId)
        {
            _logger.LogInformation("Этап удаления CarInventory с ID: {InventoryId} в репозитории", carInventoryId);
            try
            {
                CarInventory? carInventory = await _context.CarInventories.FirstOrDefaultAsync(c => c.InventoryId == carInventoryId);
                if (carInventory == null)
                {
                    _logger.LogWarning("Попытка удалить несуществующий объект с ID: {InventoryId}", carInventoryId);
                    return null;
                }

                _context.CarInventories.Remove(carInventory);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Объект CarInventory с ID: {InventoryId} успешно удален", carInventoryId);
                return carInventory;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при удалении CarInventory с ID: {InventoryId}", carInventoryId);
                throw new Exception("Ошибка при попытке удалить предмет", ex);
            }
        }


        /// <inheritdoc/>
        public async Task<List<CarInventory>> GetAllCarInventoriesAsync()
        {
            _logger.LogInformation("Этап получения всех объектов CarInventory в репозитории");
            try
            {
                List<CarInventory> carInventories = await _context.CarInventories.ToListAsync();
                _logger.LogInformation("Все объекты CarInventory успешно получены. Количество: {Count}", carInventories.Count);
                return carInventories;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении всех объектов CarInventory");
                throw new Exception("Ошибка при получении всех объектов CarInventory", ex);
            }
        }


        /// <inheritdoc/>
        public async Task<CarInventory?> GetCarInventoryByIdAsync(Guid carInventoryId)
        {
            _logger.LogInformation("Этап получения CarInventory с ID: {InventoryId} в репозитории", carInventoryId);
            try
            {
                CarInventory? carInventory = await _context.CarInventories.FirstOrDefaultAsync(c => c.InventoryId == carInventoryId);
                if (carInventory == null)
                {
                    _logger.LogWarning("Попытка получить несуществующий объект с ID: {InventoryId}", carInventoryId);
                    return null;
                }
                _logger.LogInformation("Объект CarInventory с ID: {InventoryId} успешно получен", carInventoryId);
                return carInventory;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении CarInventory с ID: {InventoryId}", carInventoryId);
                throw new Exception("Ошибка при получении CarInventory", ex);
            }
        }


        /// <inheritdoc/>
        public async Task<CarInventory> EditCarInventoryAsync(CarInventory carInventory)
        {
            _logger.LogInformation("Этап обновления CarInventory с ID: {InventoryId} в репозитории", carInventory.InventoryId);
            try
            {
                CarInventory? existingCarInventory = await _context.CarInventories.FirstOrDefaultAsync(c => c.InventoryId == carInventory.InventoryId);
                if (existingCarInventory == null)
                {
                    _logger.LogWarning("Попытка обновить несуществующий объект с ID: {InventoryId}", carInventory.InventoryId);
                    throw new Exception("Объект с таким ID не найден");
                }

                existingCarInventory.LastUpdated = DateTime.UtcNow;
                _context.Entry(existingCarInventory).CurrentValues.SetValues(carInventory);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Объект CarInventory с ID: {InventoryId} успешно обновлен", carInventory.InventoryId);
                return existingCarInventory;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обновлении CarInventory с ID: {InventoryId}", carInventory.InventoryId);
                throw new Exception("Ошибка при попытке обновить предмет", ex);
            }
        }


        /// <inheritdoc/>
        public async Task<CarModel?> GetCarModelByIdAsync(Guid CarModelId)
        {
            _logger.LogInformation("Этап получения CarModel с ID: {CarModelId} в репозитории", CarModelId);
            try
            {
                CarModel? carModel = await _context.CarModels.FirstOrDefaultAsync(id => id.CarModelId == CarModelId);
                if (carModel == null)
                {
                    _logger.LogWarning("Попытка получить несуществующую модель машины с ID: {CarModelId}", CarModelId);
                    return null;
                }
                _logger.LogInformation("Модель машины с ID: {CarModelId} успешно получена", CarModelId);
                return carModel;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при попытке получить CarModel c ID {CarModelId}", CarModelId);
                throw new Exception($"Ошибка при попытке получить CarModel c ID {CarModelId}", ex);
            }
        }
    }
}

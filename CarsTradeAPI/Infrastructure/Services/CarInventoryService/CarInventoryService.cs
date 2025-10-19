using CarsTradeAPI.Data;
using CarsTradeAPI.Entities;
using Microsoft.EntityFrameworkCore;


namespace CarsTradeAPI.Infrastructure.Services.CarInventoryService
{
    /// <summary>
    /// Реализация сервиса для управления инвентарём автомобилей
    /// </summary>
    public class CarInventoryService : ICarInventoryService
    {
        private readonly CarsTradeDbContext _dbContext;
        public CarInventoryService(CarsTradeDbContext dbContext)
        {
            _dbContext = dbContext;
        }


        /// <inheritdoc/>
        public async Task<CarInventoryServiceResult> CheckAvailabilityAsync(Guid carModelId, uint quantity)
        {
            CarInventoryServiceResult result;

            CarInventory? inventory = await _dbContext.CarInventories
                .FirstOrDefaultAsync(ci => ci.CarModelId == carModelId);
            
            if (inventory == null) 
            {
                result = new CarInventoryServiceResult
                {
                    IsAvailable = false,
                    AvailableQuantity = 0,
                };
                return result;
            }

            result = new CarInventoryServiceResult
            {
                IsAvailable = true,
                AvailableQuantity = inventory.Quantity
            };
            return result;
        }


        /// <inheritdoc/>
        public async Task DecreaseInventoryAsync(Guid carModelId, uint quantity)
        {
            CarInventory? inventory = await _dbContext.CarInventories
                .FirstOrDefaultAsync(ci => ci.CarModelId == carModelId);

            if (inventory == null)
            {
                throw new Exception($"Не удалось найти в наличии модель автомобиля с ID {carModelId}");
            }

            if (inventory.Quantity < quantity)
            {
                throw new Exception($"В наличии количество автомобилей {inventory.Quantity} меньше чем содержит запрос {quantity}");
            }

            try
            {
                inventory.Quantity -= quantity;
                inventory.LastUpdated = DateTime.UtcNow;
                _dbContext.CarInventories.Update(inventory);
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Ошибка при уменьшении количества автомобилей в наличии", ex);
            }
        }


        /// <inheritdoc/>
        public async Task IncreaseInventoryAsync(Guid carModelId, uint quantity)
        {
            CarInventory? inventory = await _dbContext.CarInventories.FirstOrDefaultAsync(ci => ci.CarModelId == carModelId);
            
            if (inventory == null)
            {
                throw new Exception($"Не удалось найти в наличии модель автомобиля с ID {carModelId}");
            }

            try
            {
                inventory.Quantity += quantity;
                inventory.LastUpdated = DateTime.UtcNow;
                _dbContext.CarInventories.Update(inventory);
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Ошибка при увеличении количества автомобилей в наличии", ex);
            }
        }
    }
}

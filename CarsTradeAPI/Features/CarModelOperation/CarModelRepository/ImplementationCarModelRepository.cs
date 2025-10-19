using CarsTradeAPI.Data;
using CarsTradeAPI.Entities;
using CarsTradeAPI.Features.CarModelOperation.SearchCarModel;
using Microsoft.EntityFrameworkCore;


namespace CarsTradeAPI.Features.CarModelOperation.CarModelRepository
{
    /// <summary>
    /// Реализация репозитория для работы с моделями автомобилей
    /// Содержит логику взаимодействия с БД через EF Core
    /// </summary>
    public class ImplementationCarModelRepository : ICarModelRepository
    {
        private readonly CarsTradeDbContext _carsTradeDbContext;
        private readonly ILogger<ImplementationCarModelRepository> _logger;

        public ImplementationCarModelRepository(CarsTradeDbContext carsTradeDbContext, ILogger<ImplementationCarModelRepository> logger)
        {
            _carsTradeDbContext = carsTradeDbContext;
            _logger = logger;
        }


        /// <inheritdoc/>
        public async Task<CarModel> CreateCarModelAsync(CarModel carModel)
        {
            _logger.LogInformation("Этап создания CarModel в репозитории");
            try
            {
                await _carsTradeDbContext.AddAsync(carModel);
                await _carsTradeDbContext.SaveChangesAsync();
                _logger.LogInformation("Модель машины успешно создана с ID: {CarModelId}", carModel.CarModelId);
                return carModel;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при попытке создать модель машины в базе данных");
                throw new Exception("Ошибка при создании модели автомобиля", ex);
            }
        }


        /// <inheritdoc/>
        public async Task<CarModel?> DeleteCarModelAsync(Guid CarModelId)
        {
            _logger.LogInformation("Этап удаления CarModel с ID: {CarModelId} в репозитории", CarModelId);
            try
            {
                CarModel? carModel = await _carsTradeDbContext.CarModels.FirstOrDefaultAsync(id => id.CarModelId == CarModelId);
                if (carModel == null)
                {
                    _logger.LogWarning("Попытка удалить несуществующую модель машины с ID: {CarModelId}", CarModelId);
                    return null;
                }

                _carsTradeDbContext.Remove(carModel);
                await _carsTradeDbContext.SaveChangesAsync();
                _logger.LogInformation("Модель машины с ID: {CarModelId} успешно удалена", CarModelId);
                return carModel;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при удалении CarModel с ID {CarModelId}", CarModelId);
                throw new Exception("Ошибка при удалении модели автомобиля", ex);
            }
        }


        /// <inheritdoc/>
        public async Task<CarModel> EditCarModelAsync(CarModel carModel)
        {
            _logger.LogInformation("Этап обновления CarModel с ID: {CarModelId} в репозитории", carModel.CarModelId);
            try
            {
                _carsTradeDbContext.CarModels.Update(carModel);
                await _carsTradeDbContext.SaveChangesAsync();
                _logger.LogInformation("Модель машины с ID: {CarModelId} успешно обновлена", carModel.CarModelId);
                return carModel;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обновлении CarModel с ID {CarModelId}", carModel.CarModelId);
                throw new Exception("Ошибка при обновлении модели автомобиля", ex);
            }
        }


        /// <inheritdoc/>
        public async Task<CarModel?> GetCarModelByIdAsync(Guid CarModelId)
        {
            _logger.LogInformation("Этап получения CarModel с ID: {CarModelId} в репозитории", CarModelId);
            try
            {
                CarModel? carModel = await _carsTradeDbContext.CarModels.FirstOrDefaultAsync(id => id.CarModelId == CarModelId);
                if (carModel == null)
                {
                    _logger.LogWarning("Модель с ID: {CarModelId} не найдена", CarModelId);
                    return null;
                }
                _logger.LogInformation("Модель машины с ID: {CarModelId} успешно получена", CarModelId);
                return carModel;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении CarModel с ID {CarModelId}", CarModelId);
                throw new Exception($"Ошибка при получении модели автомобиля с ID {CarModelId}", ex);
            }
        }


        /// <inheritdoc/>
        public async Task<List<CarModel>> GetCarModelsAsync()
        {
            _logger.LogInformation("Этап получения всех CarModels в репозитории");
            try
            {
                List<CarModel>? carModels = await _carsTradeDbContext.CarModels.ToListAsync();
                _logger.LogInformation("Все модели автомобилей успешно получены, количество: {Count}", carModels.Count);
                return carModels;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении всех моделей автомобилей");
                throw new Exception("Ошибка при получении списка моделей автомобилей", ex);
            }
        }


        /// <inheritdoc/>
        public async Task<List<CarModel>> SearchCarModels(SearchCarModelQuery query)
        {
            _logger.LogInformation("Этап поиска CarModel по параметрам запроса в репозитории");
            try
            {
                IQueryable<CarModel> searchQuery = _carsTradeDbContext.CarModels;

                // Добавление в запрос информации о производителе
                if (!string.IsNullOrWhiteSpace(query.CarManufacturer))
                {
                    searchQuery = searchQuery.Where(a => a.CarManufacturer != null && EF.Functions.ILike(a.CarManufacturer, $"%{query.CarManufacturer}%"));
                }

                // Добавление в запрос информации о модели машины
                if (!string.IsNullOrWhiteSpace(query.CarModelName))
                {
                    searchQuery = searchQuery.Where(a => a.CarModelName != null && EF.Functions.ILike(a.CarModelName, $"%{query.CarModelName}%"));
                }

                // Добавление в запрос информации о конфигурации машины
                if (query.CarConfiguration != null)
                {
                    searchQuery = searchQuery.Where(a =>
                        a.CarConfiguration != null
                        && EF.Functions.ILike(a.CarConfiguration.CarInterior, $"%{query.CarConfiguration.CarInterior}%")
                        && EF.Functions.ILike(a.CarConfiguration.WheelSize.ToString(), $"%{query.CarConfiguration.WheelSize}%")
                        && EF.Functions.ILike(a.CarConfiguration.CarMusicBrand!, $"%{query.CarConfiguration.CarMusicBrand}%"));
                }

                // Добавление в запрос информации о стране - производителе
                if (!string.IsNullOrWhiteSpace(query.CarCountry))
                {
                    searchQuery = searchQuery.Where(a => a.CarCountry != null && EF.Functions.ILike(a.CarCountry, $"%{query.CarCountry}%"));
                }

                // Добавление в запрос информации о дате производства машины
                if (query.ProductionDateTime != null)
                {
                    searchQuery = searchQuery.Where(x => x.ProductionDateTime.Year == query.ProductionDateTime.Value.Year);
                }

                // Добавление в запрос информации о двигателе
                if (query.CarEngine != null)
                {
                    searchQuery = searchQuery.Where(a => a.CarEngine != null
                        && a.CarEngine.FuelType == query.CarEngine.FuelType
                        && a.CarEngine.EngineCapacity == query.CarEngine.EngineCapacity
                        && a.CarEngine.EngineHorsePower == query.CarEngine.EngineHorsePower);
                }

                // Добавление в запрос информации о цвете
                if (!string.IsNullOrWhiteSpace(query.CarColor))
                {
                    searchQuery = searchQuery.Where(a => a.CarColor != null && EF.Functions.ILike(a.CarColor, $"%{query.CarColor}%"));
                }

                // Добавление в запрос информации о цене машины
                if (query.CarPrice != null)
                {
                    searchQuery = searchQuery.Where(a => a.CarPrice == query.CarPrice);
                }

                List<CarModel> result = await searchQuery.ToListAsync();
                _logger.LogInformation("Поиск завершён. Найдено моделей: {Count}", result.Count);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при поиске моделей автомобилей");
                throw new Exception("Ошибка при выполнении поиска моделей", ex);
            }
        }
    }
}


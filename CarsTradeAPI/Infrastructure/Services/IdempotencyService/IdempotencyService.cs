using CarsTradeAPI.Data;
using CarsTradeAPI.Entities;
using Microsoft.EntityFrameworkCore;


namespace CarsTradeAPI.Infrastructure.Services.IdempotencyService
{
    /// <summary>
    /// Сервис для обработки идемпотентных запросов
    /// </summary>
    public class IdempotencyService : IIdempotencyService
    {
        private readonly ILogger<IdempotencyService> _logger;
        private readonly CarsTradeDbContext _context;

        public IdempotencyService(ILogger<IdempotencyService> logger, CarsTradeDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        /// <inheritdoc/>
        public async Task<(Guid? resourceId, string? responseJson)> GetByKeyAsync(string key)
        {
            _logger.LogInformation("Checking IdempotencyRequest for key: {Key}", key);
            IdempotencyRequest? record = await _context.IdempotencyRequests.AsNoTracking()
                .FirstOrDefaultAsync(r => r.Key == key);
            _logger.LogInformation("IdempotencyRequest found: {Found}", record != null);
            return (record?.ResourceId, record?.Responsejson);
        }

        /// <inheritdoc/>
        public async Task SaveAsync(string key, string resourceType, Guid resourceId, string? responseJson = null)
        {
            if (await _context.IdempotencyRequests.AnyAsync(r => r.Key == key))
            {
                _logger.LogInformation("Idempotency key {Key} already exists, skipping save.", key);
                return;
            }

            IdempotencyRequest record = new IdempotencyRequest
            {
                Key = key,
                ResourceType = resourceType,
                ResourceId = resourceId,
                Responsejson = responseJson,
                CreatedAt = DateTime.UtcNow,
                Status = "Completed"
            };

            _context.IdempotencyRequests.Add(record);
            await _context.SaveChangesAsync();
            _logger.LogInformation("IdempotencyRequest saved for key: {Key}", key);
        }
    }
}
using AutoMapper;
using CarsTradeAPI.Entities;
using CarsTradeAPI.Features.CarInventoryOperation.CarInventoryDto;
using CarsTradeAPI.Features.CarInventoryOperation.CreateCarInventory;
using CarsTradeAPI.Features.CarInventoryOperation.EditCarInventory;

namespace CarsTradeAPI.Features.CarInventoryOperation
{
    /// <summary>
    /// Профиль маппинга для операций с инвентаризацией автомобилей
    /// </summary>
    public class CarInventoryMappingProfile : Profile
    {
        public CarInventoryMappingProfile()
        {
            CreateMap<CreateCarInventoryCommand, CarInventory>();

            CreateMap<EditCarInventoryCommand, CarInventory>()
                // Игнорировать Quantity, если он null в команде
                .ForMember(dest => dest.Quantity, opt => opt.Condition(src => src.Quantity.HasValue));

            CreateMap<CarInventory, ShowCarInventoryDto>();
        }
    }
}

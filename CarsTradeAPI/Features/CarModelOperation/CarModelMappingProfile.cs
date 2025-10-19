using AutoMapper;
using CarsTradeAPI.Entities;
using CarsTradeAPI.Features.CarModelOperation.CreateCarModel;
using CarsTradeAPI.Features.CarModelOperation.CarModelDto;
using CarsTradeAPI.Features.CarModelOperation.EditCarModel;


namespace CarsTradeAPI.Features.CarModelOperation
{
    /// <summary>
    /// Профиль маппинга для операций с моделью автомобиля
    /// </summary>
    public class CarModelMappingProfile : Profile
    {
        public CarModelMappingProfile()
        {
            CreateMap<CreateCarModelCommand, CarModel>();
            CreateMap<CarModel, CreateCarModelCommand>();

            CreateMap<CarModel, ShowCarModelDto>();
            CreateMap<ShowCarModelDto, CarModel>();

            CreateMap<CarModel, EditCarModelCommand>();
            CreateMap<EditCarModelCommand, CarModel>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null && !string.IsNullOrWhiteSpace(srcMember.ToString())));
        }
    }
}

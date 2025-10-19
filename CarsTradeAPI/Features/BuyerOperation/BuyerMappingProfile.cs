using AutoMapper;
using CarsTradeAPI.Entities;
using CarsTradeAPI.Features.BuyerOperation.BuyerDto;
using CarsTradeAPI.Features.BuyerOperation.CreateBuyer;
using CarsTradeAPI.Features.BuyerOperation.EditBuyer;


namespace CarsTradeAPI.Features.BuyerOperation
{
    /// <summary>
    /// Класс описывает профиль маппинга для покупателя
    /// </summary>
    public class BuyerMappingProfile : Profile
    {
        public BuyerMappingProfile()
        {
            CreateMap<CreateBuyerCommand, Buyer>();
            
            CreateMap<Buyer, ShowBuyerDto>();

            CreateMap<EditBuyerCommand, Buyer>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null && !string.IsNullOrWhiteSpace(srcMember.ToString())));
            
            CreateMap<Buyer, ShowBuyerDto>();
        }
    }
}

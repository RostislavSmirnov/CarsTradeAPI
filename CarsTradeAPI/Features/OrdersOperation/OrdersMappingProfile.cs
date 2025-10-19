using AutoMapper;
using CarsTradeAPI.Features.OrdersOperation.CreateOrder;
using CarsTradeAPI.Entities;
using CarsTradeAPI.Features.OrdersOperation.OrdersDto;
using CarsTradeAPI.Features.OrdersOperation.EditOrder;
using CarsTradeAPI.Features.OrdersOperation.OrderItems.AddOrderItem;
using CarsTradeAPI.Features.OrdersOperation.OrderItems.EditOrderItem;


namespace CarsTradeAPI.Features.OrdersOperation
{
    /// <summary>
    /// Профиль маппинга для заказов и связанных сущностей
    /// </summary>
    public class OrdersMappingProfile : Profile
    {
        public OrdersMappingProfile()
        {
            CreateMap<CreateOrderCommand, Order>();

            CreateMap<Order, ShowOrderDto>();

            CreateMap<EditOrderCommand, Order>()
                .ForMember(dest => dest.OrderId, opt => opt.Ignore()) 
                .ForMember(dest => dest.BuyerId, opt => opt.Condition(src => src.BuyerId.HasValue)) 
                .ForMember(dest => dest.EmployeeId, opt => opt.Condition(src => src.EmployeeId.HasValue)) 
                .ForMember(dest => dest.OrderCompletionDate, opt => opt.Condition(src => src.OrderCompletionDate.HasValue)) 
                .ForMember(dest => dest.OrderAddress, opt => opt.Condition(src => src.OrderAddress != null)); 

            CreateMap<OrderItem, OrderItemDto>();
            CreateMap<OrderItemDto, OrderItem>();
            
            CreateMap<OrderItem, ShowOrderItemDto>();

            CreateMap<CreateOrderItemCommand, OrderItem>()
                .ForMember(dest => dest.OrderItemId, opt => opt.Ignore())
                .ForMember(dest => dest.OrderId, opt => opt.Ignore())
                .ForMember(dest => dest.UnitPrice, opt => opt.Ignore());

            CreateMap<EditOrderItemCommand, OrderItem>().
                ForMember(dest => dest.OrderItemId, opt => opt.Ignore())
                .ForMember(dest => dest.OrderId, opt => opt.Ignore())
                .ForMember(dest => dest.UnitPrice, opt => opt.Ignore())
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null && !string.IsNullOrWhiteSpace(srcMember.ToString())));
        }
    }
}

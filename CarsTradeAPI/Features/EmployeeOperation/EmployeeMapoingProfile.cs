using AutoMapper;
using CarsTradeAPI.Features.EmployeeOperation.CreateEmployee;
using CarsTradeAPI.Entities;
using CarsTradeAPI.Features.EmployeeOperation.EditEmployee;
using CarsTradeAPI.Features.EmployeeOperation.EmployeeDto;
using CarsTradeAPI.Features.EmployeeOperation.CreateAdminForTest;


namespace CarsTradeAPI.Features.EmployeeOperation
{
    /// <summary>
    /// Профиль маппинга для сущности Сотрудника
    /// </summary>
    public class EmployeeMapoingProfile : Profile
    {
        public EmployeeMapoingProfile()
        {
            CreateMap<CreateEmployeeCommand, Employee>();
            CreateMap<CreateAdminForTestCommand, Employee>();

            CreateMap<EditEmployeeCommand, Employee>().
                ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null && !string.IsNullOrWhiteSpace(srcMember.ToString())));

            CreateMap<Employee, ShowEmployeeDto>()
                .ForMember(dest => dest.EmployeeId, opt => opt.MapFrom(src => src.EmployeeId))
                .ForMember(dest => dest.EmployeeName, opt => opt.MapFrom(src => src.EmployeeName))
                .ForMember(dest => dest.EmployeeSurname, opt => opt.MapFrom(src => src.EmployeeSurname))
                .ForMember(dest => dest.EmployeeMiddlename, opt => opt.MapFrom(src => src.EmployeeMiddlename))
                .ForMember(dest => dest.EmployeeAge, opt => opt.MapFrom(src => src.EmployeeAge))
                .ForMember(dest => dest.EmployeeSellCounter, opt => opt.MapFrom(src => src.EmployeeSellCounter))
                .ForMember(dest => dest.EmployeeRole, opt => opt.MapFrom(src => src.EmployeeRole));
        }
    }
}

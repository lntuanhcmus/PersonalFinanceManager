using AutoMapper;
using PersonalFinanceManager.Shared.Data;
using PersonalFinanceManager.Shared.Dto;
using System.Globalization;

namespace PersonalFinanceManager.Shared.MappingProfiles
{
    public class BudgetMappingProfile: Profile
    {
        public BudgetMappingProfile()
        {
            CreateMap<Budget, BudgetDto>()
            .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.StartDate.ToString("dd/MM/yyyy")))
            .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.EndDate.HasValue ? src.EndDate.Value.ToString("dd/MM/yyyy") : null))
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : null));

            CreateMap<BudgetDto, Budget>()
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => DateTime.ParseExact(src.StartDate, "dd/MM/yyyy", null)))
                .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src =>
                    !string.IsNullOrEmpty(src.EndDate) ? DateTime.ParseExact(src.EndDate, "dd/MM/yyyy", null) : (DateTime?)null))
                .ForMember(dest => dest.Category, opt => opt.Ignore()); // vì bạn có CategoryId, không map Category navigation
        }
    }
}

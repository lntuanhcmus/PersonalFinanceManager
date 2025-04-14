using PersonalFinanceManager.Shared.Dto;
using PersonalFinanceManager.WebHost.Models;
using AutoMapper;

namespace PersonalFinanceManager.WebHost.MappingProfiles
{
    public class TransactionViewMappingProfile: Profile
    {
        public TransactionViewMappingProfile() 
        {
            // DTO -> ViewModel
            CreateMap<TransactionDto, DetailTransactionViewModel>();

            // ViewModel -> DTO (nếu cần dùng ngược lại)
            CreateMap<DetailTransactionViewModel, TransactionDto>();
        }
    }
}

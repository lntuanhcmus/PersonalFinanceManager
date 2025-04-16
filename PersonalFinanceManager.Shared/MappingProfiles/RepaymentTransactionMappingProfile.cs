using AutoMapper;
using PersonalFinanceManager.Shared.Data;
using PersonalFinanceManager.Shared.Dto;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonalFinanceManager.Shared.MappingProfiles
{
    public class RepaymentTransactionMappingProfile: Profile
    {
        public RepaymentTransactionMappingProfile()
        {
            // RepaymentTransaction -> RepaymentTransactionDto
            CreateMap<RepaymentTransaction, RepaymentTransactionDto>()
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src =>
                    src.CreatedAt.ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture)))
                .ForMember(dest => dest.TransactionTime, opt => opt.MapFrom(src =>
                    src.TransactionTime.ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture)));

            // RepaymentTransactionDto -> RepaymentTransaction
            CreateMap<RepaymentTransactionDto, RepaymentTransaction>()
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src =>
                    !string.IsNullOrEmpty(src.CreatedAt)
                        ? DateTime.ParseExact(src.CreatedAt, "dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture)
                        : DateTime.Now))
                .ForMember(dest => dest.TransactionTime, opt => opt.MapFrom(src =>
                    DateTime.ParseExact(src.TransactionTime, "dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture)));

        }
    }
}

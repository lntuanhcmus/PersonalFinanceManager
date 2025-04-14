using AutoMapper;
using PersonalFinanceManager.Shared.Data.Entity;
using PersonalFinanceManager.Shared.Dto;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonalFinanceManager.Shared.MappingProfiles
{
    public class TransactionMappingProfile: Profile
    {
        public TransactionMappingProfile()
        {
            // Transaction => TransactionDto
            CreateMap<Transaction, TransactionDto>()
                .ForMember(dest => dest.TransactionTime,
                    opt => opt.MapFrom(src =>
                        src.TransactionTime.ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture)))
                .ForMember(dest => dest.TransactionTypeName,
                    opt => opt.MapFrom(src => src.TransactionType != null ? src.TransactionType.Name : null))
                .ForMember(dest => dest.CategoryName,
                    opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : null));

            // TransactionDto => Transaction
            CreateMap<TransactionDto, Transaction>()
                .ForMember(dest => dest.TransactionTime,
                    opt => opt.MapFrom(src =>
                        DateTime.ParseExact(src.TransactionTime, "dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture)))
                .ForMember(dest => dest.TransactionType, opt => opt.Ignore())
                .ForMember(dest => dest.Category, opt => opt.Ignore());
        }
    
    }
}

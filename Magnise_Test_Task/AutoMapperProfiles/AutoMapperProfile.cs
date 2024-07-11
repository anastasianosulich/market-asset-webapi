using AutoMapper;
using Magnise_Test_Task.Data.Entities;
using Magnise_Test_Task.Models.Dto;
using Magnise_Test_Task.Models.FinTechApi.Responses;

namespace Magnise_Test_Task
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<KeyValuePair<string, AssetProviderMappingResponse>, AssetProviderMapping>()
                .ForMember(dest => dest.Provider, opt => opt.MapFrom(src => src.Key))
                .ForMember(dest => dest.Symbol, opt => opt.MapFrom(src => src.Value.Symbol))
                .ForMember(dest => dest.Exchange, opt => opt.MapFrom(src => src.Value.Exchange))
                .ForMember(dest => dest.DefaultOrderSize, opt => opt.MapFrom(src => src.Value.DefaultOrderSize))
                .ForMember(dest => dest.AssetId, opt => opt.Ignore())
                .ForMember(dest => dest.Asset, opt => opt.Ignore())
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Price, opt => opt.Ignore());

            CreateMap<AssetProviderMappingResponse, AssetProviderMapping>()
                .ForMember(dest => dest.Provider, opt => opt.MapFrom((src, dest, destMember, context) => context.Items["Provider"]))
                .ForMember(dest => dest.AssetId, opt => opt.Ignore())
                .ForMember(dest => dest.Asset, opt => opt.Ignore())
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Price, opt => opt.Ignore());

            CreateMap<Models.FinTechApi.Responses.Asset, Data.Entities.Asset>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Guid.Parse(src.Id)))
                .ForMember(dest => dest.Mappings, opt => opt.MapFrom(src => src.Mappings));

            CreateMap<Data.Entities.Asset, AssetDTO>()
               .ForMember(dest => dest.Exchange, opt => opt.MapFrom(src => src.Exchange ?? string.Empty))
               .ForMember(dest => dest.BaseCurrency, opt => opt.MapFrom(src => src.BaseCurrency ?? string.Empty));

            CreateMap<AssetProviderMapping, AssetProviderMappingDto>();
        }
    }
}

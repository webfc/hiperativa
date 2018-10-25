using MongoDB.Bson;
using WebFc.Hiperativa.Data.Entities;
using WebFc.Hiperativa.Domain.ViewModels;
using WebFc.Hiperativa.Domain.ViewModels.Admin;
using AutoMapperProfile = AutoMapper.Profile;

namespace WebFc.Hiperativa.Domain.AutoMapper
{
    public class ViewModelToDomainMappingProfile : AutoMapperProfile
    {
        public ViewModelToDomainMappingProfile()
        {
            /*EXEMPLE*/
            //CreateMap<ViewModel, Entity>()
            //    .ForMember(dest => dest.PromotionalCodeId, opt => opt.MapFrom(src => ObjectId.Parse(src.Id)));
            CreateMap<UserAdministratorViewModel, UserAdministrator>()
                .ForMember(dest => dest._id, opt => opt.MapFrom(src => ObjectId.Parse(src.Id)));
            CreateMap<ProfileRegisterViewModel, Profile>()
                .ForMember(dest => dest._id, opt => opt.MapFrom(src => ObjectId.Parse(src.Id)));

        }
    }
}
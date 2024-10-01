using AutoMapper;
using StudySpace.Data.Models;
using StudySpace.Service.BusinessModel;

namespace StudySpace.API.Helper
{
    public class MappingProfiles : Profile
    {
        public MappingProfiles()
        {
            CreateMap<Account, GetDetailUserModel>();

            CreateMap<Room,RoomModel>();
            CreateMap<Account, UserModel>()
            .ForMember(dest => dest.RoleName, opt => opt.MapFrom(src => src.Role.RoleName));

        }
    }
}

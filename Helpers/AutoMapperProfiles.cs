using System.Linq;
using AutoMapper;
using DatingApp.Api.DTOs;
using DatingApp.Api.Entities;
using DatingApp.Api.Extensions;

namespace DatingApp.Api.Helpers
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            CreateMap<AppUser, MemberDto>()
                    .ForMember(destination => destination.PhotoUrl, 
                               option => 
                               option.MapFrom(src => 
                               src.Photos.FirstOrDefault(c => c.IsMain).Url))
                    .ForMember(destinationMember => destinationMember.Age,
                               memberOptions =>
                               memberOptions.MapFrom(sourceMember =>
                               sourceMember.DateOfBirth.calculateAge()));
            CreateMap<Photo, PhotoDto>();

            CreateMap<MemberUpdateDto,AppUser>();
        }
    }
}
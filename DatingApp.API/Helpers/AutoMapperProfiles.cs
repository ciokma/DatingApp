using System.Linq;
using AutoMapper;
using DatingApp.API.Data;
using DatingApp.API.Dtos;

namespace DatingApp.API.Helpers
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            //1
            CreateMap<User, UserForListDto>()
            .ForMember(dest => dest.PhotoUrl, opt => {
                opt.MapFrom(src=>src.Photos.FirstOrDefault(p=>p.isMain).Url);
            })
            .ForMember(d => d.Age, map => map.MapFrom((s,d) => s.DateOfBirth.CalculateAge()));
            /*
           .ForMember(dest => dest.PhotoUrl, opt=>{
                opt.ResolveUsing(d=>d.DateOfBirth.CalculateAge());
            });
            */
            //2
            CreateMap<User, UserForDetailedDto>()
            .ForMember(dest=>dest.photoUrl, opt=>{
                opt.MapFrom(src=>src.Photos.FirstOrDefault(p=>p.isMain).Url);
            })
            .ForMember(d => d.age, map => map.MapFrom((s,d) => s.DateOfBirth.CalculateAge()));

            /*.ForMember(dest => dest.PhotoUrl, opt=>{
                opt.ResolveUsing(d=>d.DateOfBirth.CalculateAge());
            });
            */
            //3
            CreateMap<Photo, PhotosForDetailedDto>();
            //Mapear la clase de usuario que se encargara de actualizar los campos
            CreateMap<UserForUpdateDto, User>();
        }
    }
}
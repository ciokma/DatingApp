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
            .ForMember(dest => dest.photoUrl, opt => {
                opt.MapFrom(src=>src.Photos.FirstOrDefault(p=>p.isMain).Url);
            })
            .ForMember(d => d.age, map => map.MapFrom((s,d) => s.DateOfBirth.CalculateAge()));
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
            CreateMap<Photo, PhotoForReturnDto>();
            CreateMap<PhotoForCreationDto, Photo>();
            //automapper para el registro
            CreateMap<UserForRegisterDto, User>();
            //reverse map se utiliza para no regresar toda la data del la tabla relacion
            CreateMap<MessageForCreationDto, Message>().ReverseMap();
            // AutoMapper para coincidencia
             CreateMap<Message, MessageToReturnDto>()
                .ForMember(m => m.SenderPhotoUrl, opt => opt
                    .MapFrom(u => u.Sender.Photos.FirstOrDefault(p => p.isMain).Url))
                .ForMember(m => m.RecipientPhotoUrl, opt => opt
                    .MapFrom(u => u.Recipient.Photos.FirstOrDefault(p => p.isMain).Url));
        }
    }
}
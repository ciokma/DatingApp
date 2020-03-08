using DatingApp.API.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using Microsoft.Extensions.Options;
using DatingApp.API.Helpers;
using CloudinaryDotNet;
using System.Threading.Tasks;
using DatingApp.API.Dtos;
using System.Security.Claims;
using CloudinaryDotNet.Actions;
using System.Linq;

namespace DatingApp.API.Controllers
{
    [Authorize]
    [Route("api/users/{userId}/photos")]
    [ApiController]
    public class PhotosController : ControllerBase
    {
        private readonly IDatingRepository _repo;
        private readonly IMapper _mapper;
        private readonly IOptions<CloudinarySettings> _cloudinaryConfig;
        private Cloudinary _cloudinary;

        public PhotosController(IDatingRepository repo, IMapper mapper, IOptions<CloudinarySettings> cloudinaryConfig)
        {
            this._repo = repo;
            this._mapper = mapper;
            this._cloudinaryConfig = cloudinaryConfig;
            Account acc = new Account(
                _cloudinaryConfig.Value.CloudName,
                _cloudinaryConfig.Value.ApiKey,
                _cloudinaryConfig.Value.ApiSecret
            );
            _cloudinary = new Cloudinary(acc);
        }
        [HttpGet("{id}", Name = nameof(GetPhoto))]
        public async Task<IActionResult> GetPhoto(int id)
        {
            var photoFromRepo = await _repo.GetPhoto(id);
            var photo = _mapper.Map<PhotoForReturnDto>(photoFromRepo);
            return Ok(photo);
        }
        [HttpPost]
        public async Task<IActionResult> AddPhotoForUser(int userId,
        [FromForm] PhotoForCreationDto photoForCreationDto)
        {
             //Validar que el token corresponda al usuario
            if(userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
            {
                return Unauthorized();
            }
            var userFromRepo = await _repo.GetUser(userId);
            var file = photoForCreationDto.File;
            var uploadResult = new ImageUploadResult();
            if(file.Length>0)
            {
                using (var stream = file.OpenReadStream())
                {
                    var uploadParams = new ImageUploadParams()
                    {
                        File = new FileDescription(file.Name, stream),
                        Transformation = new Transformation()
                        .Width(500).Crop("fill").Gravity("face")
                    };
                    uploadResult = _cloudinary.Upload(uploadParams);
                }
            }
            photoForCreationDto.Url = uploadResult.Uri.ToString();
            photoForCreationDto.PublicId = uploadResult.PublicId;
            var photo = _mapper.Map<Photo>(photoForCreationDto);
            if(!userFromRepo.Photos.Any(u=>u.isMain))
            {
                photo.isMain = true;
            }
            userFromRepo.Photos.Add(photo);
            if(await _repo.SaveAll())
            {
                //return Ok();
                var photoToReturn = _mapper.Map<PhotoForReturnDto>(photo);
            // Return HTTP 201 response and add a Location header to response
            // TODO - fix this, currently throws exception 'InvalidOperationException: No route matches the supplied values.'

               // return CreatedAtRoute("GetPhoto", new { id = photo.Id}, photoToReturn);
               //Aplicado a versionamiento de .net core 2.2.
                //return CreatedAtRoute(nameof(PhotosController.GetPhoto), new { id = photo.Id }, photoToReturn);
                //para .net core 3.1 en adelante hay que especificar mas parametros
                //esto se agrega para que retorne un  201 
                return CreatedAtRoute(nameof(GetPhoto), new { userId, id = photo.Id }, photoToReturn);

               /* return CreatedAtAction(
                        "GetPhoto", 
                        new { id = photo.Id},
                        photoToReturn);
                        */
            }
            return BadRequest("Could not add bad photo");
        }

        [HttpPost("{id}/setMain")]
        public async Task<IActionResult> SetMainPhoto(int userId, int id)
        {
            if(userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();
            var user = await _repo.GetUser(userId);
            if(!user.Photos.Any(p=>p.Id == id))
                return Unauthorized();
            var photoFromRepo = await _repo.GetPhoto(id);
            if(photoFromRepo.isMain)
                return BadRequest("This is already the main photo");
            
         
            var currentMainPhoto = await  _repo.GetMainPhoto(userId);
            currentMainPhoto.isMain = false;

            photoFromRepo.isMain = true;
            if(await _repo.SaveAll())
                return NoContent();
       

            return BadRequest("Could not set photo to main");

        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePhoto(int userId, int id)
        {
            if(userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();
            var user = await _repo.GetUser(userId);
            if(!user.Photos.Any(p=>p.Id == id))
                return Unauthorized();
            var photoFromRepo = await _repo.GetPhoto(id);
            if(photoFromRepo.isMain)
                return BadRequest("You cannot delete your main photo");

            if(photoFromRepo.PublicID != null)
            {
                var deleteParams = new DeletionParams(photoFromRepo.PublicID);
                var resul = _cloudinary.Destroy(deleteParams);
                 
                if(resul.Result == "ok")
                {
                    _repo.Delete(photoFromRepo);
                }
            }
            if(photoFromRepo.PublicID == null)
            {
                 _repo.Delete(photoFromRepo);
            }
            if(await _repo.SaveAll())
                return Ok();
            return BadRequest("Failed to delete the photo");
        }
    }
}
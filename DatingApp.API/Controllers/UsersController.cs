using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using DatingApp.API.Data;
using DatingApp.API.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System;
using DatingApp.API.Helpers;

namespace DatingApp.API.Controllers
{
    //Agregar el filtro para actualizar la fecha de ultima actividad
    [ServiceFilter(typeof(LogUserActivity))]
    [Authorize]
    [ApiController] //Si esto se comenta entonces los valores vienen nulo del modelo al momento dde consumir un metodo de la api
    //[fromBody] sirve para que los valores no los traiga nulo
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IDatingRepository _repo;
        private readonly IMapper _mapper;

        public UsersController(IDatingRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }
        //FromQuery se agrega para permitir un cuerpo vacio en la peticion
        [HttpGet]
        public async Task<IActionResult> GetUsers([FromQuery]UserParams userParams)
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var userFromRepo = await _repo.GetUser(currentUserId);
            userParams.UserId = currentUserId;

            if(string.IsNullOrEmpty(userParams.Gender))
            {
                userParams.Gender = userFromRepo.Gender =="male"?"female":"male";
            }
            /*
            var users = await _repo.GetUsers();
            var usersToReturn = _mapper.Map<IEnumerable<UserForDetailedDto>>(users);
            return Ok(usersToReturn);
            */
            //paginando fase 14
            var users = await _repo.GetUsers(userParams);
            var usersToReturn = _mapper.Map<IEnumerable<UserForDetailedDto>>(users);

            Response.AddPagination(users.CurrentPage, users.PageSize,
            users.TotalCount, users.TotalPages);
            
            return Ok(usersToReturn);

        }
        [HttpGet("{id}", Name = "GetUser")]
        public async Task<IActionResult> GetUser(int id)
        {
            var user = await _repo.GetUser(id);
            var userToReturn = _mapper.Map<UserForDetailedDto>(user);
            return Ok(userToReturn);
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, UserForUpdateDto userForUpdate)
        {
            //Validar que el token corresponda al usuario
            if(id != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
            {
                return Unauthorized();
            }
            var userFromRepo = await _repo.GetUser(id);
            //mapiar los cambios dentro de la variable userFromRepo, para luego aplicar el guardado
            //este codigo lo que hara es actualizar userFromRepo con los valores userForUpdate
            _mapper.Map(userForUpdate, userFromRepo);
            
            if(await _repo.SaveAll())
                return NoContent();
            
            //si  no se logro guardad se procede a notificar mediante una exepcion
            throw new Exception($"Updating user {id} failed on save");
        }
        [HttpPost("{id}/like/{recipientId}")]
        public async Task<IActionResult> LikeUser(int id, int recipientId)
        {
             if(id != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
            {
                return Unauthorized();
            }
            var like =await _repo.GetLike(id, recipientId);
            if(like != null)
            {
                return BadRequest("You already like this user");
            }
            if(await _repo.GetUser(recipientId) == null)
                return NotFound();
            like = new Like
            {
                LikerId = id,
                LikeeId = recipientId
            };
            _repo.Add<Like>(like);

            if(await _repo.SaveAll())
                return Ok();
            return BadRequest("Failed to like user");
        }

    }
}
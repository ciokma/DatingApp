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
    [Route("api/users/{userId}/[controller]")]
    public class MessagesController : ControllerBase
    {
        private readonly IDatingRepository _repo;
        private readonly IMapper _mapper;

        public MessagesController(IDatingRepository repo, IMapper mapper)
        {
            this._repo = repo;
            this._mapper = mapper;
        }
        [HttpGet("{id}", Name = "GetMessage")]
        public async Task<IActionResult> GetMessage(int userId, int id)
        {
             //Validar que el token corresponda al usuario
            if(userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
            {
                return Unauthorized();
            }
            var messageFromRepo = await _repo.GetMessage(id);
            if(messageFromRepo == null)
                return NotFound();
            return Ok(messageFromRepo);
        }
        [HttpGet]
        public async Task<IActionResult> GetMessagesForUser(int userId, 
        [FromQuery] MessageParams messageParams)
        {
             if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            messageParams.UserId = userId;

            var messagesFromRepo = await _repo.GetMessagesForUser(messageParams);

            var messages = _mapper.Map<IEnumerable<MessageToReturnDto>>(messagesFromRepo);

            Response.AddPagination(messagesFromRepo.CurrentPage, messagesFromRepo.PageSize, 
                messagesFromRepo.TotalCount, messagesFromRepo.TotalPages);
            
            return Ok(messages);

        }
        [HttpGet("thread/{recipientId}")]
        public async Task<IActionResult> GetMessageThread(int userId,   int recipientId)
        {
             if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();
            var messageFromRepo = await _repo.GetMessageThread(userId, recipientId);
            var messageThread = _mapper.Map<IEnumerable<MessageToReturnDto>>(messageFromRepo);
            return Ok(messageThread);

        }
        [HttpPost]
        public async Task<IActionResult> CreateMessage(int userId, MessageForCreationDto messageForCreationDto)
        {
            var sender = await _repo.GetUser(userId);
             //Validar que el token corresponda al usuario
            if(sender.Id != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
            {
                return Unauthorized();
            }
            messageForCreationDto.SenderId = userId;

            var recipient = await _repo.GetUser(messageForCreationDto.RecipientId);
            if(recipient == null)
            {
                return BadRequest("Could not find user");
            }
            //mapear
            var message = _mapper.Map<Message>(messageForCreationDto);
            _repo.Add(message);

            if(await _repo.SaveAll())
            {
                var messageToReturn  = _mapper.Map<MessageToReturnDto>(message);
                return CreatedAtAction("GetMessage", new { userId, id= message.Id}, messageToReturn );
            }
            throw new Exception("Creating the message failed on save");
        }
        [HttpPost("{id}")]
        public async Task<IActionResult> DeleteMessage(int id, int userId)
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
               return Unauthorized();
            var messageFromRepo = await _repo.GetMessage(id);
            if(messageFromRepo.SenderId == userId )
            {
                messageFromRepo.SenderDeleted = true;
            }
            if(messageFromRepo.RecipientId == userId )
            {
                messageFromRepo.RecipientDeleted = true;
            }
            if(messageFromRepo.RecipientDeleted && messageFromRepo.SenderDeleted)
            {
                _repo.Delete(messageFromRepo);
            }
            if(await _repo.SaveAll())
                return NoContent();

            throw new Exception("Error deleting the message");
        }
        [HttpPost("{id}/read")]
        public async Task<IActionResult> MarkMessageAsRead(int userId, int id) 
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();
            var message =  await _repo.GetMessage(id);

            if(message.RecipientId != userId)
                return Unauthorized();
            
            message.IsRead = true;
            message.DateRead = DateTime.Now;
            
            await _repo.SaveAll();
            return NoContent();
        }
    }
   
}
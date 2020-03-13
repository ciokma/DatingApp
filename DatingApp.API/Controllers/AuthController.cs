using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using DatingApp.API.Data;
using DatingApp.API.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace DatingApp.API.Controllers
{
    [ApiController] //Si esto se comenta entonces los valores vienen nulo del modelo al momento dde consumir un metodo de la api
    //[fromBody] sirve para que los valores no los traiga nulo
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthRepository _repo;

        public IConfiguration _config;
        private readonly IMapper _mapper;

        public AuthController(IAuthRepository repo, IConfiguration config, IMapper mapper)
        {
            _mapper = mapper;
            _repo = repo;
            _config = config;
        }
        [HttpPost("register")]
        //public async Task<IActionResult> Register([FromBody]UserForRegisterDto userForRegisterDto)
        public async Task<IActionResult> Register(UserForRegisterDto userForRegisterDto)
        {
            /*if(!ModelState.IsValid)
            return BadRequest(ModelState);
            */

            userForRegisterDto.Username = userForRegisterDto.Username.ToLower();
            if (await _repo.UserExists(userForRegisterDto.Username))
                return BadRequest("Username already exists");

            var userToCreate = _mapper.Map<User>(userForRegisterDto);
            /*var userToCreate = new User
            {
                UserName = userForRegisterDto.Username
            };
            */

            var createdUser = await _repo.Register(userToCreate, userForRegisterDto.Password);

            //usuario a regresar
            var userToReturn = _mapper.Map<UserForDetailedDto>(createdUser);
            return CreatedAtRoute("GetUser", new {  Controlle="User", id = createdUser.Id }, userToReturn);
            
            //return StatusCode(201);
        }
        [HttpPost("login")]
        public async Task<IActionResult> Login(UserForLogin userlogin)
        {
            // try{
            //throw new Exception("Computers say no!");

            userlogin.username = userlogin.username.ToLower();
            var userFromRepo = await _repo.Login(userlogin.username, userlogin.password);

            if (userFromRepo == null)
                return Unauthorized();

            //Version 2.0 .net core
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier,userFromRepo.Id.ToString()),
                new Claim(ClaimTypes.Name,userFromRepo.UserName)
            };

            /*
             var claims = new []
             {
                 new Claim(JwtRegisteredClaimNames.Sub, userFromRepo.UserName),
                 new Claim(JwtRegisteredClaimNames.NameId, userFromRepo.Id.ToString()),
                 new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
             };
             */
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config.GetSection("AppSettings:Token").Value));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(1),
                SigningCredentials = creds
            };

            // authentication successful so generate jwt token
            var tokenHandler = new JwtSecurityTokenHandler();

            var token = tokenHandler.CreateToken(tokenDescriptor);

            var user = _mapper.Map<UserForListDto>(userFromRepo);

            return Ok(new
            {
                token = tokenHandler.WriteToken(token),
                user
            });
            /*
            } catch{
                return StatusCode(500,"Computer Says No!");
            }
            */
        }
    }
}
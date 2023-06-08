using System.Security.Cryptography;
using System.Text;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace API.Controllers
{
    public class AccountController : BaseApiController
    {
        private readonly DataContext _context;
        private readonly ITokenService _tokenService;

        public AccountController(DataContext context, ITokenService tokenService)
        {
            _tokenService = tokenService;

            _context = context;

        }

        [HttpPost("register")] // POST: api/account/register
        public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)   //alternatively Register([FromBody]RegisterDto registerDto) when [API controller is disabled]
        {

            if (await UserExists(registerDto.Username))
            {
                return BadRequest("Username is taken");
            }
            using var hmac = new HMACSHA512();
            var user = new AppUser
            {
                UserName = registerDto.Username.ToLower(),
                PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password)),
                PasswordSalt = hmac.Key
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return new UserDto
            {
                Username = user.UserName,
                Token = _tokenService.CreateToken(user)
            };
        }

        [HttpPost("login")] // POST: api/account/login
        public async Task<ActionResult<UserDto>> Login(LoginDto logindto)
        {
            //check for username present in DataBase
            var user = await _context.Users.SingleOrDefaultAsync(x => x.UserName == logindto.Username);

            if (user == null)
            {
                return Unauthorized("Invalid username");
            }

            //Bring the salt of the user
            using var hmac = new HMACSHA512(user.PasswordSalt);

            //Compute the hash password 
            var computedhash = hmac.ComputeHash(Encoding.UTF8.GetBytes(logindto.Password));

            //Iterate over each element of the computed hash and Password Hash
            for (int i = 0; i < computedhash.Length; i++)
            {
                if (computedhash[i] != user.PasswordHash[i])
                {
                    return Unauthorized("Invalid Password");
                }
            }
            return new UserDto
            {
                Username = user.UserName,
                Token = _tokenService.CreateToken(user)
            };

        }


        private async Task<bool> UserExists(string username)
        {
            return await _context.Users.AnyAsync(x => x.UserName == username.ToLower());
        }
    }
}


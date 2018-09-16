using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using WebApiJSONWebToken.DAL;
using WebApiJSONWebToken.Models;

namespace WebApiJSONWebToken.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private UserRepository _userRepository;
        public AuthenticationController(UserRepository userRepository) => _userRepository = userRepository;

        [HttpPost("login")]
        [ProducesResponseType(200, Type = typeof(JwtSecurityTokenHandler))]
        [ProducesResponseType(400)]
        public IActionResult Login([FromBody] User userParam)
        {
            var user = _userRepository.Login(userParam.Username, userParam.Password);

            if (user == null)
                return BadRequest(new { message = "Username or password is incorrect" });


            var userRole = (user.IsAdmin) ? "admin" : "user";

            var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("MySecretKey010203"));
            var signinCredentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);

            var tokenOptions = new JwtSecurityToken(
                claims: new List<Claim> {
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Role, userRole)
                },
                expires: DateTime.Now.AddDays(2),
                signingCredentials: signinCredentials
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(tokenOptions);
            return Ok(new { Token = tokenString });
                        
        }
    }
}
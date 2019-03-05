using System;
using System.Threading.Tasks;
using BaseApi.Helper;
using BaseApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BaseApi.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class UsersController : Controller
    {
        private readonly DBcontext _dbContext = new DBcontext();
        
        [AllowAnonymous]
        [HttpPost("[action]")]
        public async Task<IActionResult> Register([FromBody] User user)
        {
            try
            {
                if (user == null)
                {
                    throw new BadRequestException(message: "user object is null");
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState.ToBadRequest());
                }

                bool isEmailAlreadyTaken =
                    await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == user.Email) != null;
                if (isEmailAlreadyTaken)
                {
                    throw new BadRequestException(message: "user with this email already exist");
                }

                user.SetPasswordhHash();
                await _dbContext.Users.AddAsync(user);
                await _dbContext.SaveChangesAsync();
                return Created("register", user.ToMessage());
            }
            catch (Exception e)
            {
                if (e is BadRequestException)
                {
                    return BadRequest(e.Message.ToBadRequest());
                }

                return StatusCode(500);
            }
        }

        [AllowAnonymous]
        [HttpPost("[action]")]
        public async Task<IActionResult> Auth([FromBody] User user)
        {
            try
            {
                if (user == null)
                {
                    throw new BadRequestException(message: "user object is null");
                }

                User userFromDb = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == user.Email);
                if (userFromDb == null)
                {
                    throw new BadRequestException(message: "Wrong login");
                }

                if (!userFromDb.Compare(userSubmittedPassword: user.Password))
                {
                    throw new BadRequestException(message: "Wrong password");
                }
                
                return Ok(JWT.GetToken(user));
            }
            catch (Exception e)
            {
                if (e is BadRequestException)
                {
                    return BadRequest(e.Message.ToBadRequest());
                }

                return StatusCode(500);
            }
        }
    }
}
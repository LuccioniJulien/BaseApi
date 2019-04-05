using System;
using System.Linq;
using System.Threading.Tasks;
using BaseApi.Helper;
using BaseApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BaseApi.Controllers {
    [Authorize (AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]

    [Route ("api/[controller]")]
    public class UsersController : Controller {
        private readonly DBcontext _dbContext;

        public UsersController (DBcontext context) {
            _dbContext = context;
        }

        [AllowAnonymous]
        [HttpPost ("[action]")]
        public ActionResult<User> Register ([FromBody] User user) {
            try {
                if (user == null) {
                    throw new BadRequestException ("user object is null");
                }

                if (!ModelState.IsValid) {
                    return BadRequest (ModelState.ToBadRequest ());
                }

                bool isEmailAlreadyTaken = _dbContext.Users.FirstOrDefault (u => u.Email == user.Email) != null;
                if (isEmailAlreadyTaken) {
                    throw new BadRequestException ("user with this email already exist");
                }

                user.SetPasswordhHash ();
                _dbContext.Users.Add (user);
                _dbContext.SaveChanges ();
                var response = new {
                    data = user.ToMessage ()
                };
                return Created ("register", response);
            } catch (Exception e) {
                return BadRequest (e.Message.ToBadRequest ());
            }
        }

        [AllowAnonymous]
        [HttpPost ("[action]")]
        public ActionResult<string> Auth ([FromBody] User user) {
            try {
                if (user == null) {
                    throw new BadRequestException ("user object is null");
                }

                User userFromDb = _dbContext.Users.FirstOrDefault (u => u.Email == user.Email);
                if (userFromDb == null) {
                    throw new BadRequestException ("Wrong email");
                }

                if (!userFromDb.Compare (user.Password)) {
                    throw new BadRequestException ("Wrong password");
                }
                var token = JWT.GetToken (userFromDb);
                var response = new {
                    meta =
                    token
                };
                return Ok (response);
            } catch (Exception e) {
                return BadRequest (e.Message.ToBadRequest ());
            }
        }

        [HttpPut ("{id}")]
        public ActionResult<string> Put (string id, [FromBody] User user) {
            try {
                var uuid = Guid.Parse (User.Identity.Name.ToString ());
                var uuidFromQuery = Guid.Parse (id);

                if (user == null) {
                    throw new BadRequestException ("user object is null");
                }

                User userFromDb = _dbContext.Users.FirstOrDefault (u => u.Id == uuidFromQuery);
                User userFromTokenId = _dbContext.Users.FirstOrDefault (u => u.Id == uuid);

                if ((userFromTokenId == null) || (userFromDb?.Id != userFromTokenId?.Id)) {
                    return Unauthorized ();
                }
                User userWithSameLogin = _dbContext.Users.FirstOrDefault (u => u.Email == user.Email);
                if (userWithSameLogin != null) {
                    throw new BadRequestException ("Login already taken");
                }
                if (!ModelState.IsValid) {
                    return BadRequest (ModelState.ToBadRequest ());
                }

                var (name, email, password, passwordConfirmation) = user;
                userFromDb.Name = name;
                userFromDb.Email = email;
                userFromDb.Password = password;
                userFromDb.PasswordConfirmation = passwordConfirmation;
                userFromDb.SetPasswordhHash ();

                _dbContext.Users.Update (userFromDb);
                _dbContext.SaveChanges ();

                return StatusCode (201);
            } catch (Exception e) {
                return BadRequest (e.Message.ToBadRequest ());
            }
        }

        [HttpDelete ("{id}")]
        public ActionResult<string> Delete (string id) {
            try {
                var uuid = Guid.Parse (User.Identity.Name.ToString ());
                var uuidFromQuery = Guid.Parse (id);

                User userFromDb = _dbContext.Users.FirstOrDefault (u => u.Id == uuidFromQuery);
                User userFromTokenId = _dbContext.Users.FirstOrDefault (u => u.Id == uuid);

                if ((userFromTokenId == null) || (userFromDb?.Id != userFromTokenId?.Id)) {
                    return Unauthorized ();
                }

                _dbContext.Users.Remove (userFromDb);
                _dbContext.SaveChanges ();

                return StatusCode (204);
            } catch (Exception e) {
                return BadRequest (e.Message.ToBadRequest ());
            }
        }
    }
}
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BaseApi.Helper;
using Microsoft.IdentityModel.Tokens;

namespace BaseApi.Models
{
    public class User
    {
        public Guid Id { get; set; }

        [Required, MinLength(5)]
        public string Name { get; set; }
        
        [EmailAddress, Required] 
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        
        [Required, NotMapped] 
        public string Password { get; set; }

        [Required, Compare("Password"), NotMapped]
        public string PasswordConfirmation { get; set; }

        public void SetPasswordhHash()
        {
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(input: Password);
        }

        public bool Compare(string userSubmittedPassword)
        {
            return BCrypt.Net.BCrypt.Verify(userSubmittedPassword, PasswordHash);
        }

        public object ToMessage()
        {
            return new
            {
                Id,
                Name,
                Email
            };
        }
    }
}
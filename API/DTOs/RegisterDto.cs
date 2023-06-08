using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace API.DTOs
{
    public class RegisterDto
    {
        //Various Annotations can be used here,  [RegularExpression], [Max Length], [Phone],[EmailAddress] etc
        [Required]
        public string Username { get; set; }
        [Required]
        
        public string Password { get; set; }
    }
}
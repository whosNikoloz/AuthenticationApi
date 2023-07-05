﻿using System.ComponentModel.DataAnnotations;

namespace authenticationAPI.Model
{
    public class UserRegisterRequest
    {
        [Required , EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required , MinLength(4) , MaxLength(12)]
        public string UserName { get; set; } = string.Empty;

        [Required, MinLength(6,ErrorMessage ="Please enter at least 6 characters")]
        public string Password { get; set; } = string.Empty;

        [Required, Compare("Password")]
        public string ConfirmPassword { get; set; } = string.Empty;

    }
}

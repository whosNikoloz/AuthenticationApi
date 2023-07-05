﻿using System.Reflection.Metadata.Ecma335;

namespace authenticationAPI.Model
{
    public class User
    {
        public int Id { get; set; }

        public string? UserName { get; set; }

        public string? Email { get; set; } = string.Empty;

        public byte[]? PasswordHash { get; set; } = new byte[32];

        public byte[]? PasswordSalt { get; set; } = new byte[32];

        public string? VerificationToken { get; set; }

        public DateTime? VerifiedAt { get; set; }

        public string? PasswordResetToken { get; set; }

        public DateTime? ResetTokenExpires { get; set; }

        public string Role { get; set; } = string.Empty;


    }
}

﻿using authenticationAPI.Data;
using authenticationAPI.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Net.Mail;
using System.Net;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using System;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Newtonsoft.Json.Linq;
using authenticationAPI.Model.LoginRequest;
using System.Web.Http.Cors;


namespace authenticationAPI.Controllers
{

    [ApiController]
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly DataDbContext _context;
        private readonly IConfiguration _configuration;

        public UserController(DataDbContext context,IConfiguration configuration)
        {
            _configuration = configuration;
            _context = context;
        }
       

        [HttpGet("Users"), Authorize(Roles = "admin")]
        public async Task<IActionResult> GetUsers()
        {
            return Ok(await _context.Users.ToListAsync());
        }


        [HttpGet("UserName"), Authorize]
        public async Task<IActionResult> GetUser(string username)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == username);

            if (user == null)
            {
                return BadRequest("No User");
            }
            return Ok(user);
        }



        [HttpPost("register")]
        public async Task<IActionResult> Register(UserRegisterRequest request)
        {
            if (_context.Users.Any(u => u.Email == request.Email) || _context.Users.Any(u => u.UserName == request.UserName))
            {
                return BadRequest("User (Email or Username) already exists.");
            }

            CreatePasswordHash(request.Password, out byte[] passwordHash, out byte[] passwordSalt);

            var user = new User
            {
                Email = request.Email,
                UserName = request.UserName,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt,
                VerificationToken = CreateRandomToken()
            };

            // Check if it's the first registered user
            if (!_context.Users.Any())
            {
                user.Role = "admin"; // Assign "admin" role
                user.VerifiedAt = DateTime.Now;
            }
            else
            {
                user.Role = "user"; // Assign "user" role
            }

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            

            string verificationLink = Url.Action("Verify", "User", new { token = user.VerificationToken }, Request.Scheme);

            await SendVerificationEmail(user.Email, verificationLink);

            return Ok("User successfully created. Verification email sent.");
        }



        [HttpPost("loginWithEmail")]
        public async Task<IActionResult> Login(UserLoginRequest request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user == null)
            {
                return BadRequest("User not found.");
            }

            if (!VerifyPasswordHash(request.Password, user.PasswordHash, user.PasswordSalt))
            {
                return BadRequest("Wrong password.");
            }

            if (user.VerifiedAt == null)
            {
                return BadRequest("User not verified.");
            }

            string jwttoken = CreateToken(user);

            return Ok(new { User = user, Token = jwttoken });

        }

        [HttpPost("loginWithUserName")]
        public async Task<IActionResult> LoginUserName(UserLoginUserNameRequest request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == request.UserName);

            if (user == null)
            {
                return BadRequest("User not found.");
            }

            if (!VerifyPasswordHash(request.Password, user.PasswordHash, user.PasswordSalt))
            {
                return BadRequest("Wrong password.");
            }

            if (user.VerifiedAt == null)
            {
                return BadRequest("User not verified.");
            }

            string token = CreateToken(user);

            return Ok(user);
        }
        [HttpPost("loginWithPhoneNumber")]
        public async Task<IActionResult> LoginPhone(UserLoginPhoneRequest request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.PhoneNumber == request.PhoneNumber);

            if (user == null)
            {
                return BadRequest("User not found.");
            }

            if (!VerifyPasswordHash(request.Password, user.PasswordHash, user.PasswordSalt))
            {
                return BadRequest("Wrong password.");
            }

            if (user.VerifiedAt == null)
            {
                return BadRequest("User not verified.");
            }

            string token = CreateToken(user);

            return Ok(user);
        }

        //Changess//
        [HttpPost("Change-password"), Authorize]
        public async Task<IActionResult> changepassword(User requestuser, string newpassword, string oldpassword)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == requestuser.Email);

            if (user == null)
            {
                return BadRequest("user not found.");
            }
            if (!VerifyPasswordHash(oldpassword, requestuser.PasswordHash, requestuser.PasswordSalt))
            {
                return BadRequest("Wrong password.");
            }

            CreatePasswordHash(newpassword, out byte[] passwordHash, out byte[] passwordSalt);


            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;
            user.PasswordResetToken = null;
            user.ResetTokenExpires = null;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception occurred during SaveChangesAsync: " + ex.Message);
            }


            return Ok(requestuser);
        }

        [HttpPost("Change-usernameornumber"), Authorize]
        public async Task<IActionResult> changeusername(User requestuser)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == requestuser.Email);

            if (user == null)
            {
                return BadRequest("user not found.");
            }

            user.UserName = requestuser.UserName;
            user.PhoneNumber = requestuser.PhoneNumber;

            try
            {
                await _context.SaveChangesAsync();
                return Ok("Successfully changed Username or number");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception occurred during SaveChangesAsync: " + ex.Message);
                return StatusCode(500, "An error occurred while saving changes.");
            }
        }

        [HttpPost("userimage"), Authorize]
        public async Task<IActionResult> userimage(User imagerequest)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == imagerequest.Email);

            if (user == null)
            {
                return BadRequest("user not found.");
            }

            user.Picture = imagerequest.Picture;

            try
            {
                await _context.SaveChangesAsync();
                return Ok("Successfully changed Username or number");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception occurred during SaveChangesAsync: " + ex.Message);
                return StatusCode(500, "An error occurred while saving changes.");
            }
        }




        [HttpGet("verify")]
        public async Task<IActionResult> Verify(string token)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.VerificationToken == token);

            if (user == null)
            {
                return BadRequest("Invalid token.");
            }

            user.VerifiedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            return Ok("User verified successfully.");
        }


        [HttpPost("Forgot-password")]
        public async Task<IActionResult> ForgotPassowrd(string email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
            {
                return BadRequest("User Not Found");
            }


            user.PasswordResetToken = CreateRandomToken();
            user.ResetTokenExpires = DateTime.Now.AddDays(1);

            string returnUrl = "https://localhost:7070/Account/ResetPassword";

            string verificationLink = $"{returnUrl}?token={user.PasswordResetToken}";

            await _context.SaveChangesAsync();

            await SendEmail(email, verificationLink);

            return Ok($"You may reset your password now.");
        }


        [HttpPost("Reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordRequest request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.PasswordResetToken == request.Token);

            if (user == null || user.ResetTokenExpires < DateTime.Now)
            {
                return BadRequest("Invalid Token");
            }


            CreatePasswordHash(request.Password, out byte[] passwordHash, out byte[] passwordSalt);

            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;
            user.PasswordResetToken = null;
            user.ResetTokenExpires = null;

            await _context.SaveChangesAsync();

            return Ok($"Password Succesfully resets.");
        }





        private async Task SendVerificationEmail(string email, string confirmationLink)
        {
            string messageBody = $@"
            <h1>Account Verification</h1>
            <p>Thank you for signing up with our service. To activate your account, please click the button below:</p>
            <a href=""{confirmationLink}"" style=""background-color: #007BFF; color: white; padding: 14px 20px; text-align: center; text-decoration: none; display: inline-block; border-radius: 4px; font-size: 16px; margin: 10px auto; display: block;"">Verify Account</a>
            <p>If you are having trouble with the button, you can also click the link below:</p>
            <a href=""{confirmationLink}"">Verify</a>
            <img src=""https://static.vecteezy.com/system/resources/previews/008/132/083/original/green-tree-cartoon-isolated-on-white-background-illustration-of-green-tree-cartoon-free-vector.jpg"" alt=""Your Logo"" style=""display: block;width:400px;height:331px; margin: 20px auto;"">
        ";

             using (MailMessage message = new MailMessage("noreplynika@gmail.com", email))
            {
                message.Subject = "Email Verification";
                message.Body = messageBody;
                message.IsBodyHtml = true;

                using (SmtpClient smtpClient = new SmtpClient("smtp.gmail.com", 587))
                {
                    smtpClient.Credentials = new NetworkCredential("noreplynika@gmail.com", "cdqwvhmdwljietwq");
                    smtpClient.EnableSsl = true;

                    try
                    {
                        await smtpClient.SendMailAsync(message);
                    }
                    catch (Exception)
                    {
                        // Handle any exception that occurs during the email sending process
                        // You can log the error or perform other error handling actions
                    }
                }
            }
        }
        private async Task SendEmail(string email, string confirmationLink)
        {
            string messageBody = $@"
            <h1>Reset Password</h1>
            <p>Please click the button below to reset your password:</p>
            <a href=""{confirmationLink}"" style=""background-color: #4CAF50; color: white; padding: 14px 20px; text-align: center; text-decoration: none; display: inline-block; border-radius: 4px; font-size: 16px; margin: 10px auto; display: block;"">Reset Password</a>
            <p>If you are having trouble with the button, you can also click the link below:</p>
            <a href=""{confirmationLink}"">Reset Password</a>
            <img src=""https://static.vecteezy.com/system/resources/previews/008/132/083/original/green-tree-cartoon-isolated-on-white-background-illustration-of-green-tree-cartoon-free-vector.jpg"" alt=""Your Logo"" style=""display: block;width:400px;height:311px; margin: 20px auto;"">
             ";

            using (MailMessage message = new MailMessage("noreplynika@gmail.com", email))
            {
                message.Subject = "Email Verification";
                message.Body = messageBody;
                message.IsBodyHtml = true;

                using (SmtpClient smtpClient = new SmtpClient("smtp.gmail.com", 587))
                {
                    smtpClient.Credentials = new NetworkCredential("noreplynika@gmail.com", "cdqwvhmdwljietwq");
                    smtpClient.EnableSsl = true;

                    try
                    {
                        await smtpClient.SendMailAsync(message);
                    }
                    catch (Exception)
                    {
                        // Handle any exception that occurs during the email sending process
                        // You can log the error or perform other error handling actions
                    }
                }
            }
        }



        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }

        private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512(passwordSalt))
            {
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return computedHash.SequenceEqual(passwordHash);
            }
        }

        private string CreateRandomToken()
        {
            return Convert.ToHexString(RandomNumberGenerator.GetBytes(64));
        }


        private string CreateToken(User user)
        {
            List<Claim> calims = new List<Claim>
            {
               new Claim(ClaimTypes.Name, user.UserName),
               new Claim("Picture", user.Picture),
               new Claim(ClaimTypes.Email, user.Email),
               new Claim(ClaimTypes.Role, user.Role)
            };

            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(
                _configuration.GetSection("AppSettings:Token").Value));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var token = new JwtSecurityToken(
                claims: calims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: creds);

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            return jwt;
        }


    }
}

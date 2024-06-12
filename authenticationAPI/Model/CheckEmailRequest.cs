using System.ComponentModel.DataAnnotations;

namespace authenticationAPI.Model.User
{
    public class CheckEmailRequest
    {
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
    }
}

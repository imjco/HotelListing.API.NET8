
using System.ComponentModel.DataAnnotations;

namespace HotelListingAPI.Models.Users
{
    public class LoginDto
    {
        [Required]
        [EmailAddress]
        public String Email { get; set; }

        [Required]
        [StringLength(15, ErrorMessage = "Your password is limited to {2} to {1} characters", MinimumLength = 6)]
        public String Password { get; set; }
    }

}

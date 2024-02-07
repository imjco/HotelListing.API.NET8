
using System.ComponentModel.DataAnnotations;

namespace HotelListingAPI.Models.Users
{
    public class ApiUserDto: LoginDto
    {
        [Required]
        public String FirstName { get; set; }

        [Required]
        public String LastName { get; set; }


        public String ContactNumber { get; set; }
       
    }

}

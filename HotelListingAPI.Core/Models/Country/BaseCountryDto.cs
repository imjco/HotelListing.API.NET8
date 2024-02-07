using System.ComponentModel.DataAnnotations;

namespace HotelListingAPI.Core.Models.Country
{
    public class BaseCountryDto
    {
        [Required]
        public String Name { get; set; }
        public String ShortName { get; set; }
    }
}

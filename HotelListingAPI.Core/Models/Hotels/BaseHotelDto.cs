using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelListingAPI.Core.Models.Hotels
{
    public abstract class BaseHotelDto
    {

        [Required]
        public String Name { get; set; }

        [Required]
        public String Address { get; set; }

        public double? Rating { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int CountryId { get; set; }

       
    }
}

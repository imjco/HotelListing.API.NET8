using System.ComponentModel.DataAnnotations.Schema;

namespace HotelListingAPI.Models.Hotels
{
    public class BaseHotelDto
    {
        

        public String Name { get; set; }

        public String Address { get; set; }

        public double Rating { get; set; }

        [ForeignKey(nameof(CountryId))]
        public int CountryId { get; set; }

       
    }
}

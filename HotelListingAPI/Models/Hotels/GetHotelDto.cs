using System.ComponentModel.DataAnnotations.Schema;

namespace HotelListingAPI.Models.Hotels
{
    public class GetHotelDto
    {
        public int Id { get; set; }

        public String Name { get; set; }

        public String Address { get; set; }

        public double Rating { get; set; }

        public int CountryId { get; set; }

        //public Country Country { get; set; }
    }
}

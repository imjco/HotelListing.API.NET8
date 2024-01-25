using System.ComponentModel.DataAnnotations.Schema;

namespace HotelListingAPI.Data
{
    public class Hotel
    {
        public int Id { get; set; }

        public String Name { get; set; }

        public String Address { get; set; }

        public double Rating { get; set; }

        [ForeignKey(nameof(CountryId))]
        public int CountryId { get; set; }

        public Country Country { get; set; }

    }

  
}

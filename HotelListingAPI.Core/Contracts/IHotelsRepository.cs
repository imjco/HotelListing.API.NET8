using HotelListingAPI.Data;

namespace HotelListingAPI.Core.Contracts
{
    public interface IHotelsRepository : IGenericRepository<Hotel>
    {
        Task<Hotel> GetDetails(int id);
      
    }
}

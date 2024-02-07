using AutoMapper;
using HotelListingAPI.Contracts;
using HotelListingAPI.Data;
using Microsoft.EntityFrameworkCore;

namespace HotelListingAPI.Repository
{
    public class HotelRepository : GenericRepository<Hotel>, IHotelsRepository
    {
        public HotelListingDbContext _context { get; }
        public HotelRepository(HotelListingDbContext context, IMapper mapper) : base(context, mapper)
        {
            _context = context;
        }

        public async Task<Hotel> GetDetails(int id)
        {
            return await _context.Hotels.Include(x => x.Country).
                  FirstOrDefaultAsync(x => x.Id == id);
        }
    }
}

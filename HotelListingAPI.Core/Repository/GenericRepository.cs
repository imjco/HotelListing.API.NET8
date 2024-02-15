using AutoMapper;
using AutoMapper.QueryableExtensions;
using HotelListingAPI.Core.Contracts;
using HotelListingAPI.Data;
using HotelListingAPI.Core.Models;
using Microsoft.EntityFrameworkCore;
using HotelListingAPI.Core.Exceptions;

namespace HotelListingAPI.Core.Repository
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        private readonly HotelListingDbContext _context;
        private readonly IMapper _mapper;
        private HotelListingDbContext context;

        public GenericRepository(HotelListingDbContext context)
        {
            this.context = context;
        }

        public GenericRepository(HotelListingDbContext context, IMapper mapper)
        {
            _context = context;
            this._mapper = mapper;
        }

        public async Task<T> AddAsync(T entity)
        {
            await _context.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;

        }

        public async Task<TResult> AddAsync<TSource, TResult>(TSource source)
        {
           var entiry = _mapper.Map<T>(source);
            await _context.AddAsync(entiry);
            await _context.SaveChangesAsync();

            return _mapper.Map<TResult>(entiry);
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await GetAsync(id);
            if (entity is null)
            {
                throw new NotFoundException(typeof(T).Name, id);
            }
            _context.Set<T>().Remove(entity);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> Exists(int id)
        {
            var entity = await GetAsync(id);
            return entity != null;
        }

        public async Task<IList<T>> GetAllAsync()
        {
           return await _context.Set<T>()
                .ProjectTo<T>(_mapper.ConfigurationProvider)
                .ToListAsync();
        }

        public async Task<PagedResult<TResult>> GetAllAsync<TResult>(QueryParameters queryParameters)
        {
            var totalSize = await _context.Set<T>().CountAsync();
            var items = await _context.Set<T>()
                .Skip(queryParameters.StartIndex)
                .Take(queryParameters.PageSize)
                .ProjectTo<TResult>(_mapper.ConfigurationProvider)
                .ToListAsync();
            return new PagedResult<TResult>
            {
                Items = items,
                PageNumber = queryParameters.PageNumber,
                RecordNumber = queryParameters.PageSize,
                TotalCount = totalSize
            };
        }

        public async Task<List<TResult>> GetAllAsync<TResult>()
        {
            return await _context.Set<T>()
                .ProjectTo<TResult>(_mapper.ConfigurationProvider)
                .ToListAsync();
        }

        public async Task<T> GetAsync(int? id)
        {
            if(id == null)
            {
                return null;
            }

            return await _context.Set<T>().FindAsync(id);   

        }

        public async Task<TResult> GetAsync<TResult>(int? id)
        {
            var result = await _context.Set<T>().FindAsync(id);
            if (result == null)
            {
                throw new NotFoundException(typeof(T).Name, id.HasValue ? id : "No Key Provided");
            }
            
            return _mapper.Map<TResult>(result);
        }

        public async Task UpdateAsync(T entity)
        {
           _context.Update(entity);
            await _context.SaveChangesAsync();


        }

        public async Task UpdateAsync<TSource>(int id, TSource source)
        {
            var entity = await GetAsync(id);
            if (entity is null)
            {
                throw new NotFoundException(typeof(T).Name, id);
            }
            _mapper.Map(source, entity);
            _context.Update(entity);
            await _context.SaveChangesAsync();
        }
    }
}

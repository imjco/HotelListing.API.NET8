using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HotelListingAPI.Data;
using HotelListingAPI.Core.Models.Country;
using AutoMapper;
using HotelListingAPI.Core.Contracts;
using Microsoft.AspNetCore.Authorization;
using HotelListingAPI.Core.Exceptions;
using HotelListingAPI.Core.Models;
using Microsoft.AspNetCore.OData.Query;

namespace HotelListingAPI.Controllers
{
     [Route("api/v{version:ApiVersion}/countries")]
   // [Route("api/countries")]
    [ApiController]
    [ApiVersion("2.0")]
   
    public class Countriesv2Controller : ControllerBase
    {
       
        private readonly IMapper _mapper;
        private readonly ICountriesRepository _countriesRepository;
        private readonly ILogger<CountriesController> _logger;

        public Countriesv2Controller(IMapper mapper, ICountriesRepository countriesRepository, ILogger<CountriesController> logger)
        {
            _countriesRepository = countriesRepository;
            this._logger = logger;
            _mapper = mapper;
           
        }

        // GET: api/Countries
        [HttpGet("GetAll")]
        [EnableQuery]
        public async Task<ActionResult<IEnumerable<GetCountryDto>>> GetCountries()
        {

            // return me with pagination
            var countries = await _countriesRepository.GetAllAsync();
            var records = _mapper.Map<List<GetCountryDto>>(countries);
            return Ok(records);
        }

        // GET: api/Countries/?StartIndex=0&PageSize=25&PageNumber=1
        [HttpGet]
        [EnableQuery]
        public async Task<ActionResult<PagedResult<GetCountryDto>>> GetPagedCountries([FromQuery] QueryParameters queryParameters)
        {

            // return me with pagination
            var pagedCountriesResult = await _countriesRepository.GetAllAsync<GetCountryDto>(queryParameters);
            return Ok(pagedCountriesResult);
        }


        // GET: api/Countries/5
        [HttpGet("{id}")]
        public async Task<ActionResult<CountryDto>> GetCountry(int id)
        {

            var country = await _countriesRepository.GetDetails(id);

            if (country == null)
            {
                throw new NotFoundExcemption(nameof(country), id);
                //return NotFound();
            }
            var countryDto = _mapper.Map<CountryDto>(country);

            return Ok(countryDto);
        }

        // PUT: api/Countries/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> PutCountry(int id, UpdateCountryDto updateCountryDto)
        {
            if (id != updateCountryDto.Id)
            {
                return BadRequest("invalid record id");
            }

            var country = await _countriesRepository.GetAsync(id);
            if (country == null)
            {
                throw new NotFoundExcemption(nameof(country), id);
                //return NotFound();
            }

            _mapper.Map(updateCountryDto, country);



            try
            {
                await _countriesRepository.UpdateAsync(country);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (! await CountryExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Countries
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<Country>> PostCountry(CreateCountryDto createCountryDto)
        {
           var country = _mapper.Map<Country>(createCountryDto);

           await  _countriesRepository.AddAsync(country);
           
           return CreatedAtAction("GetCountry", new { id = country.Id }, country);
        }

        // DELETE: api/Countries/5
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteCountry(int id)
        {
            var country = await _countriesRepository.GetAsync(id);
            if (country == null)
            {
                throw new NotFoundExcemption(nameof(country), id);
                // return NotFound();
            }

          await _countriesRepository.DeleteAsync(country.Id);

          return NoContent();
        }

        private async Task<bool> CountryExists(int id)
        {
            return await _countriesRepository.Exists(id);

        }
    }
}

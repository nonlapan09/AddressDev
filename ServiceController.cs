using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using NetCoreWebServiceAPI_AddressTK.Model.Address;
using NetCoreWebServiceAPI_AddressTK.Model.Address.Repository;
using NetCoreWebServiceAPI_AddressTK.Model.Dtos;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using NetCoreWebServiceAPI_AddressTK.Model.Address.Person;
using System.Text.RegularExpressions;

namespace NetCoreWebServiceAPI_AddressTK.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ApiController]
    [Route("[controller]")]
    [Produces(MediaTypeNames.Application.Json)]
    [Consumes(MediaTypeNames.Application.Json)]
    public class ServiceController : ControllerBase
    {
        private readonly ILogger<ServiceController> _logger;

        private AddressRepo _AddressRepo;

        private readonly IDistributedCache _cache;

        //public IActionResult Index()
        //{
        //    return View();
        //}
        public ServiceController(ILogger<ServiceController> logger, AddressRepo addressRepo, IDistributedCache cache)
        {
            _logger = logger;
            _AddressRepo = addressRepo;
            _cache = cache;
        }

        // POST: api/RequestOrder
        [HttpPost]
        [Route("/[controller]/[action]/")]
        [ProducesResponseType(typeof(List<ResponsePostCode>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(string), StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        //public async Task<ActionResult<ResponseOrder>> RequestOrder(RequestOrder JsonRequestOrder)
        public async Task<ActionResult<ResponsePostCode>> GetAddressByPostcode(RequestAddressByPostcode JsonRequest)
        {
            _logger.LogInformation("Requested: GetAddressByPostcode | Start | ");
            _logger.LogInformation("Requested: GetAddressByPostcode | Process | " + JsonRequest.Postcode);
            //_logger.LogInformation("Requested: GetAddressByPostcode | Process | " + JsonRequest.Postcode + " | " +  JsonConvert.SerializeObject(JsonRequest));
            _logger.LogInformation("Requested: GetAddressByPostcode | Process | " + JsonRequest.Postcode + " | " + JsonSerializer.Serialize(JsonRequest));


            //cacheResult = await _cache.GetObjectAsync<ResponseAddressByPostcode>((JsonRequest.Postcode.ToString().Trim()));
            string redisKey = JsonRequest.Postcode.ToString().Trim();

            //byte[] dataFromRedis = await _cache.GetAsync("redisKey");

            var dataFromRedis = await _cache.GetAsync(redisKey);
            //if (JsonRequest.ClearCache == true)
            //{
            //    _cache.RemoveAsync(redisKey);
            //}
            ///var dataFromRedis = await _cache.GetObjectAsync<ResponseAddressByPostcode>("Postcode:" + redisKey);

            List<ResponsePostCode> ResponseAddress = new();
            if (JsonRequest.ClearCache == true)
            {
                _cache.RemoveAsync(redisKey);
            }
            //if ((dataFromRedis?.Count() ?? 0) > 0)
            if (dataFromRedis != null)
            {
                //var resultWithRedis = _AddressRepo.GetAddressByPostCode(JsonRequest.Postcode.ToString().Trim());
                //var cars = JsonSerializer.Deserialize<List<ResponseAddressByPostcode>>(carsString);


                var resultStringWithRedis = Encoding.UTF8.GetString(dataFromRedis);

                ResponseAddress = JsonSerializer.Deserialize<List<ResponsePostCode>>(resultStringWithRedis);

                return Ok(new { LoadedFromRedis = true, Data = ResponseAddress });
            }
            var result = _AddressRepo.GetAddressByPostcode(JsonRequest.Postcode.ToString().Trim());
           

            /*string cachedDataString = JsonSerializer.Serialize(result);*/
            var resultDatabaseString = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(result));
            ResponseAddress = JsonSerializer.Deserialize<List<ResponsePostCode>>(resultDatabaseString);

            // Setting up the cache options
            DistributedCacheEntryOptions options = new DistributedCacheEntryOptions()
                .SetAbsoluteExpiration(DateTime.Now.AddMinutes(5))
                .SetSlidingExpiration(TimeSpan.FromMinutes(3));

            // Add the data into the cache
            await _cache.SetAsync(redisKey, resultDatabaseString);



            ///await _cache.SetObjectAsync("Postcode:" + redisKey, result);

            return Ok(new { LoadedFromRedis = false, Data = ResponseAddress, options });


            //return Ok(cacheResult);

        }
        
        [HttpPost]
        [Route("/[controller]/[action]/")]
        [ProducesResponseType(typeof(List<ResponseAddressByPostcode>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(string), StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ResponseAddressByPostcode>> GetAddressByPostcodeWithHNO (RequestAddressByPostcodeWithHNO JsonRequest)
        {
            _logger.LogInformation("Requested: GetAddressByPostcode | Start | ");
            _logger.LogInformation("Requested: GetAddressByPostcode | Process | " + JsonRequest.HNO +"|"+ JsonRequest.Postcode);
            //_logger.LogInformation("Requested: GetAddressByPostcode | Process | " + JsonRequest.Postcode + " | " +  JsonConvert.SerializeObject(JsonRequest));
            _logger.LogInformation("Requested: GetAddressByPostcode | Process | " + JsonRequest.HNO + "|" + JsonRequest.Postcode + " | " + JsonSerializer.Serialize(JsonRequest));


            //cacheResult = await _cache.GetObjectAsync<ResponseAddressByPostcode>((JsonRequest.Postcode.ToString().Trim()));
            string redisKey = JsonRequest.HNO.ToString().Trim() + JsonRequest.Postcode.ToString().Trim();
            
            //byte[] dataFromRedis = await _cache.GetAsync("redisKey");

            var dataFromRedis = await _cache.GetAsync(redisKey);
            if (JsonRequest.ClearCache == true)
            {
                _cache.RemoveAsync(redisKey);
            }
            ///var dataFromRedis = await _cache.GetObjectAsync<ResponseAddressByPostcode>("Postcode:" + redisKey);

            List<ResponseAddressByPostcode> ResponseAddress = new();

            //if ((dataFromRedis?.Count() ?? 0) > 0)
            if (dataFromRedis != null)
            {
                //var resultWithRedis = _AddressRepo.GetAddressByPostCode(JsonRequest.Postcode.ToString().Trim());
                //var cars = JsonSerializer.Deserialize<List<ResponseAddressByPostcode>>(carsString);


                var resultStringWithRedis = Encoding.UTF8.GetString(dataFromRedis);

                ResponseAddress = JsonSerializer.Deserialize<List<ResponseAddressByPostcode>>(resultStringWithRedis);

                return Ok(new { LoadedFromRedis = true, Data = ResponseAddress });
            }
            var result = _AddressRepo.GetAddressByPostcodeWithHNO(JsonRequest.HNO.ToString().Trim(), JsonRequest.Postcode.ToString().Trim());


            string cachedDataString = JsonSerializer.Serialize(result);
            var resultDatabaseString = Encoding.UTF8.GetBytes(cachedDataString);
            ResponseAddress = JsonSerializer.Deserialize<List<ResponseAddressByPostcode>>(resultDatabaseString);

            // Setting up the cache options
            DistributedCacheEntryOptions options = new DistributedCacheEntryOptions()
                .SetAbsoluteExpiration(DateTime.Now.AddMinutes(5))
                .SetSlidingExpiration(TimeSpan.FromMinutes(3));

            // Add the data into the cache
            await _cache.SetAsync(redisKey, resultDatabaseString);



            ///await _cache.SetObjectAsync("Postcode:" + redisKey, result);

            return Ok(new { LoadedFromRedis = false, Data = ResponseAddress, options });


            //return Ok(cacheResult);

        }

        [HttpPost]
        [Route("/[controller]/[action]/")]
        [ProducesResponseType(typeof(List<ResponseProvince>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(string), StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ResponseProvince>> GetMasterProvince(RequestMasterProvince JsonRequest)
        {
            _logger.LogInformation("Requested: getMasterProvince | Start | ");
            _logger.LogInformation("Requested: getMasterProvince | Process | " + JsonRequest.Province);
            //_logger.LogInformation("Requested: GetAddressByPostcode | Process | " + JsonRequest.Postcode + " | " +  JsonConvert.SerializeObject(JsonRequest));
            _logger.LogInformation("Requested: getMasterProvince | Process | " + JsonRequest.Province + " | " + JsonSerializer.Serialize(JsonRequest));

            //cacheResult = await _cache.GetObjectAsync<ResponseAddressByPostcode>((JsonRequest.Postcode.ToString().Trim()));
            string redisKey = JsonRequest.Province.ToString().Trim() ;

            //byte[] dataFromRedis = await _cache.GetAsync("redisKey");

            var dataFromRedis = await _cache.GetAsync(redisKey);
            if (JsonRequest.ClearCache == true)
            {
                _cache.RemoveAsync(redisKey);
            }
            ///var dataFromRedis = await _cache.GetObjectAsync<ResponseAddressByPostcode>("Postcode:" + redisKey);

            List<ResponseProvince> ResponseAddress = new();

            //if ((dataFromRedis?.Count() ?? 0) > 0)
            if (dataFromRedis != null)
            {
                //var resultWithRedis = _AddressRepo.GetAddressByPostCode(JsonRequest.Postcode.ToString().Trim());
                //var cars = JsonSerializer.Deserialize<List<ResponseAddressByPostcode>>(carsString);

                var resultStringWithRedis = Encoding.UTF8.GetString(dataFromRedis);

                ResponseAddress = JsonSerializer.Deserialize<List<ResponseProvince>>(resultStringWithRedis);

                return Ok(new { LoadedFromRedis = true, Data = ResponseAddress });
            }
            var result = _AddressRepo.GetMasterProvince(JsonRequest.Province.ToString().Trim());

            //string cachedDataString = JsonSerializer.Serialize(result);
            var resultDatabaseString = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(result));
            ResponseAddress = JsonSerializer.Deserialize<List<ResponseProvince>>(resultDatabaseString);

            // Setting up the cache options
            DistributedCacheEntryOptions options = new DistributedCacheEntryOptions()
                .SetAbsoluteExpiration(DateTime.Now.AddMinutes(5))
                .SetSlidingExpiration(TimeSpan.FromMinutes(3));

            // Add the data into the cache
            await _cache.SetAsync(redisKey, resultDatabaseString);

            ///await _cache.SetObjectAsync("Postcode:" + redisKey, result);

            return Ok(new { LoadedFromRedis = false, Data = ResponseAddress, options });

        }

        [HttpPost]
        [Route("/[controller]/[action]/")]
        [ProducesResponseType(typeof(List<ResponseDistrict>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(string), StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ResponseDistrict>> GetMasterDistrict(RequestMasterDistrict JsonRequest) 
        {
            _logger.LogInformation("Requested: getMasterDistrict | Start | ");
            _logger.LogInformation("Requested: getMasterDistrict | Process | " + JsonRequest.District);
            //_logger.LogInformation("Requested: GetAddressByPostcode | Process | " + JsonRequest.Postcode + " | " +  JsonConvert.SerializeObject(JsonRequest));
            _logger.LogInformation("Requested: getMasterDistrict | Process | " + JsonRequest.District + " | " + JsonSerializer.Serialize(JsonRequest));

            //cacheResult = await _cache.GetObjectAsync<ResponseAddressByPostcode>((JsonRequest.Postcode.ToString().Trim()));
            string redisKey = JsonRequest.District.ToString().Trim();
            //if (JsonRequest.ClearCache == true)
            //{
            //    _cache.RemoveAsync(redisKey);
            //}

            //byte[] dataFromRedis = await _cache.GetAsync("redisKey");

            var dataFromRedis = await _cache.GetAsync(redisKey);
            ///var dataFromRedis = await _cache.GetObjectAsync<ResponseAddressByPostcode>("Postcode:" + redisKey);

            if (JsonRequest.ClearCache == true)
            {
                _cache.RemoveAsync(redisKey);
            }
            List<ResponseDistrict> ResponseAddress = new();

            //if ((dataFromRedis?.Count() ?? 0) > 0)
            if (dataFromRedis != null)
            {
                var resultStringWithRedis = Encoding.UTF8.GetString(dataFromRedis);

                ResponseAddress = JsonSerializer.Deserialize<List<ResponseDistrict>>(resultStringWithRedis);

                return Ok(new { LoadedFromRedis = true, Data = ResponseAddress });
            }
            var result = _AddressRepo.GetMasterDistrict(JsonRequest.District.ToString().Trim());
            string cachedDataString = JsonSerializer.Serialize(result);
            var resultDatabaseString = Encoding.UTF8.GetBytes(cachedDataString);
            ResponseAddress = JsonSerializer.Deserialize<List<ResponseDistrict>>(resultDatabaseString);
            // Setting up the cache options
            DistributedCacheEntryOptions options = new DistributedCacheEntryOptions()
                .SetAbsoluteExpiration(DateTime.Now.AddMinutes(5))
                .SetSlidingExpiration(TimeSpan.FromMinutes(3));

            // Add the data into the cache
            await _cache.SetAsync(redisKey, resultDatabaseString);
            return Ok(new { LoadedFromRedis = false, Data = ResponseAddress, options });

        }

        [HttpPost]
        [Route("/[controller]/[action]/")]
        [ProducesResponseType(typeof(List<ResponseSubDistrict>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(string), StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ResponseSubDistrict>> GetMasterSubDistrict(RequestMasterSubDistrict JsonRequest)
        {
            _logger.LogInformation("Requested: getMasterSubDistrict | Start | ");
            _logger.LogInformation("Requested: getMasterSubDistrict | Process | " + JsonRequest.SubDistrict);
            //_logger.LogInformation("Requested: GetAddressByPostcode | Process | " + JsonRequest.Postcode + " | " +  JsonConvert.SerializeObject(JsonRequest));
            _logger.LogInformation("Requested: getMasterSubDistrict | Process | " + JsonRequest.SubDistrict + " | " + JsonSerializer.Serialize(JsonRequest));

            //cacheResult = await _cache.GetObjectAsync<ResponseAddressByPostcode>((JsonRequest.Postcode.ToString().Trim()));
            string redisKey = JsonRequest.SubDistrict.ToString().Trim();
            //byte[] dataFromRedis = await _cache.GetAsync("redisKey");
            
            var dataFromRedis = await _cache.GetAsync(redisKey);
            ///var dataFromRedis = await _cache.GetObjectAsync<ResponseAddressByPostcode>("Postcode:" + redisKey);
            if (JsonRequest.ClearCache == true)
            {
                _cache.RemoveAsync(redisKey);
            }
            List<ResponseSubDistrict> ResponseAddress = new();

            //if ((dataFromRedis?.Count() ?? 0) > 0)
            if (dataFromRedis != null)
            {
                var resultStringWithRedis = Encoding.UTF8.GetString(dataFromRedis);

                ResponseAddress = JsonSerializer.Deserialize<List<ResponseSubDistrict>>(resultStringWithRedis);

                return Ok(new { LoadedFromRedis = true, Data = ResponseAddress });
            }
            var result = _AddressRepo.GetMasterSubDistrict(JsonRequest.SubDistrict.ToString().Trim());
            string cachedDataString = JsonSerializer.Serialize(result);
            var resultDatabaseString = Encoding.UTF8.GetBytes(cachedDataString);
            ResponseAddress = JsonSerializer.Deserialize<List<ResponseSubDistrict>>(resultDatabaseString);
            // Setting up the cache options
            DistributedCacheEntryOptions options = new DistributedCacheEntryOptions()
                .SetAbsoluteExpiration(DateTime.Now.AddMinutes(5))
                .SetSlidingExpiration(TimeSpan.FromMinutes(3));

            // Add the data into the cache
            await _cache.SetAsync(redisKey, resultDatabaseString);
            return Ok(new { LoadedFromRedis = false, Data = ResponseAddress, options });

        }

        [HttpPost]
        [Route("/[controller]/[action]/")]
        [ProducesResponseType(typeof(List<ResponseRoad>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(string), StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ResponseRoad>> GetMasterRoad(RequestRoad JsonRequest)
        {
            _logger.LogInformation("Requested: getMasterRoad | Start | ");
            _logger.LogInformation("Requested: getMasterRoad | Process | " + JsonRequest.Road);
            //_logger.LogInformation("Requested: GetAddressByPostcode | Process | " + JsonRequest.Postcode + " | " +  JsonConvert.SerializeObject(JsonRequest));
            _logger.LogInformation("Requested: getMasterRoad | Process | " + JsonRequest.Road + " | " + JsonSerializer.Serialize(JsonRequest));

            //cacheResult = await _cache.GetObjectAsync<ResponseAddressByPostcode>((JsonRequest.Postcode.ToString().Trim()));
            string redisKey = JsonRequest.Road.ToString().Trim();
            //byte[] dataFromRedis = await _cache.GetAsync("redisKey");
            
            var dataFromRedis = await _cache.GetAsync(redisKey);
            ///var dataFromRedis = await _cache.GetObjectAsync<ResponseAddressByPostcode>("Postcode:" + redisKey);
            if (JsonRequest.ClearCache == true)
            {
                _cache.RemoveAsync(redisKey);
            }
            List<ResponseRoad> responseRoads = new();

            //if ((dataFromRedis?.Count() ?? 0) > 0)
            if (dataFromRedis != null)
            {
                var resultStringWithRedis = Encoding.UTF8.GetString(dataFromRedis);

                responseRoads = JsonSerializer.Deserialize<List<ResponseRoad>>(resultStringWithRedis);

                return Ok(new { LoadedFromRedis = true, Data = responseRoads });
            }
            var result = _AddressRepo.GetMasterRoad(JsonRequest.Road.ToString().Trim());
            string cachedDataString = JsonSerializer.Serialize(result);
            var resultDatabaseString = Encoding.UTF8.GetBytes(cachedDataString);
            responseRoads = JsonSerializer.Deserialize<List<ResponseRoad>>(resultDatabaseString);

            // Setting up the cache options
            DistributedCacheEntryOptions options = new DistributedCacheEntryOptions()
                .SetAbsoluteExpiration(DateTime.Now.AddMinutes(5))
                .SetSlidingExpiration(TimeSpan.FromMinutes(3));

            // Add the data into the cache
            await _cache.SetAsync(redisKey, resultDatabaseString);
            return Ok(new { LoadedFromRedis = false, Data = responseRoads, options });

        }

        // POST: api/RequestOrder
        [HttpPost]
        [Route("/[controller]/[action]/")]
        [ProducesResponseType(typeof(List<ResponseLane>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(string), StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        //public async Task<ActionResult<ResponseOrder>> RequestOrder(RequestOrder JsonRequestOrder)
        public async Task<ActionResult<ResponseLane>> GetMasterLane(RequestMasterLane JsonRequest)
        {
            _logger.LogInformation("Requested: GetAddress | Start | ");
            _logger.LogInformation("Requested: GetAddress | Process | " + JsonRequest.Lane);
            _logger.LogInformation("Requested: GetAddress | Process | " + JsonRequest.Lane + " | " + JsonSerializer.Serialize(JsonRequest));

            string redisKey = JsonRequest.Lane.ToString().Trim();
            
            var dataFromRedis = await _cache.GetAsync(redisKey);
            if (JsonRequest.ClearCache == true)
            {
                _cache.Remove(redisKey);
            }
            List<ResponseLane> ResponseAddress = new();

            if (dataFromRedis != null)
            {
                var resultStringWithRedis = Encoding.UTF8.GetString(dataFromRedis);

                ResponseAddress = JsonSerializer.Deserialize<List<ResponseLane>>(resultStringWithRedis);

                return Ok(new { LoadedFromRedis = true, Data = ResponseAddress });
            }


            var result = _AddressRepo.GetMasterLane(JsonRequest.Lane.ToString().Trim());
            /*string cachedDataString = JsonSerializer.Serialize(result);*/
            var resultDatabaseString = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(result));
            ResponseAddress = JsonSerializer.Deserialize<List<ResponseLane>>(resultDatabaseString);


            // Setting up the cache options
            DistributedCacheEntryOptions options = new DistributedCacheEntryOptions()
                .SetAbsoluteExpiration(DateTime.Now.AddMinutes(5))
                .SetSlidingExpiration(TimeSpan.FromMinutes(3));

            await _cache.SetAsync(redisKey, resultDatabaseString);

            return Ok(new { LoadedFromRedis = false, Data = ResponseAddress, options });
        }

        // POST: api/RequestOrder
        [HttpPost]
        [Route("/[controller]/[action]/")]
        [ProducesResponseType(typeof(List<ResponseAlley>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(string), StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        //public async Task<ActionResult<ResponseOrder>> RequestOrder(RequestOrder JsonRequestOrder)
        public async Task<ActionResult<ResponseAlley>> GetMasterAlley(RequestMasterAlley JsonRequest)
        {
            _logger.LogInformation("Requested: GetAddress | Start | ");
            _logger.LogInformation("Requested: GetAddress | Process | " + JsonRequest.Alley);
            _logger.LogInformation("Requested: GetAddress | Process | " + JsonRequest.Alley + " | " + JsonSerializer.Serialize(JsonRequest));

            string redisKey = JsonRequest.Alley.ToString().Trim();
            
            var dataFromRedis = await _cache.GetAsync(redisKey);
            if (JsonRequest.ClearCache == true)
            {
                _cache.Remove(redisKey);
            }
            List<ResponseAlley> ResponseAddress = new();

            if (dataFromRedis != null)
            {
                var resultStringWithRedis = Encoding.UTF8.GetString(dataFromRedis);

                ResponseAddress = JsonSerializer.Deserialize<List<ResponseAlley>>(resultStringWithRedis);

                return Ok(new { LoadedFromRedis = true, Data = ResponseAddress });
            }


            var result = _AddressRepo.GetMasterAlley(JsonRequest.Alley.ToString().Trim());
            /*string cachedDataString = JsonSerializer.Serialize(result);*/
            var resultDatabaseString = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(result));
            ResponseAddress = JsonSerializer.Deserialize<List<ResponseAlley>>(resultDatabaseString);


            // Setting up the cache options
            DistributedCacheEntryOptions options = new DistributedCacheEntryOptions()
                .SetAbsoluteExpiration(DateTime.Now.AddMinutes(5))
                .SetSlidingExpiration(TimeSpan.FromMinutes(3));

            await _cache.SetAsync(redisKey, resultDatabaseString);

            return Ok(new { LoadedFromRedis = false, Data = ResponseAddress, options });
        }

        [HttpPost]
        [Route("/[controller]/[action]/")]
        [ProducesResponseType(typeof(List<ResponsePersonDataByFirstName>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(string), StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ResponsePersonDataByFirstName>> GetPersonDataByFirstName(RequestPersonDataByFirstName JsonRequest)
        {
            _logger.LogInformation("Requested: getPersonDataByFirstName | Start | ");
            _logger.LogInformation("Requested: getPersonDataByFirstName | Process | " + JsonRequest.PersonName);
            //_logger.LogInformation("Requested: GetAddressByPostcode | Process | " + JsonRequest.Postcode + " | " +  JsonConvert.SerializeObject(JsonRequest));
            _logger.LogInformation("Requested: getPersonDataByFirstName | Process | " + JsonRequest.PersonName + " | " + JsonSerializer.Serialize(JsonRequest));

            // split string 
            /*string[] splitName = JsonRequest.PersonName.ToString().Split(" ");*/

            //cacheResult = await _cache.GetObjectAsync<ResponseAddressByPostcode>((JsonRequest.Postcode.ToString().Trim()));

            string redisKey = Regex.Replace(JsonRequest.PersonName, @"\s+", " ");
            //byte[] dataFromRedis = await _cache.GetAsync("redisKey");
            
            var dataFromRedis = await _cache.GetAsync(redisKey);
            ///var dataFromRedis = await _cache.GetObjectAsync<ResponseAddressByPostcode>("Postcode:" + redisKey);
            if (JsonRequest.ClearCache == true)
            {
                _cache.RemoveAsync(redisKey);
            }
            List<ResponsePersonDataByFirstName> ResponsePersonDataByFirstName = new();

            //if ((dataFromRedis?.Count() ?? 0) > 0)
            if (dataFromRedis != null)
            {
                var resultStringWithRedis = Encoding.UTF8.GetString(dataFromRedis);

                ResponsePersonDataByFirstName = JsonSerializer.Deserialize<List<ResponsePersonDataByFirstName>>(resultStringWithRedis);

                return Ok(new { LoadedFromRedis = true, Data = ResponsePersonDataByFirstName });
            }
            var result = _AddressRepo.GetResponsePersonDataByFirstNames(JsonRequest.PersonName.ToString().Trim());
            string cachedDataString = JsonSerializer.Serialize(result);
            var resultDatabaseString = Encoding.UTF8.GetBytes(cachedDataString);
            ResponsePersonDataByFirstName = JsonSerializer.Deserialize<List<ResponsePersonDataByFirstName>>(resultDatabaseString);

            // Setting up the cache options
            DistributedCacheEntryOptions options = new DistributedCacheEntryOptions()
                .SetAbsoluteExpiration(DateTime.Now.AddMinutes(5))
                .SetSlidingExpiration(TimeSpan.FromMinutes(3));

            // Add the data into the cache
            await _cache.SetAsync(redisKey, resultDatabaseString);
            return Ok(new { LoadedFromRedis = false, Data = ResponsePersonDataByFirstName, options });

        }

        [HttpPost]
        [Route("/[controller]/[action]/")]
        [ProducesResponseType(typeof(List<ResponsePersonDataByPhone>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(string), StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ResponsePersonDataByPhone>> GetPersonDataByPhone(RequestPersonDataByPhone JsonRequest)
        {
            _logger.LogInformation("Requested: getPersonDataByPhone | Start | ");
            _logger.LogInformation("Requested: getPersonDataByPhone | Process | " + JsonRequest.PhoneNumber);
            //_logger.LogInformation("Requested: GetAddressByPostcode | Process | " + JsonRequest.Postcode + " | " +  JsonConvert.SerializeObject(JsonRequest));
            _logger.LogInformation("Requested: getPersonDataByPhone | Process | " + JsonRequest.PhoneNumber + " | " + JsonSerializer.Serialize(JsonRequest));

            //cacheResult = await _cache.GetObjectAsync<ResponseAddressByPostcode>((JsonRequest.Postcode.ToString().Trim()));
            string redisKey = JsonRequest.PhoneNumber.ToString().Trim();
            //byte[] dataFromRedis = await _cache.GetAsync("redisKey");
            
            var dataFromRedis = await _cache.GetAsync(redisKey);
            ///var dataFromRedis = await _cache.GetObjectAsync<ResponseAddressByPostcode>("Postcode:" + redisKey);
            if (JsonRequest.ClearCache == true)
            {
                _cache.RemoveAsync(redisKey);
            }
            List<ResponsePersonDataByPhone> responsePersonDataByPhone = new();

            //if ((dataFromRedis?.Count() ?? 0) > 0)
            if (dataFromRedis != null)
            {
                var resultStringWithRedis = Encoding.UTF8.GetString(dataFromRedis);

                responsePersonDataByPhone = JsonSerializer.Deserialize<List<ResponsePersonDataByPhone>>(resultStringWithRedis);

                return Ok(new { LoadedFromRedis = true, Data = responsePersonDataByPhone });
            }
            var result = _AddressRepo.GetResponsePersonDataByPhones(JsonRequest.PhoneNumber.ToString().Trim());
            string cachedDataString = JsonSerializer.Serialize(result);
            var resultDatabaseString = Encoding.UTF8.GetBytes(cachedDataString);
            responsePersonDataByPhone = JsonSerializer.Deserialize<List<ResponsePersonDataByPhone>>(resultDatabaseString);

            // Setting up the cache options
            DistributedCacheEntryOptions options = new DistributedCacheEntryOptions()
                .SetAbsoluteExpiration(DateTime.Now.AddMinutes(5))
                .SetSlidingExpiration(TimeSpan.FromMinutes(3));

            // Add the data into the cache
            await _cache.SetAsync(redisKey, resultDatabaseString);
            return Ok(new { LoadedFromRedis = false, Data = responsePersonDataByPhone, options });

        }


        [HttpPost]
        [Route("/[controller]/[action]/")]
        [ProducesResponseType(typeof(List<ResponseAddressByPostcode>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(string), StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ResponseAddressByPostcode>> GetAddressByPostcodeWithPerson(RequestAddressByPostcodeWithPerson JsonRequest)
        {
            _logger.LogInformation("Requested: GetAddressByPostcode | Start | ");
            _logger.LogInformation("Requested: GetAddressByPostcode | Process | " + JsonRequest.Firstname + "|" + JsonRequest.Surname + "|" + JsonRequest.Postcode);
            //_logger.LogInformation("Requested: GetAddressByPostcode | Process | " + JsonRequest.Postcode + " | " +  JsonConvert.SerializeObject(JsonRequest));
            _logger.LogInformation("Requested: GetAddressByPostcode | Process | " + JsonRequest.Firstname + "|" + JsonRequest.Surname + "|" + JsonRequest.Postcode + " | " + JsonSerializer.Serialize(JsonRequest));


            //cacheResult = await _cache.GetObjectAsync<ResponseAddressByPostcode>((JsonRequest.Postcode.ToString().Trim()));
            string redisKey = JsonRequest.Firstname.ToString().Trim() + JsonRequest.Surname.ToString().Trim() + JsonRequest.Postcode.ToString().Trim();

            //byte[] dataFromRedis = await _cache.GetAsync("redisKey");

            var dataFromRedis = await _cache.GetAsync(redisKey);
            if (JsonRequest.ClearCache == true)
            {
                _cache.RemoveAsync(redisKey);
            }
            ///var dataFromRedis = await _cache.GetObjectAsync<ResponseAddressByPostcode>("Postcode:" + redisKey);

            List<ResponseAddressByPostcode> ResponseAddress = new();

            //if ((dataFromRedis?.Count() ?? 0) > 0)
            if (dataFromRedis != null)
            {
                //var resultWithRedis = _AddressRepo.GetAddressByPostCode(JsonRequest.Postcode.ToString().Trim());
                //var cars = JsonSerializer.Deserialize<List<ResponseAddressByPostcode>>(carsString);


                var resultStringWithRedis = Encoding.UTF8.GetString(dataFromRedis);

                ResponseAddress = JsonSerializer.Deserialize<List<ResponseAddressByPostcode>>(resultStringWithRedis);

                return Ok(new { LoadedFromRedis = true, Data = ResponseAddress });
            }
            var result = _AddressRepo.GetAddressByPostcodeWithPerson(JsonRequest.Firstname.ToString().Trim(), JsonRequest.Surname.ToString().Trim(), JsonRequest.Postcode.ToString().Trim());


            string cachedDataString = JsonSerializer.Serialize(result);
            var resultDatabaseString = Encoding.UTF8.GetBytes(cachedDataString);
            ResponseAddress = JsonSerializer.Deserialize<List<ResponseAddressByPostcode>>(resultDatabaseString);

            // Setting up the cache options
            DistributedCacheEntryOptions options = new DistributedCacheEntryOptions()
                .SetAbsoluteExpiration(DateTime.Now.AddMinutes(5))
                .SetSlidingExpiration(TimeSpan.FromMinutes(3));

            // Add the data into the cache
            await _cache.SetAsync(redisKey, resultDatabaseString);



            ///await _cache.SetObjectAsync("Postcode:" + redisKey, result);

            return Ok(new { LoadedFromRedis = false, Data = ResponseAddress, options });


            //return Ok(cacheResult);

        }

        [HttpPost]
        [Route("/[controller]/[action]/")]
        [ProducesResponseType(typeof(List<ResponseAddressByPostcode>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(string), StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ResponseAddressByPostcode>> GetAddressByPostcodeHnoAndVillage(RequestAddressByPostcodeHnoAndVillage JsonRequest)
        {
            _logger.LogInformation("Requested: RequestAddressPostcodeHnoAndVillage | Start | ");
            _logger.LogInformation("Requested: RequestAddressPostcodeHnoAndVillage | Process | " + JsonRequest.Postcode + "|" + JsonRequest.Hno + "|" + JsonRequest.Village);
            _logger.LogInformation("Requested: RequestAddressPostcodeHnoAndVillage | Process | " + JsonRequest.Postcode + "|" + JsonRequest.Hno + "|" + JsonRequest.Village + " | " + JsonSerializer.Serialize(JsonRequest));


            //cacheResult = await _cache.GetObjectAsync<ResponseAddressByPostcode>((JsonRequest.Postcode.ToString().Trim()));
            string redisKey = JsonRequest.Postcode.ToString().Trim() + JsonRequest.Hno.ToString().Trim() + JsonRequest.Village.ToString().Trim();

            //byte[] dataFromRedis = await _cache.GetAsync("redisKey");

            var dataFromRedis = await _cache.GetAsync(redisKey);
            if (JsonRequest.ClearCache == true)
            {
                _cache.RemoveAsync(redisKey);
            }
            ///var dataFromRedis = await _cache.GetObjectAsync<ResponseAddressByPostcode>("Postcode:" + redisKey);

            List<ResponseAddressByPostcode> ResponseAddress = new();

            //if ((dataFromRedis?.Count() ?? 0) > 0)
            if (dataFromRedis != null)
            {
                //var resultWithRedis = _AddressRepo.GetAddressByPostCode(JsonRequest.Postcode.ToString().Trim());
                //var cars = JsonSerializer.Deserialize<List<ResponseAddressByPostcode>>(carsString);


                var resultStringWithRedis = Encoding.UTF8.GetString(dataFromRedis);

                ResponseAddress = JsonSerializer.Deserialize<List<ResponseAddressByPostcode>>(resultStringWithRedis);

                return Ok(new { LoadedFromRedis = true, Data = ResponseAddress });
            }
            var result = _AddressRepo.GetAddressByPostcodeHnoAndVillage(JsonRequest.Postcode.ToString().Trim(), JsonRequest.Hno.ToString().Trim(), JsonRequest.Village.ToString().Trim());


            string cachedDataString = JsonSerializer.Serialize(result);
            var resultDatabaseString = Encoding.UTF8.GetBytes(cachedDataString);
            ResponseAddress = JsonSerializer.Deserialize<List<ResponseAddressByPostcode>>(resultDatabaseString);

            // Setting up the cache options
            DistributedCacheEntryOptions options = new DistributedCacheEntryOptions()
                .SetAbsoluteExpiration(DateTime.Now.AddMinutes(5))
                .SetSlidingExpiration(TimeSpan.FromMinutes(3));

            // Add the data into the cache
            await _cache.SetAsync(redisKey, resultDatabaseString);



            ///await _cache.SetObjectAsync("Postcode:" + redisKey, result);

            return Ok(new { LoadedFromRedis = false, Data = ResponseAddress, options });


            //return Ok(cacheResult);

        }



        //********* INSERT DATA ************//
        //*********************************//

        [HttpPost]
        [Route("/[controller]/[action]/")]
        [ProducesResponseType(typeof(List<ResponseAddPersonData>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(string), StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ResponseAddPersonData>> AddPersonData(AddPersonDataModel JsonRequest)
        {
            try {
                string fname = JsonRequest.FirstnameTH.Trim();
                string surname = JsonRequest.SurnameTH.Trim();
                if (JsonRequest.FirstnameTH == "") 
                {
                    fname = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(JsonRequest.FirstnameEN.Trim());
                    surname = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(JsonRequest.SurnameEN.Trim());
                }
                var checkPerson = _AddressRepo.CheckPersonDataByFirstName(fname, surname).ToList();
                if (checkPerson.Count() > 0)
                {
                    var phoneData = _AddressRepo.AddPhoneNumber(JsonRequest.UserID,checkPerson[0].PersonID, JsonRequest.PhoneNumber.Trim());
                    var responsePersonData = new ResponseAddPersonData {
                        UserID = JsonRequest.UserID,
                        PersonID = phoneData.PersonID,
                        TitleTH = JsonRequest.TitleTH.ToString().Trim(),
                        FirstNameTH = JsonRequest.FirstnameTH.ToString().Trim(),
                        SurnameTH = JsonRequest.SurnameTH.ToString().Trim(),
                        TitleEN = JsonRequest.TitleEN.ToString().Trim(),
                        FirstNameEN = JsonRequest.FirstnameEN.ToString().Trim(),
                        SurnameEN = JsonRequest.SurnameEN.ToString().Trim(),
                        PhoneID = phoneData.PhoneID,
                        PhoneNumber = JsonRequest.PhoneNumber.ToString().Trim()
                    };
                    return responsePersonData;
                }
                var result = _AddressRepo.AddPersonData(JsonRequest);
                return Ok(new { data = result });
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        [HttpPost]
        [Route("/[controller]/[action]/")]
        [ProducesResponseType(typeof(List<ResponseAddPersonData>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(string), StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ResponseAddPersonData>> EditPersonData(RequestEditPersonData JsonRequest) 
        {
            try
            {
                _logger.LogInformation("Requested: EditPersonData | Start | ");
                _logger.LogInformation("Requested: EditPersonData | Process | " + JsonSerializer.Serialize(JsonRequest));

                var result = _AddressRepo.EditPersonData(JsonRequest);
                return Ok(new { data = result });
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        [HttpPost]
        [Route("/[controller]/[action]/")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(string), StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<RequestAddPersonHID>> AddPersonHID(RequestAddPersonHID JsonRequest) 
        {
            try
            {
                _logger.LogInformation("Requested: AddPersonHID | Start | ");
                _logger.LogInformation("Requested: AddPersonHID | Process | " + JsonRequest.UserID + " | " + JsonRequest.PersonID + " | " + JsonRequest.PhoneID);
                _logger.LogInformation("Requested: AddPersonHID | Process | " + JsonRequest.UserID + " | " + JsonRequest.PersonID + " | " + JsonRequest.PhoneID + " | " + JsonSerializer.Serialize(JsonRequest));

                var result = _AddressRepo.AddPersonHID(JsonRequest);
                return Ok(new { data = result });
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        [HttpPost]
        [Route("/[controller]/[action]/")]
        [ProducesResponseType(typeof(List<ResponseUpdateStatisticsHID>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(string), StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ResponseUpdateStatisticsHID>> UpdateStatisticsHID(RequestUpdateStatisticsHID JsonRequest)
        {
            try
            {
                _logger.LogInformation("Requested: UpdateStatisticsHID | Start | ");
                _logger.LogInformation("Requested: UpdateStatisticsHID | Process | " + JsonRequest.PersonID + " | " + JsonRequest.Hid);
                _logger.LogInformation("Requested: UpdateStatisticsHID | Process | " + JsonRequest.PersonID + " | " + JsonSerializer.Serialize(JsonRequest));

                var result = _AddressRepo.UpdateStatisticsHID(Convert.ToInt64(JsonRequest.PersonID),Convert.ToInt64(JsonRequest.Hid));
                return Ok(new { data = result });
            }
            catch (Exception ex)
            {
                return null;
            }
        }

    }

}

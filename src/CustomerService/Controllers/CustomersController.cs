using CustomerService.Contracts;
using CustomerService.Contracts.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace CustomerService.Controllers
{

    /// <summary>
    /// The customer service enpoint
    /// </summary>
    [Route("customer")]
    [ApiController]
    public class CustomersController : ControllerBase
    {

        private readonly ICustomersService _customerService;

        /// <summary>
        /// The constructor
        /// </summary>
        /// <param name="customerService"></param>
        public CustomersController(ICustomersService customerService)
        {
            _customerService = customerService;
        }

        /// <summary>
        /// Update customer score
        /// </summary>
        /// <param name="customerid">customer id</param>
        /// <param name="score">score</param>
        /// <returns>Current score after update</returns>
        [HttpPost("{customerid}/score/{score}")]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        public async Task<int> UpdateAsync([FromRoute] long customerid, [FromRoute][Range(-1000, 1000)] int score)
        {
            return await _customerService.CreateOrUpdateAsync(customerid, score);
        }

        /// <summary>
        /// Get customer score by id (This is a method for testing)
        /// </summary>
        /// <param name="customerid">customer id</param>
        /// <returns>Current score</returns>
        [HttpGet("{customerid}")]
        [ProducesResponseType(typeof(CustomerDto), StatusCodes.Status200OK)]
        public async Task<int> GetAsync([FromRoute] long customerid)
        {
            return await _customerService.GetScoreByIdAsync(customerid);
        }
    }
}

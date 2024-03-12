using CustomerService.Contracts;
using CustomerService.Contracts.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace CustomerService.Controllers
{

    ///  <summary>
    /// The leader board service enpoint
    /// </summary>
    [Route("leaderboard")]
    [ApiController]
    public class LeaderBoardsController : ControllerBase
    {
        private readonly ICustomersService _customerService;

        /// <summary>
        /// The constructor
        /// </summary>
        /// <param name="customerService"></param>
        public LeaderBoardsController(ICustomersService customerService)
        {
            _customerService = customerService;
        }

        /// <summary>
        /// Get customers by rank
        /// </summary>
        /// <param name="start">start rank, included in response if exists</param>
        /// <param name="end">end rank, included in response if exists</param>
        /// <returns>All eligible customers</returns>
        [HttpGet("")]
        [ProducesResponseType(typeof(IEnumerable<CustomerDto>), StatusCodes.Status200OK)]
        public async Task<IEnumerable<CustomerDto>> GetByRanksAsync([FromQuery, Range(1, int.MaxValue)] int start, [FromQuery, Range(1, int.MaxValue)] int end)
        {
            return await _customerService.GetByRanksAsync(start, end, CancellationToken.None);
        }

        /// <summary>
        /// Get customers by customerid
        /// </summary>
        /// <param name="customerid">customer id</param>
        /// <param name="high">number of neighbors whose rank is higher than the specified customer.</param>
        /// <param name="low">number of neighbors whose rank is lower than the specified customer.</param>
        /// <returns>All eligible customers</returns>
        [HttpGet("{customerid}")]
        [ProducesResponseType(typeof(IEnumerable<CustomerDto>), StatusCodes.Status200OK)]
        public async Task<IEnumerable<CustomerDto>> GetByIdAsync([FromRoute] long customerid, [FromQuery, DefaultValue(0), Range(0, int.MaxValue)] int high, [FromQuery, DefaultValue(0), Range(0, int.MaxValue)] int low)
        {
            return await _customerService.GetByIdAndNeighborsAsync(customerid, high, low, CancellationToken.None);
        }
    }
}

using CustomerService.Contracts.Dtos;
using Microsoft.Extensions.Logging;

namespace CustomerService.Contracts
{

    /// <summary>
    /// The Customer data service
    /// </summary>
    public interface ICustomersService
    {
        /// <summary>
        /// Create or update customer
        /// </summary>
        /// <param name="id">the customer id</param>
        /// <param name="score">the score</param>
        /// <returns>the newest score</returns>
        Task<int> CreateOrUpdateAsync(int id, int score);
        /// <summary>
        /// Get customers by rank range
        /// </summary>
        /// <param name="start">start rank</param>
        /// <param name="end">end rank</param>
        /// <returns>customer data set</returns>
        Task<IEnumerable<CustomerDto>> GetByRanksAsync(int start, int end);
        /// <summary>
        /// Get customer by id and load the neighbors
        /// </summary>
        /// <param name="id">customer id</param>
        /// <param name="high">number of neighbors whose rank is higher than the customer.</param>
        /// <param name="low">number of neighbors whose rank is lower than the customer.</param>
        /// <returns></returns>
        Task<IEnumerable<CustomerDto>> GetByIdAndNeighborsAsync(int id, int high, int low);
    }
}

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
        /// <param name="cancellationToken">cancellationToken</param>
        /// <returns>the newest score</returns>
        Task<int> CreateOrUpdateAsync(long id, int score, CancellationToken cancellationToken);
        /// <summary>
        /// Get customers by rank range
        /// </summary>
        /// <param name="start">start rank</param>
        /// <param name="end">end rank</param>
        /// <param name="cancellationToken">cancellationToken</param>
        /// <returns>customer data set</returns>
        Task<List<CustomerDto>> GetByRanksAsync(int start, int end, CancellationToken cancellationToken);
        /// <summary>
        /// Get customer by id and load the neighbors
        /// </summary>
        /// <param name="id">customer id</param>
        /// <param name="high">number of neighbors whose rank is higher than the customer.</param>
        /// <param name="low">number of neighbors whose rank is lower than the customer.</param>
        /// <param name="cancellationToken">cancellationToken</param>
        /// <returns></returns>
        Task<List<CustomerDto>> GetByIdAndNeighborsAsync(long id, int high, int low, CancellationToken cancellationToken);

        /// <summary>
        /// Get Customer score customers data set(this is a method just for test)
        /// </summary>
        /// <param name="id">the customer id</param>
        /// <param name="cancellationToken">cancellationToken</param>
        /// <returns>Matched customer</returns>
        Task<int> GetScoreByIdAsync(long id, CancellationToken cancellationToken);

        /// <summary>
        /// Get the max rank of leader board(this is a method just for test)
        /// </summary>
        /// <param name="cancellationToken">cancellationToken</param>
        /// <returns>Max rank no</returns>
        Task<int> GetMaxRankAsync(CancellationToken cancellationToken);
    }
}

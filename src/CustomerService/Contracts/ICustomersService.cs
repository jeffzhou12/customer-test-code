using CustomerService.Contracts.Dtos;

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
    }
}

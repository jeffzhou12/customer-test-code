using CustomerService.Contracts;
using CustomerService.Contracts.Dtos;
using System.Collections.Concurrent;
using System.ComponentModel;

namespace CustomerService.Services
{

    /// <summary>
    /// The customer service interface implementation
    /// </summary>
    public class CustomersService : ICustomersService
    {


        /*
         * Some changes in v2:
         * 1.Remove the useless Task.Run
         * 2.Use thread-safe data structures
         * 3.Asynchronously update rankings to improve concurrent performance
         * 4.Add a cache of max rank to optimize query performance
         * 5.Fixed data update atomicity issue
         * */

        // Customer score collection
        private static ConcurrentDictionary<long, int> customerData = new ConcurrentDictionary<long, int>();
        // Customer leaderboard collection
        private static ConcurrentBag<CustomerDto> leaderboardData = new ConcurrentBag<CustomerDto>();
        //cache the max rank for query speed
        private int _maxRank = 0;

        /// <summary>
        /// Create or update customer
        /// </summary>
        /// <param name="id">the customer id</param>
        /// <param name="score">the score</param>
        /// <param name="cancellationToken">cancellationToken</param>
        /// <returns>the newest score</returns>
        public async Task<int> CreateOrUpdateAsync(long id, int score, CancellationToken cancellationToken)
        {
            var newScore = 0;
            try
            {
                var currentScore = 0;
                // Check if customer exists
                if (customerData.TryGetValue(id, out currentScore))
                {
                    // Update existing customer
                    newScore = currentScore + score;
                    customerData[id] = newScore;
                }
                else
                {
                    // Add to customer data
                    newScore = score;
                    customerData.TryAdd(id, score);
                }
                // Update ranks asynchronously
                await UpdateLeaderboardAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while updating customer score.", ex);
            }

            return newScore;
        }

        /// <summary>
        /// Updates the leaderboard asynchronously by sorting the customer data
        /// </summary>
        /// <param name="cancellationToken">cancellationToken</param>
        /// <returns>a Task representing the asynchronous operation</returns>
        private async Task UpdateLeaderboardAsync(CancellationToken cancellationToken)
        {
            var sort_data = await Task.Run(() =>
            {
                return customerData.Where(it => it.Value > 0)
                                           .OrderBy(it => it.Value)
                                           .ThenByDescending(it => it.Key)
                                           .Select(it => new CustomerDto()
                                           {
                                               CustomerId = it.Key,
                                               Score = it.Value
                                           })
                                           .ToList();
            }, cancellationToken);

            // Assign ranks
            var rank = sort_data.Count;
            _maxRank = rank;
            Parallel.ForEach(sort_data, item =>
            {
                item.Rank = rank--;
            });
            // Set leaderboard
            ConcurrentBag<CustomerDto> oldLeaderboardData = leaderboardData;
            ConcurrentBag<CustomerDto> newLeaderboardData = new ConcurrentBag<CustomerDto>(sort_data);
            leaderboardData = newLeaderboardData;
            if (!oldLeaderboardData.IsEmpty)
            {
                oldLeaderboardData.Clear();
            }

        }

        /// <summary>
        /// Get customers by rank range
        /// </summary>
        /// <param name="start">start rank</param>
        /// <param name="end">end rank</param>
        /// <param name="cancellationToken">cancellationToken</param>
        /// <returns>customer data set</returns>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="Exception"></exception>
        public async Task<List<CustomerDto>> GetByRanksAsync(int start, int end, CancellationToken cancellationToken)
        {
            List<CustomerDto> result = new List<CustomerDto>();
            try
            {
                if (leaderboardData is null)
                {
                    throw new InvalidOperationException("The leaderboard data is null.");
                }
                if (start == 0 && end == 0)
                {
                    return Enumerable.Empty<CustomerDto>().ToList();
                }
                else if (start > 0 && end == 0)
                {
                    result = leaderboardData.Where(it => it.Rank >= start).ToList();
                }
                else if (start == 0 && end > 0)
                {
                    result = leaderboardData.Where(it => it.Rank <= end).ToList();
                }
                else if (start <= end)
                {
                    result = leaderboardData.Where(it => it.Rank >= start && it.Rank <= end).ToList();
                }
                else
                {
                    throw new ArgumentOutOfRangeException("The start index must be less than or equal to the end index.");
                }
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while retrieving customer ranks.", ex);
            }

            return result;
        }

        /// <summary>
        /// Get customer by id and load the neighbors
        /// </summary>
        /// <param name="id">customer id</param>
        /// <param name="high">number of neighbors whose rank is higher than the customer.</param>
        /// <param name="low">number of neighbors whose rank is lower than the customer.</param>
        /// <param name="cancellationToken">cancellationToken</param>
        /// <returns>customer data set</returns>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="Exception"></exception>
        public async Task<List<CustomerDto>> GetByIdAndNeighborsAsync(long id, int high, int low, CancellationToken cancellationToken)
        {

            List<CustomerDto> result = new List<CustomerDto>();
            try
            {
                if (leaderboardData is null)
                {
                    throw new InvalidOperationException("The leaderboard data is null.");
                }

                CustomerDto currentCustomer = leaderboardData.FirstOrDefault(customer => customer.CustomerId == id);

                if (currentCustomer is not null)
                {
                    if (_maxRank == 0)
                    {
                        _maxRank = leaderboardData.Max(customer => customer.Rank);
                    }
                    int maxRank = _maxRank;
                    int currentRank = currentCustomer.Rank;
                    int higherRankMinValue = Math.Max(1, currentRank - high);
                    int lowerRankMaxValue = Math.Min(maxRank, currentRank + low);

                    result = leaderboardData.Where(customer => customer.Rank >= higherRankMinValue && customer.Rank <= lowerRankMaxValue).ToList();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while retrieving customer ranks.", ex);
            }

            return result;
        }

        /// <summary>
        /// Get the current core by id , this method is just for test
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken">cancellationToken</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<int> GetScoreByIdAsync(long id, CancellationToken cancellationToken)
        {
            var score = 0;
            try
            {
                customerData.TryGetValue(id, out score);
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while updating customer score.", ex);
            }

            return score;
        }

        /// <summary>
        /// Get the max rank of leader board(this is a method just for test)
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<int> GetMaxRankAsync(CancellationToken cancellationToken)
        {
            if (_maxRank == 0)
            {
                _maxRank = leaderboardData.Max(customer => customer.Rank);
            }
            return _maxRank;
        }

    }
}

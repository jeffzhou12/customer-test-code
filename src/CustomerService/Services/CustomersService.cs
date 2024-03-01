using CustomerService.Contracts;
using CustomerService.Contracts.Dtos;
using System.Collections.Concurrent;
using static System.Formats.Asn1.AsnWriter;

namespace CustomerService.Services
{

    /// <summary>
    /// The customer service interface implementation
    /// </summary>
    public class CustomersService : ICustomersService
    {

        // Use a concurrent collection for thread safety
        private static ConcurrentDictionary<long, CustomerDto> customerData = new ConcurrentDictionary<long, CustomerDto>();
        // Use a concurrent collection for thread safety
        private static ConcurrentBag<CustomerDto> leaderboardData = new ConcurrentBag<CustomerDto>();

        /// <summary>
        /// Create or update customer
        /// </summary>
        /// <param name="id">the customer id</param>
        /// <param name="score">the score</param>
        /// <returns>the newest score</returns>
        public async Task<int> CreateOrUpdateAsync(long id, int score)
        {
            var finalScore = 0;
            try
            {
                // Check if customer exists
                if (customerData.TryGetValue(id, out CustomerDto customer))
                {
                    // Update existing customer
                    finalScore = customer.Score + score;
                    customer.Score = finalScore;
                }
                else
                {
                    // Create new customer
                    customer = new CustomerDto
                    {
                        CustomerId = id,
                        Score = score
                    };
                    finalScore = score;

                    // Add to customer data
                    customerData.TryAdd(id, customer);
                }

                // Update ranks asynchronously
                await UpdateLeaderboardAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while updating customer score.", ex);
            }

            return finalScore;
        }

        /// <summary>
        /// Updates the leaderboard asynchronously by sorting the customer data
        /// </summary>
        /// <returns>a Task representing the asynchronous operation</returns>
        private async Task UpdateLeaderboardAsync()
        {
            // Perform the sorting operation asynchronously
            var sort_data = await Task.Run(() =>
            {
                return customerData.Values.Where(it => it.Score > 0)
                                           .OrderBy(it => it.Score)
                                           .ThenByDescending(it => it.CustomerId)
                                           .ToList();
            });

            // Assign ranks
            var rank = sort_data.Count;
            foreach (var item in sort_data)
            {
                item.Rank = rank--;
            }

            // Set leaderboard
            leaderboardData = new ConcurrentBag<CustomerDto>(sort_data);
        }

        /// <summary>
        /// Get customers by rank range
        /// </summary>
        /// <param name="start">start rank</param>
        /// <param name="end">end rank</param>
        /// <returns>customer data set</returns>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="Exception"></exception>
        public async Task<List<CustomerDto>> GetByRanksAsync(int start, int end)
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
                    result = leaderboardData.ToList();
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
        /// <returns>customer data set</returns>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="Exception"></exception>
        public async Task<List<CustomerDto>> GetByIdAndNeighborsAsync(long id, int high, int low)
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
                    int maxRank = leaderboardData.Max(customer => customer.Rank);
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
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<int> GetScoreByIdAsync(long id)
        {
            var score = 0;
            try
            {
                if (customerData.TryGetValue(id, out CustomerDto customer))
                {
                    score = customer.Score;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while updating customer score.", ex);
            }

            return score;
        }

    }
}

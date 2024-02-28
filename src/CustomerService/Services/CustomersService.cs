using CustomerService.Contracts;
using CustomerService.Contracts.Dtos;
using System.Collections.Concurrent;

namespace CustomerService.Services
{

    /// <summary>
    /// The customer service interface implementation
    /// </summary>
    public class CustomersService : ICustomersService
    {

        //all customer data
        private static ConcurrentBag<CustomerDto> customerData = new ConcurrentBag<CustomerDto>();
        //customer data in leader board
        private static List<CustomerDto> leaderboardData = new List<CustomerDto>();

        /// <summary>
        /// Create or update customer
        /// </summary>
        /// <param name="id">the customer id</param>
        /// <param name="score">the score</param>
        /// <returns>the newest score</returns>
        public async Task<int> CreateOrUpdateAsync(int id, int score)
        {
            var finalScore = 0;
            await Task.Run(() =>
            {
                try
                {

                    var customer = customerData.FirstOrDefault(it => it.CustomerId == id);
                    if (customer is null)
                    {
                        customer = new CustomerDto
                        {
                            CustomerId = id,
                            Score = score
                        };
                        customerData.Add(customer);
                        finalScore = score;
                    }
                    else
                    {
                        finalScore = customer.Score + score;
                        customer.Score = finalScore; 
                    }
                    //reset the rank of customers by score and customer id
                    var sort_data = customerData.Where(it => it.Score > 0).OrderByDescending(it => it.Score).ThenBy(it => it.CustomerId).ToList();
                    var rank = 1;
                    var count = sort_data.Count;

                    for (int i = 0; i < count; i++)
                    {
                        sort_data[i].Rank = rank;
                        rank++;
                    }

                    //insert data to leader board
                    leaderboardData = sort_data.OrderBy(it => it.Rank).ThenBy(it => it.CustomerId).ToList();

                }
                catch (Exception ex)
                {
                    throw ex.InnerException ?? ex;
                }

            });

            return finalScore;
        }

        /// <summary>
        /// Get customers by rank range
        /// </summary>
        /// <param name="start">start rank</param>
        /// <param name="end">end rank</param>
        /// <returns>customer data set</returns>
        public async Task<IEnumerable<CustomerDto>> GetByRanksAsync(int start, int end)
        {
            var result = new List<CustomerDto>();
            await Task.Run(() =>
            {
                try
                {
                    //both empty with start and end
                    if (start == 0 && end == 0)
                    {
                        result = leaderboardData;
                    }
                    //only start
                    else if (start > 0 && end == 0)
                    {
                        result = leaderboardData.Where(it => it.Rank >= start).ToList();
                    }
                    //only end
                    else if (start == 0 && end > 0)
                    {
                        result = leaderboardData.Where(it => it.Rank <= end).ToList();
                    }
                    else if (start <= end)
                    {
                        result = leaderboardData.Where(it => it.Rank >= start && it.Rank <= end).ToList();
                    }
                }
                catch (Exception ex)
                {
                    throw ex.InnerException ?? ex;
                }
            });
            return result;
        }

        /// <summary>
        /// Get customer by id and load the neighbors
        /// </summary>
        /// <param name="id">customer id</param>
        /// <param name="high">number of neighbors whose rank is higher than the customer.</param>
        /// <param name="low">number of neighbors whose rank is lower than the customer.</param>
        /// <returns></returns>
        public async Task<IEnumerable<CustomerDto>> GetByIdAndNeighborsAsync(int id, int high, int low)
        {

            var result = new List<CustomerDto>();
            await Task.Run(() =>
            {
                try
                {
                    var current_customer = leaderboardData.FirstOrDefault(it => it.CustomerId == id);
                    if (current_customer is not null)
                    {
                        var maxRank = leaderboardData.Max(it => it.Rank);
                        if (current_customer is not null)
                        {
                            var current_rank = current_customer.Rank;
                            var higher_rank_min_value = current_rank - high <= 0 ? 1 : current_rank - high;
                            var lower_rank_max_valuie = current_rank + low >= maxRank ? maxRank : current_rank + low;
                            result = leaderboardData.Where(it => it.Rank >= higher_rank_min_value && it.Rank <= lower_rank_max_valuie).ToList();

                        }
                    }
                }
                catch (Exception ex)
                {
                    throw ex.InnerException ?? ex;
                }
            });
            return result;
        }

    }
}

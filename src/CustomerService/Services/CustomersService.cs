using CustomerService.Contracts;
using CustomerService.Contracts.Dtos;

namespace CustomerService.Services
{

    /// <summary>
    /// The customer service interface implementation
    /// </summary>
    public class CustomersService : ICustomersService
    {

        private static readonly object locker = new object();
        // Customer leaderboard collection
        private static List<CustomerDto> leaderboardsData = new List<CustomerDto>();

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
            lock (locker)
            {
                try
                {

                    var existedData = leaderboardsData.FirstOrDefault(it => it.CustomerId == id);
                    if (existedData is not null)
                    {
                        newScore = existedData.Score + score;
                        //remove the customer from leader board
                        leaderboardsData.Remove(existedData);
                    }
                    else
                    {
                        newScore = score;
                    }

                    //add customer into the leader board
                    if (newScore > 0)
                    {
                        var insertIndex = 0;
                        //find tht latest record with same score with new score and id less than new id
                        var latestSameScoreIndex = leaderboardsData.FindLastIndex(it => it.Score == newScore && it.CustomerId < id);

                        if (latestSameScoreIndex >= 0)
                        {
                            insertIndex = latestSameScoreIndex + 1;
                        }
                        else
                        {
                            //find the first one customer index with same score (case when all with same score customers'id great than target id)
                            var fistSameScoreIndex = leaderboardsData.FindIndex(it => it.Score == newScore);
                            if (fistSameScoreIndex >= 0)
                            {
                                insertIndex = fistSameScoreIndex;
                            }
                            else
                            {
                                //find the first customer with the score less than new score
                                var latestIndex = leaderboardsData.FindIndex(it => it.Score < newScore);
                                if (latestIndex < 0)
                                {
                                    insertIndex = leaderboardsData.Count;
                                }
                                else
                                {
                                    insertIndex = latestIndex;
                                }
                            }
                        }

                        leaderboardsData.Insert(insertIndex, new CustomerDto
                        {
                            CustomerId = id,
                            Score = newScore
                        });
                    }

                }
                catch (Exception ex)
                {
                    throw new Exception("An error occurred while updating customer score.", ex);
                }
            }

            return newScore;
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
                if (leaderboardsData is null)
                {
                    return Enumerable.Empty<CustomerDto>().ToList();
                }

                if (start == 0) start = 1;
                if (end == 0) end = leaderboardsData.Count;
                if (end > leaderboardsData.Count) end = leaderboardsData.Count;

                result = leaderboardsData.GetRange(start - 1, end - start + 1);

                if (result.Count > 0)
                {
                    for (int i = 0; i < result.Count; i++)
                    {
                        result[i].Rank = i + start;
                    }
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
                var total = leaderboardsData.Count;
                if (total > 0)
                {
                    var currentCustomer = leaderboardsData.FirstOrDefault(it => it.CustomerId == id);

                    if (currentCustomer is not null)
                    {
                        var currentIndex = leaderboardsData.IndexOf(currentCustomer);
                        var start = currentIndex - high < 0 ? 0 : currentIndex - high + 1;
                        var end = currentIndex + low > total ? total : currentIndex + low + 1;
                        result = await GetByRanksAsync(start, end, cancellationToken);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while retrieving customer ranks.", ex);
            }

            return result;
        }

    }
}

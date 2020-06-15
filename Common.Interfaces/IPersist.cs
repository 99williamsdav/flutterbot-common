namespace Common.Interfaces
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IPersist<T>
    {
        List<T> GetAll();

        Task<List<T>> GetAllAsync();

        /// <summary>
        /// The find items, specifying filters to use in the search query.
        /// </summary>
        List<T> Find(
            IEnumerable<KeyValuePair<string, object>> filters, 
            IList<string> excludeFields = null, 
            string sortBy = null,
            int? limit = null);

        Task<List<T>> FindAsync(
            IEnumerable<KeyValuePair<string, object>> filters,
            IList<string> excludeFields = null,
            string sortBy = null,
            int? limit = null);

        long Remove(IEnumerable<KeyValuePair<string, object>> filters);

        Task<long> RemoveAsync(IEnumerable<KeyValuePair<string, object>> filters);

        void Save(T value);

        Task SaveAsync(T value);

        List<T> Find(string jsonText, Dictionary<string, int> sortDict = null);

        Task<List<T>> FindAsync(string jsonText, Dictionary<string, int> sortDict = null);
    }
}
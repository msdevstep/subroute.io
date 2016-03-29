namespace Subroute.Core.Data
{
    public class PagedCollection<TEntity>
    {
        public int TotalCount { get; set; }

        public int Skip { get; set; }

        public int Take { get; set; }
        
        public TEntity[] Results { get; set; }
    }
}
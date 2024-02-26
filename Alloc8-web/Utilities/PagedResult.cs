namespace Alloc8_web.Utilities
{
    public class PagedResult<T> where T : class
    {
        public PagedResult() { }
        public List<T>? data { get; set; }
        public int totalItems { get; set; }
        public int pageNumber { get; set; }
        public int pageSize { get; set; }
        public int totalPages { get; set; }
    }
}

namespace ElasticsearchTrial.Utilities.Paging
{
    public class PaginationQuery
    {
        private const int _defaultPage = 1;
        private const int _defaultPageSize = 10;

        public PaginationQuery()
        {
            PageNumber = _defaultPage;
            PageSize = _defaultPageSize;
        }
        public PaginationQuery(int pageNumber, int pageSize)
        {
            PageNumber = pageNumber;
            PageSize = pageSize;
        }

        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }
    public class FilterQuery
    {
        public string Field { get; set; }
        public object Value { get; set; }

        public FilterQueryCompareType? CompareType { get; set; } = null;
    }

    public enum FilterQueryCompareType
    {
        Equals = 0,

        GreaterThan = 1,

        LessThan = 2,

        EqualsOrGreaterThan = 3,

        EqualsOrLessThan = 4
    }
}

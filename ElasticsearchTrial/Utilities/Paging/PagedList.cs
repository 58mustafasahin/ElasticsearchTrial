namespace ElasticsearchTrial.Utilities.Paging;

public class PagedProperty
{
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }


    public int TotalPages => PageSize == 0 ? 1 : (int)Math.Ceiling(TotalCount / (double)PageSize);


    public bool HasPrevious => CurrentPage > 1;


    public bool HasNext => CurrentPage < TotalPages;
}

public class PagedList<T>
{
    public PagedList() { }

    public PagedProperty PagedProperty { get; set; }
    public List<T> Items { get; set; }


    public PagedList(List<T> items, int count, int pageNumber, int pageSize)
    {
        SetPagedList(items, count, pageNumber, pageSize);
    }


    public PagedList(List<T> items, int count, int? pageNumber, int? pageSize)
    {
        SetPagedList(items, count, pageNumber == null ? 0 : pageNumber.Value, pageSize == null ? 0 : pageSize.Value);
    }

    public void SetPagedList(List<T> items, int count, int pageNumber, int pageSize)
    {
        Items = items;
        PagedProperty = new PagedProperty();
        PagedProperty.TotalCount = count;
        PagedProperty.PageSize = pageSize;
        PagedProperty.CurrentPage = pageNumber;
    }


}

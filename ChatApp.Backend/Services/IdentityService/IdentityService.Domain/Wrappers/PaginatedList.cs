namespace IdentityService.Domain.Wrappers;

public class PaginatedList<T>
{
    public List<T> Items { get; }
    public int Count { get; }
    public int PageNumber { get; }
    public int PageSize { get; }

    public PaginatedList(List<T> items, int count, int pageNumber, int pageSize)
    {
        Items = items;
        Count = count;
        PageNumber = pageNumber;
        PageSize = pageSize;
    }
}
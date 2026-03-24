namespace WebApi.DTOs;

public class PagedResultDto<T>
{
    public required IEnumerable<T> Items { get; set; }
    public required int TotalCount { get; set; }
    public required int Skip { get; set; }
    public required int Take { get; set; }
    public bool HasMore => Skip + Take < TotalCount;
}
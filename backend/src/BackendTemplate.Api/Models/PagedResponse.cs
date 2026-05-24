namespace BackendTemplate.Api.Models;

using BackendTemplate.Application.Common;

public record PagedResponse<T>(
    IReadOnlyList<T> Items,
    int Total,
    int Page,
    int PageSize)
{
    public static PagedResponse<T> From(Page<T> page)
        => new(page.Items, page.Total, page.PageNumber, page.PageSize);
}

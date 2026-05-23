namespace BackendTemplate.Application.Common;

public record Page<T>(IReadOnlyList<T> Items, int Total, int PageNumber, int PageSize);

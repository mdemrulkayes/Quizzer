using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace Shared.Core;
public sealed class PaginatedList<T>
{
    public IReadOnlyCollection<T> Items { get; }
    public int TotalPages { get; }
    public int PageNumber { get; }
    public int TotalCount { get; }

    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;

    [JsonConstructor]
    public PaginatedList(IReadOnlyCollection<T> items, int totalPages, int pageNumber, int totalCount)
    {
        Items = items;
        TotalPages = totalPages;
        PageNumber = pageNumber;
        TotalCount = totalCount;
    }

    public static async Task<PaginatedList<T>> CreatePaginatedListAsync(IQueryable<T> source, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        var count = await source.CountAsync(cancellationToken);
        var items = await source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        var totalPages = (int)Math.Ceiling(count / (double)pageSize);

        return new PaginatedList<T>(items, totalPages, pageNumber, count);
    }

}

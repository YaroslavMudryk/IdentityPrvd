namespace IdentityPrvd.WebApi.Extensions;

public static class CollectionExtensions
{
    public static IReadOnlyCollection<T> SortByCollectionId<T>(
        this IReadOnlyCollection<T> resultCollection,
        IReadOnlyCollection<int> sortedIdsCollection,
        Func<T, int> getId)
        => sortedIdsCollection
            .Select(id => resultCollection.FirstOrDefault(d => getId(d) == id))
            .Where(d => d is not null)
            .ToList()!;

    public static Dictionary<string, List<string>> GroupUnionCollectionBy<T>(
        this IReadOnlyCollection<T> firstCollection,
        IReadOnlyCollection<T> secondCollection,
        Func<T, Ulid> distinctBy,
        Func<T, string> groupBy,
        Func<T, string> select)
        => firstCollection
            .Concat(secondCollection)
            .DistinctBy(distinctBy)
            .GroupBy(groupBy)
            .ToDictionary(g => g.Key, g => g.Select(select).Distinct().ToList());
}

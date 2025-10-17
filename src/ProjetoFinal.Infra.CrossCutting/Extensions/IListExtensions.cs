namespace ProjetoFinal.Infra.CrossCutting.Extensions;

public static class ListExtensions
{
    public static void AddRange<T>(this IList<T> list, IEnumerable<T> items)
    {
        items.ToList().ForEach(item => list.Add(item));
    }
}
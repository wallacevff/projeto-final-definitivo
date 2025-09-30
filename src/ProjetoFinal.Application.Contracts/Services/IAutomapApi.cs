namespace ProjetoFinal.Aplication.Services.Services;

public interface IAutomapApi
{
    TDestination MapFrom<TDestination>(object source)
        where TDestination : class;

    void MapTo<TDestination, TSource>(TSource source, TDestination destination)
        where TDestination : class
        where TSource : class;
}
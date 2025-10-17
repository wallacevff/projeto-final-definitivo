using System.Linq.Expressions;
using System.Reflection;
using LinqKit;
using ProjetoFinal.Domain.Shared.Filters;

namespace ProjetoFinal.Infra.Data.Repositories;

public partial class DefaultRepository<TEntity, TFilter, TKey>
    where TEntity : class
    where TFilter : Filter
{
    protected virtual Expression<Func<TEntity, bool>> GetFilters(TFilter filter)
    {
        var predicate = PredicateBuilder.New<TEntity>(true);
        var excludedProperties = new List<string> { "TotalPages", "PageSize", "PageNumber" };

        IList<PropertyInfo> filterProperties = typeof(TFilter).GetProperties();

        foreach (PropertyInfo filterProperty in filterProperties)
        {
            var filterValue = filterProperty.GetValue(filter);

            // Ignora propriedades excluídas
            if (excludedProperties.Contains(filterProperty.Name))
                continue;

            if (filterValue != null)
            {
                PropertyInfo? entityProperty = typeof(TEntity).GetProperty(
                    filterProperty.Name.Replace("From", "").Replace("To", ""),
                    BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

                if (entityProperty != null)
                {
                    var parameter = Expression.Parameter(typeof(TEntity), "p");
                    var property = Expression.Property(parameter, entityProperty.Name);
                    var constant = Expression.Constant(filterValue);

                    // Se for string, usar Contains
                    if (entityProperty.PropertyType == typeof(string))
                    {
                        var method = typeof(string).GetMethod("Contains", new[] { typeof(string) });
                        if (method != null)
                        {
                            var body = Expression.Call(property, method, constant);
                            predicate = predicate.And(Expression.Lambda<Func<TEntity, bool>>(body, parameter));
                        }
                    }
                    // Trata propriedades nulas (Nullable<>)
                    else if (entityProperty.PropertyType.IsGenericType &&
                             entityProperty.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                    {
                        var underlyingType = Nullable.GetUnderlyingType(entityProperty.PropertyType)!;

                        if (underlyingType == typeof(DateTime))
                        {
                            // DateTime?
                            if (filterProperty.Name.EndsWith("From"))
                            {
                                var hasValue = Expression.Property(property, "HasValue");
                                var value = Expression.Property(property, "Value");
                                var body = Expression.AndAlso(
                                    hasValue,
                                    Expression.GreaterThanOrEqual(value, constant)
                                );
                                predicate = predicate.And(Expression.Lambda<Func<TEntity, bool>>(body, parameter));
                            }
                            else if (filterProperty.Name.EndsWith("To"))
                            {
                                var hasValue = Expression.Property(property, "HasValue");
                                var value = Expression.Property(property, "Value");
                                var body = Expression.AndAlso(
                                    hasValue,
                                    Expression.LessThanOrEqual(value, constant)
                                );
                                predicate = predicate.And(Expression.Lambda<Func<TEntity, bool>>(body, parameter));
                            }
                            else
                            {
                                var hasValue = Expression.Property(property, "HasValue");
                                var value = Expression.Property(property, "Value");
                                var body = Expression.AndAlso(
                                    hasValue,
                                    Expression.Equal(value, constant)
                                );
                                predicate = predicate.And(Expression.Lambda<Func<TEntity, bool>>(body, parameter));
                            }
                        }
                        else if (underlyingType == typeof(DateOnly))
                        {
                            // DateOnly?
                            var hasValue = Expression.Property(property, "HasValue");
                            var value = Expression.Property(property, "Value");

                            var compareMethod = typeof(DateOnly).GetMethod("CompareTo", new[] { typeof(DateOnly) });
                            var zero = Expression.Constant(0);

                            if (filterProperty.Name.EndsWith("From"))
                            {
                                var compareCall = Expression.Call(value, compareMethod!, constant);
                                var body = Expression.AndAlso(
                                    hasValue,
                                    Expression.GreaterThanOrEqual(compareCall, zero)
                                );
                                predicate = predicate.And(Expression.Lambda<Func<TEntity, bool>>(body, parameter));
                            }
                            else if (filterProperty.Name.EndsWith("To"))
                            {
                                var compareCall = Expression.Call(value, compareMethod!, constant);
                                var body = Expression.AndAlso(
                                    hasValue,
                                    Expression.LessThanOrEqual(compareCall, zero)
                                );
                                predicate = predicate.And(Expression.Lambda<Func<TEntity, bool>>(body, parameter));
                            }
                            else
                            {
                                var body = Expression.AndAlso(
                                    hasValue,
                                    Expression.Equal(value, constant)
                                );
                                predicate = predicate.And(Expression.Lambda<Func<TEntity, bool>>(body, parameter));
                            }
                        }
                        else
                        {
                            // Outros tipos Nullable
                            var hasValue = Expression.Property(property, "HasValue");
                            var value = Expression.Property(property, "Value");
                            var body = Expression.AndAlso(
                                hasValue,
                                Expression.Equal(value, constant)
                            );
                            predicate = predicate.And(Expression.Lambda<Func<TEntity, bool>>(body, parameter));
                        }
                    }
                    else if (entityProperty.PropertyType == typeof(DateTime))
                    {
                        // DateTime
                        if (filterProperty.Name.EndsWith("From"))
                        {
                            var body = Expression.GreaterThanOrEqual(property, constant);
                            predicate = predicate.And(Expression.Lambda<Func<TEntity, bool>>(body, parameter));
                        }
                        else if (filterProperty.Name.EndsWith("To"))
                        {
                            var body = Expression.LessThanOrEqual(property, constant);
                            predicate = predicate.And(Expression.Lambda<Func<TEntity, bool>>(body, parameter));
                        }
                        else
                        {
                            var body = Expression.Equal(property, constant);
                            predicate = predicate.And(Expression.Lambda<Func<TEntity, bool>>(body, parameter));
                        }
                    }
                    else if (entityProperty.PropertyType == typeof(DateOnly))
                    {
                        // DateOnly
                        var compareMethod = typeof(DateOnly).GetMethod("CompareTo", new[] { typeof(DateOnly) });
                        var zero = Expression.Constant(0);

                        if (filterProperty.Name.EndsWith("From"))
                        {
                            var compareCall = Expression.Call(property, compareMethod!, constant);
                            var body = Expression.GreaterThanOrEqual(compareCall, zero);
                            predicate = predicate.And(Expression.Lambda<Func<TEntity, bool>>(body, parameter));
                        }
                        else if (filterProperty.Name.EndsWith("To"))
                        {
                            var compareCall = Expression.Call(property, compareMethod!, constant);
                            var body = Expression.LessThanOrEqual(compareCall, zero);
                            predicate = predicate.And(Expression.Lambda<Func<TEntity, bool>>(body, parameter));
                        }
                        else
                        {
                            var body = Expression.Equal(property, constant);
                            predicate = predicate.And(Expression.Lambda<Func<TEntity, bool>>(body, parameter));
                        }
                    }
                    else
                    {
                        // Tipos comuns
                        var body = Expression.Equal(property, constant);
                        predicate = predicate.And(Expression.Lambda<Func<TEntity, bool>>(body, parameter));
                    }
                }
            }
        }

        return predicate;
    }

    protected virtual Expression<Func<TEntityInternal, bool>> GetFiltersInternal<TEntityInternal, TFilterInternal>(
        TFilterInternal filter)
    {
        var predicate = PredicateBuilder.New<TEntityInternal>(true);
        var excludedProperties = new List<string> { "TotalPages", "PageSize", "PageNumber" };

        IList<PropertyInfo> filterProperties = typeof(TFilterInternal).GetProperties();

        foreach (PropertyInfo filterProperty in filterProperties)
        {
            var filterValue = filterProperty.GetValue(filter);

            // Ignora propriedades excluídas
            if (excludedProperties.Contains(filterProperty.Name))
                continue;

            if (filterValue != null)
            {
                PropertyInfo? entityProperty = typeof(TEntityInternal).GetProperty(
                    filterProperty.Name.Replace("From", "").Replace("To", ""),
                    BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

                if (entityProperty != null)
                {
                    var parameter = Expression.Parameter(typeof(TEntityInternal), "p");
                    var property = Expression.Property(parameter, entityProperty.Name);
                    var constant = Expression.Constant(filterValue);

                    // Se for string, usar Contains
                    if (entityProperty.PropertyType == typeof(string))
                    {
                        var method = typeof(string).GetMethod("Contains", new[] { typeof(string) });
                        if (method != null)
                        {
                            var body = Expression.Call(property, method, constant);
                            predicate = predicate.And(Expression.Lambda<Func<TEntityInternal, bool>>(body, parameter));
                        }
                    }
                    // Trata propriedades nulas (Nullable<>)
                    else if (entityProperty.PropertyType.IsGenericType &&
                             entityProperty.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                    {
                        var underlyingType = Nullable.GetUnderlyingType(entityProperty.PropertyType)!;

                        if (underlyingType == typeof(DateTime))
                        {
                            // DateTime?
                            if (filterProperty.Name.EndsWith("From"))
                            {
                                var hasValue = Expression.Property(property, "HasValue");
                                var value = Expression.Property(property, "Value");
                                var body = Expression.AndAlso(
                                    hasValue,
                                    Expression.GreaterThanOrEqual(value, constant)
                                );
                                predicate = predicate.And(
                                    Expression.Lambda<Func<TEntityInternal, bool>>(body, parameter));
                            }
                            else if (filterProperty.Name.EndsWith("To"))
                            {
                                var hasValue = Expression.Property(property, "HasValue");
                                var value = Expression.Property(property, "Value");
                                var body = Expression.AndAlso(
                                    hasValue,
                                    Expression.LessThanOrEqual(value, constant)
                                );
                                predicate = predicate.And(
                                    Expression.Lambda<Func<TEntityInternal, bool>>(body, parameter));
                            }
                            else
                            {
                                var hasValue = Expression.Property(property, "HasValue");
                                var value = Expression.Property(property, "Value");
                                var body = Expression.AndAlso(
                                    hasValue,
                                    Expression.Equal(value, constant)
                                );
                                predicate = predicate.And(
                                    Expression.Lambda<Func<TEntityInternal, bool>>(body, parameter));
                            }
                        }
                        else if (underlyingType == typeof(DateOnly))
                        {
                            // DateOnly?
                            var hasValue = Expression.Property(property, "HasValue");
                            var value = Expression.Property(property, "Value");

                            var compareMethod = typeof(DateOnly).GetMethod("CompareTo", new[] { typeof(DateOnly) });
                            var zero = Expression.Constant(0);

                            if (filterProperty.Name.EndsWith("From"))
                            {
                                var compareCall = Expression.Call(value, compareMethod!, constant);
                                var body = Expression.AndAlso(
                                    hasValue,
                                    Expression.GreaterThanOrEqual(compareCall, zero)
                                );
                                predicate = predicate.And(
                                    Expression.Lambda<Func<TEntityInternal, bool>>(body, parameter));
                            }
                            else if (filterProperty.Name.EndsWith("To"))
                            {
                                var compareCall = Expression.Call(value, compareMethod!, constant);
                                var body = Expression.AndAlso(
                                    hasValue,
                                    Expression.LessThanOrEqual(compareCall, zero)
                                );
                                predicate = predicate.And(
                                    Expression.Lambda<Func<TEntityInternal, bool>>(body, parameter));
                            }
                            else
                            {
                                var body = Expression.AndAlso(
                                    hasValue,
                                    Expression.Equal(value, constant)
                                );
                                predicate = predicate.And(
                                    Expression.Lambda<Func<TEntityInternal, bool>>(body, parameter));
                            }
                        }
                        else
                        {
                            // Outros tipos Nullable
                            var hasValue = Expression.Property(property, "HasValue");
                            var value = Expression.Property(property, "Value");
                            var body = Expression.AndAlso(
                                hasValue,
                                Expression.Equal(value, constant)
                            );
                            predicate = predicate.And(Expression.Lambda<Func<TEntityInternal, bool>>(body, parameter));
                        }
                    }
                    else if (entityProperty.PropertyType == typeof(DateTime))
                    {
                        // DateTime
                        if (filterProperty.Name.EndsWith("From"))
                        {
                            var body = Expression.GreaterThanOrEqual(property, constant);
                            predicate = predicate.And(Expression.Lambda<Func<TEntityInternal, bool>>(body, parameter));
                        }
                        else if (filterProperty.Name.EndsWith("To"))
                        {
                            var body = Expression.LessThanOrEqual(property, constant);
                            predicate = predicate.And(Expression.Lambda<Func<TEntityInternal, bool>>(body, parameter));
                        }
                        else
                        {
                            var body = Expression.Equal(property, constant);
                            predicate = predicate.And(Expression.Lambda<Func<TEntityInternal, bool>>(body, parameter));
                        }
                    }
                    else if (entityProperty.PropertyType == typeof(DateOnly))
                    {
                        // DateOnly
                        var compareMethod = typeof(DateOnly).GetMethod("CompareTo", new[] { typeof(DateOnly) });
                        var zero = Expression.Constant(0);

                        if (filterProperty.Name.EndsWith("From"))
                        {
                            var compareCall = Expression.Call(property, compareMethod!, constant);
                            var body = Expression.GreaterThanOrEqual(compareCall, zero);
                            predicate = predicate.And(Expression.Lambda<Func<TEntityInternal, bool>>(body, parameter));
                        }
                        else if (filterProperty.Name.EndsWith("To"))
                        {
                            var compareCall = Expression.Call(property, compareMethod!, constant);
                            var body = Expression.LessThanOrEqual(compareCall, zero);
                            predicate = predicate.And(Expression.Lambda<Func<TEntityInternal, bool>>(body, parameter));
                        }
                        else
                        {
                            var body = Expression.Equal(property, constant);
                            predicate = predicate.And(Expression.Lambda<Func<TEntityInternal, bool>>(body, parameter));
                        }
                    }
                    else
                    {
                        // Tipos comuns
                        var body = Expression.Equal(property, constant);
                        predicate = predicate.And(Expression.Lambda<Func<TEntityInternal, bool>>(body, parameter));
                    }
                }
            }
        }

        return predicate;
    }
}
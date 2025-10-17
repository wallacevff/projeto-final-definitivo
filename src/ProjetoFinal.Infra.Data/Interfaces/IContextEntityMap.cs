using Microsoft.EntityFrameworkCore;

namespace ProjetoFinal.Infra.Data.Interfaces;

public interface IContextEntityMap<TEntity> : IEntityTypeConfiguration<TEntity> where TEntity : class
{
}
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Oip.Base.Data.Contexts;

namespace Oip.Base.Data.Extensions;

/// <summary>
/// Entity Extension
/// </summary>
public static class EntityTypeBuilderExtension
{
    /// <summary>
    /// Set table by default and exec tableBuilder
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="database"></param>
    /// <param name="tableBuilder"></param>
    /// <typeparam name="TEntity"></typeparam>
    public static void SetTable<TEntity>(this EntityTypeBuilder<TEntity> builder, DatabaseFacade database,
        Action<TableBuilder<TEntity>>? tableBuilder = null)
        where TEntity : class
    {
        builder.SetTableWithSchema(database, OipModuleContext.SchemaName, tableBuilder);
    }


    /// <summary>
    /// Set table by default and exec tableBuilder
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="database"></param>
    /// <param name="schemaName"></param>
    /// <param name="tableBuilder"></param>
    /// <typeparam name="TEntity"></typeparam>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SetTableWithSchema<TEntity>(
        this EntityTypeBuilder<TEntity> builder, 
        DatabaseFacade database,
        string schemaName,
        Action<TableBuilder<TEntity>>? tableBuilder = null)
        where TEntity : class
    {
        var tableName = typeof(TEntity).Name.Replace("Entity", string.Empty);
        if (database.IsNpgsql() || database.IsSqlServer())
            builder.ToTable(tableName, schemaName, tableBuilder ?? (_ => { }));
    }

    /// <summary>
    /// Design without key but using with key
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="designTime"></param>
    /// <param name="keyExpression"></param>
    /// <typeparam name="TEntity"></typeparam>
    public static void SetPrimaryKey<TEntity>(this EntityTypeBuilder<TEntity> builder, bool designTime,
        Expression<Func<TEntity, object?>> keyExpression) where TEntity : class
    {
        if (designTime)
            builder.HasNoKey();
        else
            builder.HasKey(keyExpression);
    }
}
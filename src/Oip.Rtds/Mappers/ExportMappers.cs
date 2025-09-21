using Oip.Rtds.Data.Dtos;
using Oip.Rtds.Data.Entities;
using Oip.Rtds.Grpc;
using Riok.Mapperly.Abstractions;

namespace Oip.Rts.Mappers;

/// <summary>
/// Mapper for ExportModule
/// </summary>
[Mapper]
public static partial class TagMapper
{
    /// <summary>
    /// Maps a TagEntity to a TagResponse.
    /// </summary>
    /// <param name="entity">The TagEntity to map.</param>
    /// <returns>The TagResponse representation of the entity.</returns>
    [MapProperty("Descriptor", "Descriptor_")]
    [MapperIgnoreSource(nameof(TagEntity.CreationDate))]
    [MapperIgnoreSource(nameof(TagEntity.Creator))]
    public static partial TagResponse ToTagResponse(this TagEntity entity);
}
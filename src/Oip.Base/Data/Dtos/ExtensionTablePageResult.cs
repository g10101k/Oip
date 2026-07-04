namespace Oip.Data.Dtos;

/// <summary>
/// Page result for extension-aware tables.
/// </summary>
public record ExtensionTablePageResult<TRow>(
    List<TRow> Data,
    int Total,
    int First,
    int Rows,
    List<ExtensionTableColumnDto> Columns);

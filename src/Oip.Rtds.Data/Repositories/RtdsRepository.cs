using Oip.Rtds.Data.Contexts;

namespace Oip.Rtds.Data.Repositories;

/// <summary>
/// Repository for managing RTDS (Real-Time Data System) data operations
/// </summary>
public class RtdsRepository(RtdsContext context)
{
    /// <summary>
    /// Inserts a collection of numeric values into the RTDS database
    /// </summary>
    /// <param name="values">List of value DTOs containing double precision data to insert</param>
    /// <returns>Task representing the asynchronous insert operation</returns>
    public async Task InsertValues(List<InsertValueDto<double>> values)
    {
        await context.InsertValues(values);
    }
}
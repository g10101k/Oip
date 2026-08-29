using Oip.Rtds.Data.Contexts;
using Oip.Rtds.Data.Services;

namespace Oip.Rtds.Data.Repositories;

/// <summary>
/// Repository for managing RTDS (Real-Time Data System) data operations
/// </summary>
/// <param name="writeQueue">The queue drained by the background writer.</param>
public class RtdsRepository(RtdsWriteQueue writeQueue)
{
    /// <summary>
    /// Enqueues a collection of numeric values for writing to the RTDS database.
    /// The values are written by the background writer in batches, so the call returns
    /// once the values are accepted by the queue, not once they are stored.
    /// </summary>
    /// <param name="values">List of value DTOs containing double precision data to insert</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>Task representing the asynchronous enqueue operation</returns>
    public async Task InsertValues(List<InsertValueDto<double>> values,
        CancellationToken cancellationToken = default)
    {
        await writeQueue.EnqueueAsync(values, cancellationToken);
    }
}

using Oip.Rtds.Data.Contexts;

namespace Oip.Rtds.Data.Repositories;

public class RtdsRepository(RtdsContext context)
{
    public async Task InsertValues(List<InsertValueDto<double>> values)
    {
        await context.InsertValues(values);
    }
}
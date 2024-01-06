using System;
using System.Collections.Generic;
using System.Linq;
using Oip.Security.Bl.Dtos.Common;

namespace Oip.Security.Bl.Helpers;

public static class EnumHelpers
{
    public static List<SelectItemDto> ToSelectList<T>() where T : struct, IComparable
    {
        var selectItems = Enum.GetValues(typeof(T))
            .Cast<T>()
            .Select(x => new SelectItemDto(Convert.ToInt16(x).ToString(), x.ToString())).ToList();

        return selectItems;
    }
}
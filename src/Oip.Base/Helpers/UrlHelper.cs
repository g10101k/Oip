using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Oip.Base.Helpers
{
    /// <summary>
    /// Url helper exten
    /// </summary>
    public static class UrlHelper
    {
        /// <summary>
        /// Append part to url with separate /
        /// </summary>
        /// <param name="url"></param>
        /// <param name="part"></param>
        /// <returns></returns>
        public static string UrlAppend(this string url, string part)
        {
            return string.Format("{0}/{1}", url.TrimEnd('/'), part.TrimStart('/'));
        }
    }
}

namespace Oip.Base.Helpers
{
    /// <summary>
    /// Url helper
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
            return $"{url.TrimEnd('/')}/{part.TrimStart('/')}";
        }
    }
}

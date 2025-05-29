namespace Oip.Base.Controllers.Api
{
    /// <summary>
    /// Represents a request to delete a module by its identifier.
    /// </summary>
    public class ModuleDeleteRequest
    {
        /// <summary>
        /// Gets or sets the unique identifier of the module to be deleted.
        /// </summary>
        public int ModuleId { get; set; }
    }
}
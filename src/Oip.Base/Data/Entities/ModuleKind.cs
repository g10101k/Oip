namespace Oip.Base.Data.Entities;

/// <summary>
/// Defines how a module is supplied to OIP.
/// </summary>
public enum ModuleKind
{
    /// <summary>
    /// Module is compiled into the OIP application.
    /// </summary>
    Core = 0,

    /// <summary>
    /// Module renders an external page in an iframe.
    /// </summary>
    Iframe = 1,

    /// <summary>
    /// Module is loaded at runtime from a Web Component extension bundle.
    /// </summary>
    Extension = 2
}

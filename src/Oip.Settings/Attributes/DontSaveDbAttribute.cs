namespace Oip.Settings.Attributes;

/// <summary>
/// Attribute for exclude property to save into db
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class NotSaveToDbAttribute : Attribute;
namespace Oip.Settings.Attributes;

/// <summary>
/// Marks the property to exclude it from being saved to db
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class NotSaveToDbAttribute : Attribute;
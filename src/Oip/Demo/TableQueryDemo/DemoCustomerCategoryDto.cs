namespace Oip.Demo.TableQueryDemo;

/// <summary>
/// Customer category with the rights of the current user on it.
/// </summary>
/// <param name="Id">Identifier.</param>
/// <param name="Name">Name.</param>
/// <param name="Rights">Rights of the current user besides read: edit, delete, manage-acl.</param>
public record DemoCustomerCategoryDto(int Id, string Name, List<string> Rights);

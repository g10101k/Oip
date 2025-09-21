namespace Oip.Rtds.Data.Entities;

public class InterfaceEntity
{
    public uint Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public List<TagEntity>? Tags { get; set; } = new();
}
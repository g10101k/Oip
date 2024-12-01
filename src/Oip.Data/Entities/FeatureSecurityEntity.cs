namespace Oip.Data.Entities;

public class FeatureSecurityEntity
{
    public int FeatureSecurityId { get; set; }

    public int FeatureId { get; set; }

    public string Role { get; set; }

    public string Right { get; set; }

    public FeatureEntity Feature {get;set;}
}
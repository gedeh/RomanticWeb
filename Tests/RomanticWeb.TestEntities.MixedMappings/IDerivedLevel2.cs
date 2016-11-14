namespace RomanticWeb.TestEntities.MixedMappings
{
    public interface IDerivedLevel2 : IDerived
    {
        new IDerivedLevel2 Property { get; set; }
    }
}
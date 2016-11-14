namespace RomanticWeb.TestEntities.MixedMappings
{
    public interface IDerived : IGenericParent<int>
    {
        IDerived Property { get; set; }
    }
}
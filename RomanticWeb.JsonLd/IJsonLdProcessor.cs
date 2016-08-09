using System.Collections.Generic;
using RomanticWeb.Model;

namespace RomanticWeb.JsonLd
{
    public interface IJsonLdProcessor
    {
        string FromRdf(IEnumerable<IEntityQuad> dataset, bool userRdfType = false, bool useNativeTypes = false);

        IEnumerable<IEntityQuad> ToRdf(string json, JsonLdOptions options, bool produceGeneralizedRdf = false);

        string Compact(string json, string jsonLdContext);

        string Flatten(string json, string jsonLdContext);

        string Expand(string json, JsonLdOptions options);
    }
}
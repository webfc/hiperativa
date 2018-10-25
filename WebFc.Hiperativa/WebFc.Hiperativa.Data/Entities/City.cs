using MongoDB.Bson.Serialization.Attributes;

namespace WebFc.Hiperativa.Data.Entities
{
    [BsonIgnoreExtraElements]
    public class City : UtilityFramework.Infra.Core.MongoDb.Data.Modelos.City
    {
    }
}
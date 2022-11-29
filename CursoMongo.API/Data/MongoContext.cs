using CursoMongo.API.Data.Schemas;
using CursoMongo.API.Domain.Enums;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;

namespace CursoMongo.API.Data;

public class MongoContext
{
    public IMongoDatabase Db { get; }

    public MongoContext(IConfiguration configuration)
    {
        try
        {
            var client = new MongoClient(configuration["ConexaoDb"]);
            Db = client.GetDatabase(configuration["NomeDB"]);

            MapClasses();
        }
        catch (Exception ex)
        {
            throw new MongoException("Não foi possível se conectar ao MongoDb", ex);
        }

    }

    private void MapClasses()
    {
        if (!BsonClassMap.IsClassMapRegistered(typeof(RestauranteSchema)))
        {
            BsonClassMap.RegisterClassMap<RestauranteSchema>(i =>
            {
                i.AutoMap();
                i.MapIdMember(c => c.Id);
                i.MapMember(c => c.Cozinha).SetSerializer(new EnumSerializer<ECozinha>(BsonType.Int32));
                i.SetIgnoreExtraElements(true);
            });
        }      
    }
}

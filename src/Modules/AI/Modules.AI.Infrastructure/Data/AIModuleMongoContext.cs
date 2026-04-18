using Modules.AI.Core.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;

namespace Modules.AI.Infrastructure.Data;

public class AIModuleMongoContext
{
    private readonly IMongoDatabase _database;

    static AIModuleMongoContext()
    {
        BsonSerializer.TryRegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));
    }

    public AIModuleMongoContext(string connectionString, string databaseName)
    {
        var client = new MongoClient(connectionString);
        _database = client.GetDatabase(databaseName);
    }

    public IMongoCollection<AIProviderConfig> ProviderConfigs =>
        _database.GetCollection<AIProviderConfig>("ai_provider_configs");

    public IMongoCollection<AIGenerationRequest> GenerationRequests =>
        _database.GetCollection<AIGenerationRequest>("ai_generation_requests");

    public IMongoCollection<InterviewPrepMaterial> InterviewPrepMaterials =>
        _database.GetCollection<InterviewPrepMaterial>("interview_prep_materials");
}

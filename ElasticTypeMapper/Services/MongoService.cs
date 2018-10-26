using MongoDB.Driver;
using Nest;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ElasticTypeMapper.Services
{
    public class MongoService
    {
        private readonly MongoClient _client = new MongoClient("mongodb://localhost");
        private readonly ElasticService _elasticService = new ElasticService();
        private const string ElasticType = "item";

        public MongoService()
        {
        }

        public async Task<List<string>> GetListOfDataBasesAsync()
        {
            Dictionary<string, object> namespaceMap = new Dictionary<string, object>();

            var databaseNames = new List<string>();
            var cursor = await _client.ListDatabaseNamesAsync();

            Console.WriteLine("Getting Databases");

            await cursor.ForEachAsync(db =>
            {
                if (db != "admin" &&
                    db != "config" &&
                    db != "local")
                {
                    databaseNames.Add(db);
                }
            });

            Console.WriteLine("Creating Elastic Indices");

            foreach (var databaseName in databaseNames)
            {
                var collectionCursor = await _client.GetDatabase(databaseName).ListCollectionNamesAsync();
                var listOfMapperTasks = new List<Task<ICreateIndexResponse>>();
                await collectionCursor.ForEachAsync(collection =>
                {
                    collection = collection.ToLower();
                    var collecionKey = databaseName + "." + collection;
                    var kv = new Dictionary<string, string>
                    {
                        { "rename",databaseName+ collection + "." + ElasticType }
                    };
                    listOfMapperTasks.Add(_elasticService.CreateMapperAsync(databaseName + collection, ElasticType));
                    namespaceMap.Add(collecionKey, kv);
                });

                foreach (var task in listOfMapperTasks)
                {
                    var result = await task;

                    if (!result.IsValid)
                    {
                        Console.WriteLine(result.ServerError.Error);
                    }
                }
            }
            var finalMap = new Dictionary<string, object>
            {
                { "namespaces", namespaceMap }
            };
            Console.WriteLine(JsonConvert.SerializeObject(finalMap, Formatting.Indented));
            return databaseNames.ToList();
        }
    }
}
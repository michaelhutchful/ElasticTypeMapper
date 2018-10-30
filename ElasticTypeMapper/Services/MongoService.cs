using MongoDB.Driver;
using Nest;
using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ElasticTypeMapper.Services
{
    public class MongoService
    {
        private readonly ElasticService _elasticService = new ElasticService();
        private const string ElasticType = "item";
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public MongoService()
        {
        }

        public async Task<List<string>> GetListOfDataBasesAsync(string elasticUrl, string mongoUrl)
        {
            MongoClient client = new MongoClient(mongoUrl);
            Dictionary<string, object> namespaceMap = new Dictionary<string, object>();

            var databaseNames = new List<string>();
            var cursor = await client.ListDatabaseNamesAsync();

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
                var collectionCursor = await client.GetDatabase(databaseName).ListCollectionNamesAsync();
                var listOfMapperTasks = new List<Task<ICreateIndexResponse>>();
                await collectionCursor.ForEachAsync(collection =>
                {
                    collection = collection.ToLower();
                    var collecionKey = databaseName + "." + collection;
                    var kv = new Dictionary<string, string>
                    {
                        { "rename",databaseName+ collection + "." + ElasticType }
                    };
                    listOfMapperTasks.Add(_elasticService.CreateMapperAsync
                        (databaseName + collection, ElasticType, elasticUrl));
                    namespaceMap.Add(collecionKey, kv);
                });

                foreach (var task in listOfMapperTasks)
                {
                    var result = await task;

                    if (!result.IsValid)
                    {
                        _logger.Warn(result.ServerError.Error);
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
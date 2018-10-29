using Microsoft.Extensions.Configuration;

namespace ElasticTypeMapper.Models
{
    public class AppSettings
    {
        private readonly IConfiguration config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", true, true)
            .Build();

        public string SqlHost { get => config["sqlHost"]; }
        public string SqlUser { get => config["sqlUser"]; }
        public string SqlSchema { get => config["sqlSchema"]; }
        public string SqlPassword { get => config["sqlPassword"]; }
        public string SqlPort { get => config["sqlPort"]; }
        public string ElasticUrl { get => config["elasticAddress"]; }
        public string MongoUrl { get => config["mongoUrl"]; }
        public string SourceDb { get => config["sourceDb"]; }
    }
}
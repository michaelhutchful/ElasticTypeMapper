using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ElasticTypeMapper.Services
{
    public class MysqlService
    {
        private readonly string _connStr = "server=localhost;user=root;database=slydepaydb;port=3306;password=password";
        private ElasticService _elasticService = new ElasticService();
        private const string elasticType = "item";

        public async Task GetAllTablesAsync()
        {
            var tableNames = new List<string>();
            Console.WriteLine("Connecting to MySql Database:");
            MySqlConnection conn = new MySqlConnection(_connStr);
            await conn.OpenAsync();
            MySqlCommand cmd = new MySqlCommand("show tables", conn);

            var monReader = await cmd.ExecuteReaderAsync();

            while (monReader.Read())
            {
                var tableName = monReader.GetString(0);
                tableNames.Add(tableName);
                await _elasticService.CreateMapperAsync(tableName, tableName);
            }

            await conn.CloseAsync();
            Console.WriteLine("Connection to MySql complete");
        }
    }
}
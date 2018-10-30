using MySql.Data.MySqlClient;
using NLog;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ElasticTypeMapper.Services
{
    public class MysqlService
    {
        public string SqlHost { get; set; }
        public string SqlUser { get; set; }
        public string SqlSchema { get; set; }
        public string SqlPassword { get; set; }
        public string SqlPort { get; set; }

        private readonly ElasticService _elasticService = new ElasticService();
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public MysqlService(string sqlHost, string sqlUser, string sqlSchema, string sqlPassword, string sqlPort)
        {
            SqlHost = sqlHost ?? throw new ArgumentNullException(nameof(sqlHost));
            SqlUser = sqlUser ?? throw new ArgumentNullException(nameof(sqlUser));
            SqlSchema = sqlSchema ?? throw new ArgumentNullException(nameof(sqlSchema));
            SqlPassword = sqlPassword ?? throw new ArgumentNullException(nameof(sqlPassword));
            SqlPort = sqlPort ?? throw new ArgumentNullException(nameof(sqlPort));
        }

        public async Task GetAllTablesAsync(string elasticUrl)
        {
            var connString =
                $"server={SqlHost};user={SqlUser};database={SqlSchema};port={SqlPort};password={SqlPassword}";
            var tableNames = new List<string>();
            Console.WriteLine(Properties.Resources.ConnectingToMySql);
            MySqlConnection conn = new MySqlConnection(connString);
            await conn.OpenAsync();
            MySqlCommand cmd = new MySqlCommand(Properties.Resources.ShowTablesQuery, conn);

            var monReader = await cmd.ExecuteReaderAsync();

            while (monReader.Read())
            {
                var tableName = monReader.GetString(0);
                tableNames.Add(tableName);
                await _elasticService.CreateMapperAsync(tableName, tableName, elasticUrl);
            }

            await conn.CloseAsync();
            Console.WriteLine(Properties.Resources.SQLComplete);
        }
    }
}
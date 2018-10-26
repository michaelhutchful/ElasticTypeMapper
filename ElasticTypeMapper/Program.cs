using ElasticTypeMapper.Services;
using System;
using System.Threading.Tasks;

namespace ElasticTypeMapper
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            //MongoService mongoService = new MongoService();
            //Console.WriteLine("Welcome to mongo connector mapper");
            //await mongoService.GetListOfDataBasesAsync();
            //Console.WriteLine("Complete, press any key to continue...");
            MysqlService mysqlService = new MysqlService();
            await mysqlService.GetAllTablesAsync();
            Console.ReadLine();
        }
    }
}
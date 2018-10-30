using ElasticTypeMapper.Models;
using ElasticTypeMapper.Services;
using Microsoft.Extensions.CommandLineUtils;
using System;
using System.Threading.Tasks;

namespace ElasticTypeMapper
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var logger = NLog.LogManager.GetCurrentClassLogger();

            var appSettings = new AppSettings();
            var app = new CommandLineApplication
            {
                Name = "ElasticTypeMapper",
                Description = "Used to override default mapper in elastic search"
            };

            app.HelpOption("-?|-h|--help");

            var sqlHostOption = app.Option("--sqlHost",
                $"The ip address of your MySql Server example:--sqlHost {appSettings.SqlHost}",
                CommandOptionType.SingleValue);
            var sqlUserOption = app.Option($"--sqlUser",
                $"The MySql user example: --sqlUser {appSettings.SqlUser}",
                CommandOptionType.SingleValue);
            var sqlSchemaOption = app.Option($"--sqlSchema <optionvalue>",
                $"The schema or database you want to use to create type mapping, " +
                $"example: --sqlSchema {appSettings.SqlSchema}",
                CommandOptionType.SingleValue);
            var sqlPasswordOption = app.Option($"--sqlPassword",
                $"The password of the MySql user, example: --sqlPassword {appSettings.SqlPassword}",
                CommandOptionType.SingleValue);
            var sqlPortOption = app.Option($"--sqlPort",
                $"The port of the MySql server, example: --sqlPort {appSettings.SqlPort}",
                CommandOptionType.SingleValue);
            var elasticUrlOption = app.Option($"--esUrl",
                $"The URL of Elastic Search, example: --esUrl {appSettings.ElasticUrl}",
                CommandOptionType.SingleValue);
            var mongoUrlOption = app.Option($"--mongoUrl",
                $"The URL of Mongo DB, example: --sqlUser {appSettings.MongoUrl}",
                CommandOptionType.SingleValue);
            var sourceDbOption = app.Option($"--sourceDb <optionvalue>",
                $"The type of database that would be used as a reference for type mapping." +
                "Default is MySql \n0: MySql\n1: Mongo",
                CommandOptionType.SingleValue);

            app.OnExecute(async () =>
            {
                var elasticUrl = appSettings.ElasticUrl;
                Enum.TryParse(appSettings.SourceDb, out DatabaseType databaseType);

                if (elasticUrlOption.HasValue())
                {
                    elasticUrl = elasticUrlOption.Value();
                }
                else if (string.IsNullOrWhiteSpace(elasticUrl))
                {
                    Console.WriteLine("You must set an Elastic Search URL, you can do this in appsettings.json or " +
                        "command line argument --esUrl");
                }

                if (sourceDbOption.HasValue())
                {
                    Enum.TryParse(sourceDbOption.Value(), out DatabaseType tempDatabaseType);
                    databaseType = tempDatabaseType;
                }

                switch (databaseType)
                {
                    case DatabaseType.MySql:
                        Console.WriteLine("Mapping using MySql");
                        await MapUsingMySql
                        (logger, appSettings, sqlHostOption, sqlUserOption, sqlSchemaOption, sqlPasswordOption,
                        sqlPortOption, elasticUrl);
                        break;

                    case DatabaseType.Mongo:
                        Console.WriteLine("Mapping using MongoDb");
                        await MapUsingMongoAsync(mongoUrlOption, appSettings, logger, elasticUrl);
                        break;

                    default:
                        break;
                }
                return 0;
            });

            app.Execute(args);
            Console.WriteLine("Press any key to exit");
            Console.ReadLine();
        }

        private static async Task MapUsingMongoAsync
            (CommandOption mongoUrlOption, AppSettings appSettings, NLog.Logger logger, string elasticUrl)
        {
            var mongoUrl = appSettings.MongoUrl;

            if (mongoUrlOption.HasValue())
            {
                mongoUrl = mongoUrlOption.Value();
            }
            else if (string.IsNullOrWhiteSpace(mongoUrl))
            {
                logger.Error("You must set a mongo address, you can do this in appsettings.json or command" +
                    " line argument --mongoUrl");
            }
            MongoService mongoService = new MongoService();
            await mongoService.GetListOfDataBasesAsync(elasticUrl, mongoUrl);
        }

        private static async Task MapUsingMySql
            (NLog.Logger logger, AppSettings appSettings, CommandOption sqlHostOption,
            CommandOption sqlUserOption, CommandOption sqlSchemaOption, CommandOption sqlPasswordOption,
            CommandOption sqlPortOption, string elasticUrl)
        {
            var sqlHost = appSettings.SqlHost;
            var sqlUser = appSettings.SqlUser;
            var sqlSchema = appSettings.SqlSchema;
            var sqlPassword = appSettings.SqlPassword;
            var sqlPort = appSettings.SqlPort;

            if (sqlHostOption.HasValue())
            {
                sqlHost = sqlHostOption.Value();
            }
            else if (string.IsNullOrWhiteSpace(sqlHost))
            {
                logger.Error("You must set a host address, you can do this in appsettings.json or command" +
                    " line argument --sqlHost");
            }

            if (sqlUserOption.HasValue())
            {
                sqlUser = sqlUserOption.Value();
            }
            else if (string.IsNullOrWhiteSpace(sqlUser))
            {
                logger.Error("You must set a MySql user, you can do this in appsettings.json or command" +
                    " line argument --sqlUser");
            }

            if (sqlSchemaOption.HasValue())
            {
                sqlSchema = sqlSchemaOption.Value();
            }
            else if (string.IsNullOrWhiteSpace(sqlSchema))
            {
                logger.Error("You must set a MySql Schema/Databse, you can do this in appsettings.json or " +
                    "command line argument --sqlSchema");
            }

            if (sqlPasswordOption.HasValue())
            {
                sqlPassword = sqlPasswordOption.Value();
                logger.Warn("Try not to use the MySql password as a command line parameter for security " +
                    "reasons.\nYou can always set it in appsettings.json");
            }
            else if (string.IsNullOrWhiteSpace(sqlPassword))
            {
                logger.Error("You must set a MySql passwor, you can do this in appsettings.json or command");
            }

            if (sqlPortOption.HasValue())
            {
                sqlPort = sqlPortOption.Value();
            }
            else if (string.IsNullOrWhiteSpace(sqlPort))
            {
                logger.Error("You must set a port, you can do this in appsettings.json or " +
                    "command line argument --sqlPort");
            }

            Console.WriteLine("MySql Host: " + sqlHost);
            Console.WriteLine("MySql User: " + sqlUser);
            Console.WriteLine("MySql Schema/Database: " + sqlSchema);

            var mySqlService = new MysqlService(sqlHost, sqlUser, sqlSchema, sqlPassword, sqlPort);
            await mySqlService.GetAllTablesAsync(elasticUrl);
        }
    }
}
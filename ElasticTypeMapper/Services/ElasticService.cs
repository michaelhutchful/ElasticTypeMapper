﻿using Nest;
using System;
using System.Threading.Tasks;

namespace ElasticTypeMapper.Services
{
    public class ElasticService
    {
        public async Task<ICreateIndexResponse> CreateMapperAsync
            (string index, string elasticType, string elasticAddress)
        {
            var settings = new ConnectionSettings(new Uri(elasticAddress));
            var client = new ElasticClient(settings);
            Console.WriteLine($"Creating Index: {index} and Type: {elasticType}");
            var indexCreationResult = await client.CreateIndexAsync(index, c => c
                 .Mappings(mappings => mappings.Map<dynamic>(elasticType, m => m
                      .DynamicTemplates(d => d
                          .DynamicTemplate("converttofloat", dt => dt
                               .MatchMappingType("long")
                               .Mapping(mappping => mappping.Generic(g => g.Type("float"))))))));

            return indexCreationResult;
        }
    }
}
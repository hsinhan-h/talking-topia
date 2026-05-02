using ApplicationCore.Dtos;
using ApplicationCore.Entities;
using ApplicationCore.Interfaces;
using ApplicationCore.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel.Connectors.MongoDB;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Memory;
using MongoDB.Driver;
using System;
using System.ClientModel;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationCore.Services
{
    [Experimental("SKEXP0020")]
    public class VectorSearchServices : IVectorSearchService
    {
        private readonly IRepository<Course> _courseRepository;
        private readonly MemoryBuilder _memoryBuilder;
        private readonly ISemanticTextMemory _semanticTextMemory;
        private readonly ApplicationCore.Settings.MongoDbVecotrSearchSettings _mongoDbSettings;
        private readonly MongoDBMemoryStore _mongoDBMemoryStore;
        private readonly string _openAiApiKey;
        private readonly string _connectionString;
        private readonly string _searchIndexName;
        private readonly string _databaseName;
        private readonly string _collectionName;
        private readonly IMongoClient _mongoClient;


        public VectorSearchServices(IRepository<Course> courseRepository, IOptions<MongoDbVecotrSearchSettings> mongoDbSettings,IConfiguration configuration)
        {
            _courseRepository = courseRepository;
            // Initialize the openAI API key for text embedding generation
            _openAiApiKey = configuration["OpenAIApiKey"];
            // Initialize the mongodb settings: connection string, search index name, database name, collection name
            _mongoDbSettings = mongoDbSettings.Value;
            _connectionString = _mongoDbSettings.ConnectionString;
            _searchIndexName = _mongoDbSettings.SearchIndexName;
            _databaseName = _mongoDbSettings.VectorDatabaseName;
            _collectionName = _mongoDbSettings.VectorCollectionName;
            // Initialize the memory store: MongoDBMemoryStore(or you can use other memory store like QdrantMemoryStore, etc.)
            _mongoDBMemoryStore = new MongoDBMemoryStore(_connectionString, _databaseName, _searchIndexName);
            // 顯式地創建 ApiKeyCredential，避免隱式轉換錯誤
            var apiKeyCredential = new ApiKeyCredential(_openAiApiKey);
            // Initialize the memory builder: set up text embedding generation and memory store
            _memoryBuilder = new MemoryBuilder();
            // 初始化記憶體建構器
            _memoryBuilder = new MemoryBuilder();
            _memoryBuilder.WithOpenAITextEmbeddingGeneration(
                modelId: "text-embedding-ada-002",
                apiKey: _openAiApiKey);
            _memoryBuilder.WithMemoryStore(_mongoDBMemoryStore);
            // Build the memory: create the semantic text memory
            _semanticTextMemory = _memoryBuilder.Build();
            _mongoClient = new MongoClient(_connectionString);
        } 



        private async Task FetchAndSaveProductDocuments(ISemanticTextMemory memory, int startIndex, int limitSize)
        {
            List<Course> courses = _courseRepository.GetProductsByPageAsQueryable(startIndex, limitSize).ToList();
            foreach (var course in courses)
            {
                try
                {
                    await memory.SaveInformationAsync(
                        collection: _collectionName,
                        text: course.Title,
                        id: course.CourseId.ToString(),
                        description: course.SubTitle,
                        additionalMetadata: course.Description
                    );
                }
                catch (Exception ex)
                {
                    new Exception( ex.ToString() );
                }
            }
        }

        public async Task FetchAndSaveProductDocumentsAsync(int startIndex, int limitSize)
        {
            // Fetch and save product documents to the semantic text memory
            await FetchAndSaveProductDocuments(_semanticTextMemory, startIndex, limitSize);
        }

        Task IVectorSearchService.FetchAndSaveProductDocuments(ISemanticTextMemory memory, int startIndex, int limitSize)
        {
            throw new NotImplementedException();
        }

        public async Task<List<CourseVectorSearchResultDto>> GetVectorSearchAsync(string userInput)
        {
            var memories = _semanticTextMemory.SearchAsync(_collectionName, userInput, limit: 4, minRelevanceScore: 0.6);

            var result = new List<CourseVectorSearchResultDto>();
            await foreach (var memory in memories)
            {
                var courseVectorSearchResult = new CourseVectorSearchResultDto
                {
                    Id = memory.Metadata.Id,
                    Description = memory.Metadata.Description,
                    Name = memory.Metadata.Text,
                    Relevance = memory.Relevance.ToString("0.00")
                };
                result.Add(courseVectorSearchResult);
            }

            return result;
        }

    }
}

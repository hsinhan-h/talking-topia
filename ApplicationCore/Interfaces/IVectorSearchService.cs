using ApplicationCore.Dtos;
using Microsoft.SemanticKernel.Memory;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationCore.Interfaces
{
    [Experimental("SKEXP0020")]
    public interface IVectorSearchService
    {
       public Task FetchAndSaveProductDocuments(ISemanticTextMemory memory, int startIndex, int limitSize);
       public Task FetchAndSaveProductDocumentsAsync(int startIndex, int limitSize);
       public Task<List<CourseVectorSearchResultDto>> GetVectorSearchAsync(string userInput);
    }
}

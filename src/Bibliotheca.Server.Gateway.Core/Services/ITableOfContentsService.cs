using System.Collections.Generic;
using System.Threading.Tasks;
using Bibliotheca.Server.Gateway.Core.DataTransferObjects;

namespace Bibliotheca.Server.Gateway.Core.Services
{
    public interface ITableOfContentsService
    {
        Task<IList<ChapterItemDto>> GetTableOfConents(string projectId, string branchName);
    }
}
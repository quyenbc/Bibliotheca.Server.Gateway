using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bibliotheca.Server.Depository.Abstractions.DataTransferObjects;
using Bibliotheca.Server.Gateway.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Bibliotheca.Server.Gateway.Api.Controllers
{
    [Authorize]
    [ApiVersion("1.0")]
    [Route("api/projects/{projectId}/branches/{branchName}/documents")]
    public class DocumentsController : Controller
    {
        private readonly IDocumentsService _documentsService;

        private readonly IMarkdownService _markdownService;

        private readonly IBranchesService _branchService;

        public DocumentsController(IDocumentsService documentsService, IMarkdownService markdownService, IBranchesService branchService)
        {
            _documentsService = documentsService;
            _markdownService = markdownService;
            _branchService = branchService;
        }

        [HttpGet]
        public async Task<IList<BaseDocumentDto>> Get(string projectId, string branchName)
        {
            var documents = await _documentsService.GetDocumentsAsync(projectId, branchName);
            return documents;
        }

        [HttpGet("{fileUri}")]
        public async Task<DocumentDto> Get(string projectId, string branchName, string fileUri)
        {
            var document = await _documentsService.GetDocumentAsync(projectId, branchName, fileUri);
            return document;
        }

        [HttpGet("content/{fileUri}")]
        public async Task<FileResult> GetContent(string projectId, string branchName, string fileUri)
        {
            var document = await _documentsService.GetDocumentAsync(projectId, branchName, fileUri);
            byte[] content = document.Content;
            string contentType = document.ConentType;

            if(document.ConentType == "text/markdown")
            {
                var markdown = System.Text.Encoding.UTF8.GetString(document.Content);
                var html = _markdownService.ConvertToHtml(markdown);

                content = System.Text.Encoding.UTF8.GetBytes (html);
                contentType = "text/html";
            }

            return new FileContentResult(content, contentType);
        }

        [HttpPost]
        public async Task<IActionResult> Post(string projectId, string branchName, [FromBody] DocumentDto document)
        {
            await _documentsService.CreateDocumentAsync(projectId, branchName, document);

            document.Content = null;
            return Created($"/projects/{projectId}/branches/{branchName}/documents/{document.Uri}", document);
        }

        [HttpPut("{fileUri}")]
        public async Task<IActionResult> Put(string projectId, string branchName, string fileUri, [FromBody] DocumentDto document)
        {
            await _documentsService.UpdateDocumentAsync(projectId, branchName, fileUri, document);
            return Ok();
        }

        [HttpPut]
        public async Task<IActionResult> Put(string projectId, string branchName, IFormFile file)
        {
            var branches = await _branchService.GetBranchesAsync(projectId);
            if(branches.Any(x => x.Name == branchName))
            {
                await _branchService.DeleteBranchAsync(projectId, branchName);
            }

            await _documentsService.UploadBranchAsync(projectId, branchName, file.OpenReadStream());
            return Ok();
        }

        [HttpPut("filebody")]
        public async Task<IActionResult> Put(string projectId, string branchName)
        {
            var branches = await _branchService.GetBranchesAsync(projectId);
            if(branches.Any(x => x.Name == branchName))
            {
                await _branchService.DeleteBranchAsync(projectId, branchName);
            }
            
            await _documentsService.UploadBranchAsync(projectId, branchName, Request.Body);
            return Ok();
        }

        [HttpDelete("{fileUri}")]
        public async Task<IActionResult> Delete(string projectId, string branchName, string fileUri)
        {
            await _documentsService.DeleteDocumentAsync(projectId, branchName, fileUri);
            return Ok();
        }
    }
}
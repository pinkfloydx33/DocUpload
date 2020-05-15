using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DocumentUpload.Api.Contracts.v1;
using DocumentUpload.Api.Swagger;
using DocumentUpload.Api.Utilities;
using DocumentUpload.Core.Data;
using DocumentUpload.Core.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Annotations;
using DocumentDetails = DocumentUpload.Api.Contracts.v1.DocumentDetails;
using DocumentSearchParameters = DocumentUpload.Core.Data.DocumentSearchParameters;

namespace DocumentUpload.Api.Controllers
{
	[ApiController]
	[ApiVersion("1.0")]
	[Route("api/v{version:apiVersion}/[controller]")]
	public class DocumentsController : ControllerBase
	{
		private readonly ILogger<DocumentsController> _logger;
		private readonly IDocumentRepository _repository;
		private readonly IFileValidator _validator;
		private readonly IFileTypeInfoProvider _fileInfoProvider;

		public DocumentsController(
			IDocumentRepository repository, IFileValidator validator, IFileTypeInfoProvider provider, ILogger<DocumentsController> logger)
		{
			_logger = logger;
			_repository = repository;
			_validator = validator;
			_fileInfoProvider = provider;
		}

		[HttpGet]
		[SwaggerResponse(StatusCodes.Status200OK, "Returns a paginated list of all Documents", typeof(IEnumerable<DocumentDetails>))]
		[ProducesResponseHeader("X-Pagination-Count", StatusCodes.Status200OK, Type = ResponseHeaderType.Numeric)]
		[ProducesResponseHeader("Links", StatusCodes.Status200OK)]
		public async Task<ActionResult<IEnumerable<DocumentDetails>>> GetAll([FromQuery] Contracts.v1.DocumentSearchParameters parameters)
		{
			_logger.LogInformation("Begin GetAll/Search Request");
			try
			{
				var search = new DocumentSearchParameters
				{
					Page = parameters.PageNumber,
					PageSize = parameters.PageSize,
					DocumentTypes = parameters.DocumentTypes?.Select(Mapper.Map).ToList()
				};

				var results = await _repository.ListDocumentsAsync(search, HttpContext.RequestAborted);

				var linkBuilder = new PaginatedLinkBuilder(Url, nameof(GetAll), new { parameters.DocumentTypes }, parameters.PageNumber, parameters.PageSize, results.Total);
				
				Response.Headers.Add("Link", linkBuilder.ToLinkHeaderValues());
				Response.Headers.Add("X-Pagination-Count", results.Total.ToString());

				_logger.LogInformation("Returning {Count}/{Total} Documents found", results.Returned, results.Total);

				var output = results.Documents?.Select(Mapper.Map) ?? Array.Empty<DocumentDetails>();
				
				return Ok(output);

			}
			catch (Exception e)
			{
				_logger.LogError(e, "Error performing GetAll Request");
				return StatusCode(StatusCodes.Status500InternalServerError);
			}
		}

		[HttpGet]
		[Route("{id:long}")]
		[SwaggerResponse(StatusCodes.Status200OK, "Returns the details for a single document", typeof(DocumentDetails))]
		[SwaggerResponse(StatusCodes.Status404NotFound, "A document with the specified ID was not found")]
		public async Task<ActionResult<DocumentDetails>> Get(long id)
		{
			_logger.LogInformation("Begin Get Document {Id}", id);

			try
			{
				var result = await _repository.GetByIdAsync(id, HttpContext.RequestAborted);
				if (result is null)
				{
					_logger.LogWarning("Document {Id} was not found", id);
					return NotFound();
				}

				_logger.LogInformation("Document {Id} Found ({Name}", id, result.FileName);

				return Mapper.Map(result);

			}
			catch (Exception e)
			{
				_logger.LogError(e, "Error performing Get Request for Document {Id}", id);

				return StatusCode(StatusCodes.Status500InternalServerError);
			}
		}

		[HttpGet]
		[Route("{id:long}/download")]
		[SwaggerResponse(StatusCodes.Status404NotFound, "A document with the specified ID was not found or has been deleted")]
		[SwaggerResponse(StatusCodes.Status200OK, "File was found and is attached to response", typeof(FileResult))]
		public async Task<ActionResult> Download(long id)
		{
			_logger.LogInformation("Begin Download of Document {Id}", id);

			try
			{
				var documentInfo = await _repository.GetByIdAsync(id, HttpContext.RequestAborted);
				if (documentInfo is null)
				{
					_logger.LogWarning("Document {Id} was not found", id);
					return NotFound();
				}

				var result = await _repository.GetContentAsync(id, HttpContext.RequestAborted);
				if (result is null || result.Length == 0)
				{
					_logger.LogWarning("Document {Id} was not found or has been deleted", id);
					return NotFound();
				}

				var fileName = documentInfo.FileName;
				var contentType = _fileInfoProvider.GetMimeType(fileName);

				_logger.LogInformation("Found Document {Id} (Name: {Name}, Type: {Type})", id, fileName, documentInfo.DocumentType);

				return File(result, contentType, fileName);

			}
			catch (Exception e)
			{
				_logger.LogError(e, "An error has prevented download of Document {Id}", id);
				return StatusCode(StatusCodes.Status500InternalServerError);
			}
		}

		[HttpDelete]
		[Route("{id:long}")]
		[SwaggerResponse(StatusCodes.Status404NotFound, "The Requested document was not found")]
		[SwaggerResponse(StatusCodes.Status204NoContent, "The DELETE was successful")]
		public async Task<ActionResult> Delete(long id)
		{
			_logger.LogInformation("Begin Delete of Document {Id}", id);

			try
			{
				var deleted = await _repository.DeleteAsync(id, HttpContext.RequestAborted);

				if (!deleted)
				{
					_logger.LogWarning("Document {Id} was not found", id);
					return NotFound();
				}

				_logger.LogInformation("Document {Id} was successfully deletede", id);

				return NoContent();
			}
			catch (Exception e)
			{
				_logger.LogError(e, "An error has prevented deletion of Document {Id}", id);
				return StatusCode(StatusCodes.Status500InternalServerError);
			}
		}


		[HttpPost]
		[SwaggerResponse(StatusCodes.Status400BadRequest, "The File is invalid")]
		[SwaggerResponse(StatusCodes.Status201Created, "The Document was created", typeof(long))]
		[SwaggerResponse(StatusCodes.Status409Conflict, "A Document with this title already exists")]
		public async Task<ActionResult<DocumentDetails>> PostDocument([FromForm] AddDocumentRequest doc, IFormFile file, ApiVersion version) // ApiVersion required for Routing
		{
			_logger.LogInformation("Begin Upload of File {Name}", file.FileName);

			var fileContent = await file.GetFileBytesAsync(HttpContext.RequestAborted);

			if (!_validator.IsValid(file.FileName, fileContent, out var message))
			{
				_logger.LogError("Posted File {Name} is not Valid: {Error}", file.FileName, message);
				return BadRequest(message);
			}

			var details = new DocumentDetailsDto
			{
				Description = doc.Description,
				FileName = file.FileName,
				Owner = doc.Owner,
				Title = doc.Title,
				FileSize = file.Length,
				DocumentType = _fileInfoProvider.GetDocumentType(file.FileName)
			};

			try
			{

				var result = await _repository.InsertAsync(details, fileContent, HttpContext.RequestAborted);
				if (result <= 0)
				{
					_logger.LogError("Unable to upload Document {Name} ({Title}). A document with this title already exists.", details.FileName, details.Title);
					return Conflict("A document with this title already exists");
				}

				_logger.LogInformation("Document {Name} was uploaded as {Id} ({Title})", details.FileName, result, details.Title);

				return CreatedAtAction(nameof(Get), new { version = version.ToString(), id = result }, result);

			}
			catch (Exception e)
			{
				_logger.LogError(e, "An error has prevented upload of Document {Name}", details.FileName);
				return StatusCode(StatusCodes.Status500InternalServerError);
			}

		}

	}
}

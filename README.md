# Document Upload Service

This is a *basic* document upload service. The service allows uploading, listing (primitive search), download and deletion of documents. The following endpoints are defined by the API:

| Endpoint                       | Verb     | Description                                                     |
| -------------------------------| :------: | --------------------------------------------------------------- |
| `api/v1/documents`             | `GET`    | Lists all documents (paginaated) with basic search capabilities |
| `api/v1/documents/id`          | `GET`    | Gets a document by it's identifier                              |
| `api/v1/documents/id/download` | `GET`    | Downloads a document                                            |
| `api/v1/documents`             | `DELETE` | Delete a document by it's identifier                            |
| `api/v1/documents`             | `POST`   | Upload a new document into the system                           |
| `swagger`					     | N/A      | Swagger endpoint for testing                                    |


## Architecture and Features

The project is split into the following components:

| Component                     | Description                                    |
| ----------------------------- | ---------------------------------------------- |
| DocumentUpload.Core           | Shared interfaces, enum, classes and utilities |
| DocumentUpload.Services       | Business-layer implementation of services      |
| DocumentUpload.API.Contracts  | API Contracts/models (versioned)               |
| DocumentUpload.API            | Main application / API                         |
| DocumentUpload.SQL            | SQL Server Data Project (DACPAC)               | 
| DocumentUpload.API.Tests      | API Endpoint Tests / Utility Tests             |
| DocumentUpload.Services.Tests | Business-layer tests

The database schema is basic: a table for storing document information and a second table for storing the content as `VARBINARY(MAX)`. Queries against the `Documents` table will not be impacted 
by the storage of binary data. This split-out will also make it easier to allow versioned document contents as well as provides the opportunity to store data in alternate 
locations (file based, Azure Blob storage, etc). Access to the database is performed using a repository-type pattern with Dapper. A _generic repository_ (ie. `IRepository<T>` ) was not 
used partially due to the simplistic nature of the project, but also due to the denormalized storage (`Contents` is not part of the `DocumentDetails` model but is rather retrieved by a `GetContent` 
method on the `IDocumentRepository` interface). The _generic repository_ was also avoided as it fits better when the backing store can offer an `IQueryable` (which is not included here).

The `DocumentRepository` allows _optionally_ specifying Page/Size. This means that _direct_ usage of the repository allows querying for _all_ results. It would have been an unfair/illogical 
limitation placed on the repository itself. To disallow users from requesting the _entire_ listing of documents (which could grow to millions!), the `GET[All]` endpoint of the API  enforces a maximum 
page size (currently hard-coded). The endpoint accepts optional `pageSize` and `pageNumber` query parameters; the total number of available documents is returned in the response via the `X-Pagination-Count` 
header with first/last/next/previous URIs provided via the `Link` header. 

When files are uploaded, the user must specify a `Title` for the document. This information is required to be _unique_ (for the user) and is not impacted by/related to the file name. Duplicate 
`file names` are allowed. The user may specify an optional `Description` for their document. If no description is specified, the implementation delegates to an `IDescriptionGenerator` 
(retrieved based on document type from an `IDescriptionGeneratorFactory`) which will inspect the file and provide a short description (image dimensions, first line of text, etc). During upload,  
the `IFileValidator` is used to verify the file falls within the current constraints of the system (see below).

The `IFileTypeInfoProvider` interface describes a set of services that can provide meta-information about a specified file/document. When files are downloaded, this interface is queryed for 
the appropriate MIME Type/content-type headers. When uploading, an implementation of this interface translates a file name into the appropriate `DocumentType` (image, text, PDF, unknown).

## Limitations

The system currently only allows a specific set of file extensions to be uploaded and files must be smaller than a certain size. These settings are configurable via the `appsettings.json` 
file. The system can be side-stepped by renaming the local file (todo-list item).

Document titles must be unique per-user; uploading a second version of the file currently requires the user to specifiy an alternate name. Duplicate titles are not _currently_ guarded 
against directly; instead a `SQLException` with Error Number `2627` is caught, translating insertion failures to a value of `0` (`Insert` returns the new identity). This was chosen due to 
the simplistic nature of the task. The project can/should be updated to query for duplicates first; alternatively we can remove the limitation around duplicates (TODO list)

## TODO

- More Tests!
- Support File Editing
  - Versioning of files
- User/Authorization-based access
  - Row-Level Security on tables
  - `Get`, `GetAll`, `Delete` and `Download` limited by user-access
  - `Owner` should be determined by Authorization, not manually part of the payload
- Improved File Handling
  - File Compression
  - Stream Large files
- Better duplicate handling
  - Remove restriction or query for duplicates first
- Use AutoMapper instead of manual mapping
- Support more file types
  - Dynamically add new types
  - Support classification-determination by type
- Alternate-storage mechanisms
  - Azure blob storage, etc.
- Composable Validation
  - Rewrite validator to return a `Validation`-type object
  - Support different types of validation based on document type
  - Validate by file `header`/magic bytes rather than extension-based
- Tagging/Categories
- More Search options
  - Title
  - Category (future)
  - Tags (future)
- Better SQL Generation
  - Switch to DapperContrib ? 
  - Provide an `IQueryable<T>` and query-translation

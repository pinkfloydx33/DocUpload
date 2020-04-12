using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using Dapper;
using DocumentUpload.Core;
using DocumentUpload.Core.Data;
using DocumentUpload.Core.Services;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace DocumentUpload.Services.Data
{
	public class DocumentRepository : IDocumentRepository
	{
		private const int SqlErrorCodeUniqueViolation = 2627;
        
        private readonly DataContextOptions _options;
        private readonly IDescriptionGeneratorFactory _descriptionFactory;

        public DocumentRepository(IOptions<DataContextOptions> options, IDescriptionGeneratorFactory descriptionFactory)
        {
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
            _descriptionFactory = descriptionFactory;
        }

        public async Task<DocumentDetailList> ListDocumentsAsync(DocumentSearchParameters search, CancellationToken token)
		{

			var parameters = new DynamicParameters();
			var (whereClause, offsetclause) = ExtractSearchClauses(search, parameters);

			var queries = $@"SELECT COUNT_BIG(1) FROM [Documents] {whereClause};
SELECT [DocumentId], [FileName], [Title], [Description], [CreateDate], [FileSize], [DocumentType], [Owner]
FROM [dbo].[Documents]
{whereClause}
ORDER BY [DocumentId]
{offsetclause}
;";

			await new SyncContextRemover();
			await using var conn = await CreateAndOpenConnection(token);

			var definition = CreateCommand(queries, parameters, token);
			
			using var reader = await conn.QueryMultipleAsync(definition);
			var total = await reader.ReadSingleOrDefaultAsync<long>();
			var items = await reader.ReadAsync<DocumentDetailsDto>();

			return new DocumentDetailList
			{
				Total = (uint) total,
				Documents = items.ToList()
			};

		}

		public async Task<DocumentDetailsDto> GetByIdAsync(long id, CancellationToken token)
		{
			await new SyncContextRemover();

			const string query = @"
SELECT [DocumentId], [FileName], [Title], [Description], [CreateDate], [FileSize], [DocumentType], [Owner]
FROM [dbo].[Documents]
WHERE DocumentId = @id;
";

			var definition = new CommandDefinition(query, new { id }, cancellationToken: token);

			await using var conn = await CreateAndOpenConnection(token);

			return await conn.QuerySingleOrDefaultAsync<DocumentDetailsDto>(definition);
		}

		public async Task<byte[]> GetContentAsync(long id, CancellationToken token)
		{
			await new SyncContextRemover();

			const string query = "SELECT [Content] FROM [dbo].[DocumentContent] WHERE [DocumentId] = @id";
			var definition = CreateCommand(query, new { id }, token);

			await using var conn = await CreateAndOpenConnection(token);

			return await conn.ExecuteScalarAsync<byte[]>(definition);
		}

		public async Task<bool> DeleteAsync(long id, CancellationToken token)
		{
			
			await new SyncContextRemover();

			const string query = "DELETE FROM [dbo].[Documents] WHERE [DocumentId] = @id; SELECT @@ROWCOUNT AS [Deleted];";

			var definition = CreateCommand(query, new { id }, token);

			await using var conn = await CreateAndOpenConnection(token);
			var countDeleted = await conn.ExecuteScalarAsync<long>(definition);

			return countDeleted != 0;

		}


		public Task<long> InsertAsync(DocumentDetailsDto details, byte[] dataBytes, CancellationToken token)
		{
			if (details is null)
				throw new ArgumentNullException(nameof(details));

			if (dataBytes is null)
				throw new ArgumentNullException(nameof(dataBytes));

			return Impl();

			async Task<long> Impl()
			{

				await new SyncContextRemover();

                if (string.IsNullOrWhiteSpace(details.Description))
                    details.Description = await GetFileDescription(details.DocumentType, dataBytes);

				using var scope = new TransactionScope(TransactionScopeOption.RequiresNew, TransactionScopeAsyncFlowOption.Enabled);

				await using var conn = await CreateAndOpenConnection(token);

				const string insertDetailsQuery = @"
INSERT INTO [dbo].[Documents] ([FileName], [Title], [Description], [FileSize], [DocumentType], [Owner])
VALUES (@FileName, @Title, @Description, @FileSize, @DocumentType, @Owner);
SELECT SCOPE_IDENTITY() AS ID;
";

				var insertDetailsDefinition = CreateCommand(insertDetailsQuery, details, token);
				long newId;

				try
				{
					newId = await conn.ExecuteScalarAsync<long>(insertDetailsDefinition);
				}
				// catching UniqueConstraint Violation -- just for demo
				catch (SqlException s) when (s.Number == SqlErrorCodeUniqueViolation)
				{
					newId = 0;
				}

				// should probably throw an exception
				if (newId <= 0)
					return 0; // rolls back scope


				const string insertDataQuery = @"
INSERT INTO [dbo].[DocumentContent] ([DocumentId], [Content])
VALUES (@Id, @Content)
";

				var insertDataDefinition = CreateCommand(insertDataQuery, new { Id = newId, Content = dataBytes }, token);
				await conn.ExecuteAsync(insertDataDefinition);

				scope.Complete();

				return newId;

			}
		}

        internal async ValueTask<string> GetFileDescription(DocumentType docType, byte[] dataBytes)
        {
            var generator = _descriptionFactory.GetGenerator(docType);
            return await generator.GetDescriptionAsync(dataBytes);
        }
		
		// internal for testing
		internal static (string where, string pagination) ExtractSearchClauses(DocumentSearchParameters search, DynamicParameters parameters)
		{
			if (!(search is {} p)) 
				return (null, null);

			string whereClause = null;
			string offsetClause = null;

			if (p.DocumentTypes?.Count > 0)
			{
				whereClause = "WHERE [DocumentType] IN @DocTypes";
				parameters.Add("DocTypes", p.DocumentTypes);
			}

			if (p.Page.HasValue || p.PageSize.HasValue)
			{
				var take = p.PageSize.GetValueOrDefault();
				if (take <= 0)
					take = 1;

				var page = p.Page.GetValueOrDefault() - 1;
				if (page < 0)
					page = 0;

				parameters.Add("@Skip", page * take);
				parameters.Add("@Take", take);

				offsetClause = "OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY";
			}

			return (whereClause, offsetClause);
		}

		private async Task<SqlConnection> CreateAndOpenConnection(CancellationToken token)
		{
			var conn = new SqlConnection(_options.ConnectionString);
			await conn.OpenAsync(token);

			return conn;
		}

		private static CommandDefinition CreateCommand(string query, object param, CancellationToken token)
			=> new CommandDefinition(query, param, cancellationToken: token);

	}
}
using System.Threading;
using System.Threading.Tasks;

namespace DocumentUpload.Core.Data
{
	public interface IDocumentRepository
	{
		Task<DocumentDetailList> ListDocumentsAsync(DocumentSearchParameters search, CancellationToken token);
		Task<DocumentDetailsDto> GetByIdAsync(long id, CancellationToken token);
		Task<byte[]> GetContentAsync(long id, CancellationToken token);
		Task<bool> DeleteAsync(long id, CancellationToken token);
		Task<long> InsertAsync(DocumentDetailsDto details, byte[] dataBytes, CancellationToken token);

	}
}
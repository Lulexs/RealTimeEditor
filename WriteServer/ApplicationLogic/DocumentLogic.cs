using ApplicationLogic.Dtos;
using ApplicationLogic.Exceptions;
using Persistence.DocumentRepository;

namespace ApplicationLogic;

public class DocumentLogic {
    private readonly DocumentRepositoryCassandra _docRepoCass;
    private readonly DocumentRepositoryRedis _docRepoRed;

    public DocumentLogic(DocumentRepositoryCassandra docRepoCass, DocumentRepositoryRedis docRepoRed) {
        _docRepoCass = docRepoCass;
        _docRepoRed = docRepoRed;
    }

    public async Task ChangeDocumentName(ChangeDocumentNameDto dto) {
        var documentExists = await _docRepoCass.VerifyExistsAsync(dto.WorkspaceId, dto.DocumentId);
        if (!documentExists) {
            throw new DocumentNotFoundException($"Document {dto.DocumentId} not found");
        }
        await _docRepoRed.ChangeDocumentName(dto.WorkspaceId, dto.DocumentId, dto.NewName);
    }
}
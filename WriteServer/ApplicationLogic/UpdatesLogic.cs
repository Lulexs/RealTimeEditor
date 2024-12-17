using Models;
using Persistence.DocumentRepository;

namespace ApplicationLogic;

public class UpdatesLogic {

    private DocumentRepositoryCassandra _docRepoCass;
    private DocumentRepositoryRedis _docRepoRed;

    public UpdatesLogic(DocumentRepositoryCassandra docRepoCass, DocumentRepositoryRedis docRepoRed) {
        _docRepoCass = docRepoCass;
        _docRepoRed = docRepoRed;
    }

    public bool VerifyExists(Guid workspaceId, Guid docId) {
        // return _docRepoCass.GetDocument(workspaceId, docId) != null;
        return true;
    }

}
namespace ApplicationLogic;

public class Class1
{

    // Prima update od API-ja, upisuje u Redis Pub/Sub
    // Update - koji tip podatka?
    public void WriteUpdate()
    {

    }

    // Vraća ceo dokument (čita sve update-ove do sada)
    public void ReadDocument(GUID WorkspaceId, GUID DocumentId)
    {

    }

    // Prima sve update-ove, merguje ih i upisuje u Redis cache
    public void CacheDocument(GUID WorkspaceId, GUID DocumentId)
    {

    }


}

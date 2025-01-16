namespace ApplicationLogic.Exceptions;

public class DocumentNotFoundException : Exception {
    public DocumentNotFoundException() : base("Document not found") { }
    public DocumentNotFoundException(string message) : base(message) { }
    public DocumentNotFoundException(string message, Exception inner) : base(message, inner) { }
}
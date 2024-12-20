namespace ApplicationLogic.Exceptions;

public class CantLoadDocumentContentException : Exception {
    public CantLoadDocumentContentException() : base("Couldn't load document content") { }
    public CantLoadDocumentContentException(string message) : base(message) { }
    public CantLoadDocumentContentException(string message, Exception innerException) : base(message, innerException) { }
}
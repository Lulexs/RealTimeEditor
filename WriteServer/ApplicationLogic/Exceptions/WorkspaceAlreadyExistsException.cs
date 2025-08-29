namespace ApplicationLogic.Exceptions;

public class WorkspaceAlreadyExistsException : Exception {
    public WorkspaceAlreadyExistsException() : base("Workspace already exists") { }
    public WorkspaceAlreadyExistsException(string message) : base(message) { }
    public WorkspaceAlreadyExistsException(string message, Exception inner) : base(message, inner) { }
}
namespace ApplicationLogic.Exceptions;

public class WorkspaceNotFoundException : Exception {
    public WorkspaceNotFoundException() : base("Workspace not found") { }
    public WorkspaceNotFoundException(string message) : base(message) { }
    public WorkspaceNotFoundException(string message, Exception inner) : base(message, inner) { }
}
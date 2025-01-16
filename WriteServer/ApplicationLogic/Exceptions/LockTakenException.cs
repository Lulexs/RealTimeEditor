namespace ApplicationLogic.Exceptions;

public class LockTakenException : Exception {
    public LockTakenException() : base("Cannot acquire requested lock") { }
    public LockTakenException(string message) : base(message) { }
    public LockTakenException(string message, Exception inner) : base(message, inner) { }
}
namespace ApplicationLogic.Exceptions;

public class FailedToQueueUpdateException : Exception {
    public FailedToQueueUpdateException() : base("Couldn't enqueue update for persisting") { }
    public FailedToQueueUpdateException(string message) : base(message) { }
    public FailedToQueueUpdateException(string message, Exception innerException) : base(message, innerException) { }
}
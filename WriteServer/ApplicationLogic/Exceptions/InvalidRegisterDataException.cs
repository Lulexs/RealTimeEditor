namespace ApplicationLogic.Exceptions;

public class InvalidRegisterDataException : Exception {
    public InvalidRegisterDataException() : base("Invalid registration data provided.") { }
    public InvalidRegisterDataException(string message) : base(message) { }
    public InvalidRegisterDataException(string message, Exception innerException) : base(message, innerException) { }
}
namespace ApplicationLogic.Exceptions
{
    public class InvalidUserPasswordException : Exception
    {
        public InvalidUserPasswordException() : base("Invalid user password.") { }
        public InvalidUserPasswordException(string message) : base(message) { }
        public InvalidUserPasswordException(string message, Exception innerException) : base(message, innerException) { }
    }
}
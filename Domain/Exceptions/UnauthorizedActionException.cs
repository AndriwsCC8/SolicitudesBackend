namespace Domain.Exceptions
{
    public class UnauthorizedActionException : Exception
    {
        public UnauthorizedActionException(string message) : base(message)
        {
        }

        public UnauthorizedActionException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}

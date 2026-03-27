namespace TaskManager.API.Exceptions;

public class UserAlreadyExistsException : Exception
{
    public UserAlreadyExistsException(string email) 
        : base($"A user with email '{email}' already exists. Please use a different email or try logging in.")
    {
    }
}

public class InvalidCredentialsException : Exception
{
    public InvalidCredentialsException() 
        : base("Invalid email or password. Please check your credentials and try again.")
    {
    }
}

public class ValidationException : Exception
{
    public ValidationException(string message) : base(message)
    {
    }
}

public class UnauthorizedException : Exception
{
    public UnauthorizedException(string message = "You are not authorized to perform this action.") 
        : base(message)
    {
    }
}
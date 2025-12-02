namespace Promptino.Core.Exceptions;

public class PromptNotFoundExceptions : Exception
{
    public PromptNotFoundExceptions() : base()
    { }

    public PromptNotFoundExceptions(string message) : base(message)
    { }

    public PromptNotFoundExceptions(string message, Exception? innerException) : base(message, innerException)
    { }
}

public class PromptExistsException : Exception
{
    public PromptExistsException() : base()
    { }

    public PromptExistsException(string message) : base(message)
    { }

    public PromptExistsException(string message, Exception? innerException) : base(message, innerException)
    { }

}

public class ImageLimitException : Exception
{
    public ImageLimitException() : base()
    { }

    public ImageLimitException(string message) : base(message)
    { }

    public ImageLimitException(string message, Exception? innerException) : base(message, innerException)
    { }

}
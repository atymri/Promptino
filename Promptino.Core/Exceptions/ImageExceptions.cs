namespace Promptino.Core.Exceptions;

public class ImageNotFoundException : Exception
{
    public ImageNotFoundException() : base()
    { }

    public ImageNotFoundException(string message) : base(message)
    { }

    public ImageNotFoundException(string message, Exception? innerException) : base(message, innerException)
    { }
}

public class ImageExistsException : Exception
{
    public ImageExistsException() : base()
    { }

    public ImageExistsException(string message) : base(message)
    { }

    public ImageExistsException(string message, Exception? innerException) : base(message, innerException)
    { }
}
namespace Promptino.Core.Exceptions;

public class CategoryNotFoundException : Exception
{
    public CategoryNotFoundException() : base()
    { }

    public CategoryNotFoundException(string message) : base(message)
    { }

    public CategoryNotFoundException(string message, Exception? innerException) : base(message, innerException)
    { }
}

public class CategoryExistsException : Exception
{
    public CategoryExistsException() : base()
    { }

    public CategoryExistsException(string message) : base(message)
    { }

    public CategoryExistsException(string message, Exception? innerException) : base(message, innerException)
    { }
}

public class NullCategoryRequestException : Exception
{
    public NullCategoryRequestException() : base()
    { }

    public NullCategoryRequestException(string message) : base(message)
    { }

    public NullCategoryRequestException(string message, Exception? innerException) : base(message, innerException)
    { }
}


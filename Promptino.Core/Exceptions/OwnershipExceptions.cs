namespace Promptino.Core.Exceptions;

public class PromptOwnershipException : Exception
{
    public PromptOwnershipException() : base() { }
    public PromptOwnershipException(string message) : base(message) { }
    public PromptOwnershipException(string message, Exception? innerException) : base(message, innerException) { }
}

public class CommentOwnershipException : Exception
{
    public CommentOwnershipException() : base() { }
    public CommentOwnershipException(string message) : base(message) { }
    public CommentOwnershipException(string message, Exception? innerException) : base(message, innerException) { }
}

public class CommentNotFoundException : Exception
{
    public CommentNotFoundException() : base() { }
    public CommentNotFoundException(string message) : base(message) { }
    public CommentNotFoundException(string message, Exception? innerException) : base(message, innerException) { }
}

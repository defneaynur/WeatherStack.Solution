namespace Core.Framework.Exceptions;

public class CoreNotificationException : Exception
{
    public string? Type { get; set; }

    public CoreNotificationException(string message)
        : base(message)
    {
    }

    public CoreNotificationException(string message, Exception error)
        : base(message, error)
    {
    }

    public CoreNotificationException(string message, string? type)
        : base(message)
    {
        Type = type;
    }

    public CoreNotificationException(string message, string? type, Exception error)
        : base(message, error)
    {
        Type = type;
    }
}
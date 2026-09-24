namespace ContactManager.Core.Exceptions;

public class NoRecordFoundException : Exception
{
    public NoRecordFoundException(string message = "Kein Datensatz gefunden.") : base(message)
    {
    }
}

using System.Runtime.Serialization;

namespace AI.YouTubeSummarizer;
[Serializable]
internal class EndException : Exception
{
    public EndException()
    {
    }

    public EndException(string? message) : base(message)
    {
    }

    public EndException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    protected EndException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}
public class QueuedRequest
{
    public QueuedRequestType Type;
    public string Json;

    public QueuedRequest(QueuedRequestType type, string json)
    {
        Type = type;
        Json = json;
    }
}
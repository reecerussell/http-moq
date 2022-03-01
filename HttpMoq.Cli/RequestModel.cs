using System.Net;

internal class RequestModel
{
    public string Path { get; set; }
    public string Method { get; set; } = "GET";
    public uint? Limit { get; set; }
    public ResponseModel Response { get; set; } = new();
}

internal class ResponseModel
{
    public HttpStatusCode StatusCode { get; set; } = HttpStatusCode.OK;
    public string? Text { get; set; }
    public object? Json { get; set; }
}

namespace WebApi.DTOs;

public class ApiError
{
    public required string Code { get; set; }
    public required string Message { get; set; }
    public int Status { get; set; }
    public string? TraceId { get; set; }
    public Dictionary<string, object?>? Details { get; set; }
}

using Application.Interfaces;

using Microsoft.AspNetCore.Http;

namespace WebApi.Services;

public class HttpCurrentSensor(IHttpContextAccessor httpContextAccessor) : ICurrentSensor
{
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    public bool IsAuthenticated => SensorId.HasValue;

    public int? SensorId => _httpContextAccessor.HttpContext?.Items["SensorId"] as int?;
}

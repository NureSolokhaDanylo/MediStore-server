using Application.Interfaces;
using Domain.Models;
using Microsoft.AspNetCore.Mvc;
using WebApi.DTOs;
using WebApi.Mappers;

namespace WebApi.Controllers;

[ApiController]
[Route("api/v1/alerts")]
public class AlertsController : ReadController<Alert, AlertDto, IAlertService>
{
    public AlertsController(IAlertService service) : base(service) { }

    protected override AlertDto ToDto(Alert entity) => entity.ToDto();
    protected override int GetId(AlertDto dto) => dto.Id;
}

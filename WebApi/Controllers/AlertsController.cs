using Application.Interfaces;
using Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.DTOs;
using WebApi.Mappers;

namespace WebApi.Controllers;

[ApiController]
[Route("api/v1/alerts")]
public class AlertsController : ReadController<Alert, AlertDto, IAlertService>
{
    private readonly IAlertService _alertService;

    public AlertsController(IAlertService service) : base(service) { _alertService = service; }

    protected override AlertDto ToDto(Alert entity) => entity.ToDto();
    protected override int GetId(AlertDto dto) => dto.Id;

    [Authorize(Roles = "Admin,Operator,Observer")]
    public override Task<ActionResult<IEnumerable<AlertDto>>> GetAll() => base.GetAll();

    [Authorize(Roles = "Admin,Operator,Observer")]
    public override Task<ActionResult<AlertDto>> Get(int id) => base.Get(id);
}

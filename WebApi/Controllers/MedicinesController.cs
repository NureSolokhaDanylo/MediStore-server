using Application.Interfaces;

using Domain.Models;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using WebApi.DTOs;
using WebApi.Mappers;

namespace WebApi.Controllers;

[ApiController]
[Route("api/v1/medicines")]
public class MedicinesController : CrudController<Medicine, MedicineDto, MedicineCreateDto, IMedicineService>
{
    public MedicinesController(IMedicineService service) : base(service) { }

    protected override MedicineDto ToDto(Medicine entity) => entity.ToDto();
    protected override Medicine ToEntity(MedicineDto dto) => dto.ToEntity();
    protected override Medicine ToEntity(MedicineCreateDto dto) => dto.ToEntity();
    protected override int GetId(MedicineDto dto) => dto.Id;

    [Authorize(Roles = "Admin,Operator,Observer")]
    public override Task<ActionResult<IEnumerable<MedicineDto>>> GetAll() => base.GetAll();

    [Authorize(Roles = "Admin,Operator,Observer")]
    public override Task<ActionResult<MedicineDto>> Get(int id) => base.Get(id);

    [Authorize(Roles = "Admin")]
    public override Task<IActionResult> Create([FromBody] MedicineCreateDto dto) => base.Create(dto);

    [Authorize(Roles = "Admin")]
    public override Task<ActionResult<MedicineDto>> Update([FromBody] MedicineDto dto) => base.Update(dto);

    [Authorize(Roles = "Admin")]
    public override Task<IActionResult> Delete(int id) => base.Delete(id);
}

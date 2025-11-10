using Application.Interfaces;

using Domain.Models;

using Microsoft.AspNetCore.Mvc;

using WebApi.DTOs;
using WebApi.Mappers;

namespace WebApi.Controllers;

[ApiController]
[Route("api/v1/medicines")]
public class MedicinesController : CrudController<Medicine, MedicineDto, IMedicineService>
{
    public MedicinesController(IMedicineService service) : base(service) { }

    protected override MedicineDto ToDto(Medicine entity) => entity.ToDto();
    protected override Medicine ToEntity(MedicineDto dto) => dto.ToEntity();
    protected override int GetId(MedicineDto dto) => dto.Id;
}

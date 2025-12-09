using Domain.Models;

using WebApi.DTOs;

namespace WebApi.Mappers;

public static class DtoMappers
{
    // Medicine
    public static MedicineDto ToDto(this Medicine m) => new()
    {
        Id = m.Id,
        Name = m.Name,
        Description = m.Description,
        TempMax = m.TempMax,
        TempMin = m.TempMin,
        HumidMax = m.HumidMax,
        HumidMin = m.HumidMin
    };

    public static Medicine ToEntity(this MedicineDto dto) => new()
    {
        Id = dto.Id,
        Name = dto.Name,
        Description = dto.Description,
        TempMax = dto.TempMax,
        TempMin = dto.TempMin,
        HumidMax = dto.HumidMax,
        HumidMin = dto.HumidMin
    };

    // Create DTO -> Entity
    public static Medicine ToEntity(this MedicineCreateDto dto) => new()
    {
        Name = dto.Name,
        Description = dto.Description,
        TempMax = dto.TempMax,
        TempMin = dto.TempMin,
        HumidMax = dto.HumidMax,
        HumidMin = dto.HumidMin
    };

    // Batch
    public static BatchDto ToDto(this Batch b) => new()
    {
        Id = b.Id,
        BatchNumber = b.BatchNumber,
        Quantity = b.Quantity,
        ExpireDate = b.ExpireDate,
        DateAdded = b.DateAdded,
        MedicineId = b.MedicineId,
        ZoneId = b.ZoneId
    };

    public static Batch ToEntity(this BatchDto dto) => new()
    {
        Id = dto.Id,
        BatchNumber = dto.BatchNumber,
        Quantity = dto.Quantity,
        ExpireDate = dto.ExpireDate,
        DateAdded = dto.DateAdded,
        MedicineId = dto.MedicineId,
        ZoneId = dto.ZoneId
    };

    public static Batch ToEntity(this BatchCreateDto dto) => new()
    {
        BatchNumber = dto.BatchNumber,
        Quantity = dto.Quantity,
        ExpireDate = dto.ExpireDate,
        DateAdded = dto.DateAdded,
        MedicineId = dto.MedicineId,
        ZoneId = dto.ZoneId
    };

    // Sensor
    public static SensorDto ToDto(this Sensor s) => new()
    {
        Id = s.Id,
        SerialNumber = s.SerialNumber,
        LastValue = s.LastValue,
        LastUpdate = s.LastUpdate,
        IsOn = s.IsOn,
        SensorType = s.SensorType,
        ZoneId = s.ZoneId
    };

    public static Sensor ToEntity(this SensorDto dto) => new()
    {
        Id = dto.Id,
        SerialNumber = dto.SerialNumber,
        LastValue = dto.LastValue,
        LastUpdate = dto.LastUpdate,
        IsOn = dto.IsOn,
        SensorType = dto.SensorType,
        ZoneId = dto.ZoneId
    };

    public static Sensor ToEntity(this SensorCreateDto dto) => new()
    {
        SerialNumber = dto.SerialNumber,
        LastValue = dto.LastValue,
        LastUpdate = dto.LastUpdate,
        IsOn = dto.IsOn,
        SensorType = dto.SensorType,
        ZoneId = dto.ZoneId
    };

    // Zone
    public static ZoneDto ToDto(this Zone z) => new()
    {
        Id = z.Id,
        Name = z.Name,
        Description = z.Description,
        TempMax = z.TempMax,
        TempMin = z.TempMin,
        HumidMax = z.HumidMax,
        HumidMin = z.HumidMin
    };

    public static Zone ToEntity(this ZoneDto dto) => new()
    {
        Id = dto.Id,
        Name = dto.Name,
        Description = dto.Description,
        TempMax = dto.TempMax,
        TempMin = dto.TempMin,
        HumidMax = dto.HumidMax,
        HumidMin = dto.HumidMin
    };

    public static Zone ToEntity(this ZoneCreateDto dto) => new()
    {
        Name = dto.Name,
        Description = dto.Description,
        TempMax = dto.TempMax,
        TempMin = dto.TempMin,
        HumidMax = dto.HumidMax,
        HumidMin = dto.HumidMin
    };

    // Reading
    public static ReadingDto ToDto(this Reading r) => new()
    {
        Id = r.Id,
        TimeStamp = r.TimeStamp,
        Value = r.Value,
        SensorId = r.SensorId
    };

    public static Reading ToEntity(this ReadingDto dto) => new()
    {
        Id = dto.Id,
        TimeStamp = dto.TimeStamp,
        Value = dto.Value,
        SensorId = dto.SensorId
    };

    public static Reading ToEntity(this ReadingCreateDto dto) => new()
    {
        TimeStamp = dto.TimeStamp,
        Value = dto.Value,
        SensorId = dto.SensorId
    };

    // Collections
    public static IEnumerable<MedicineDto> ToDto(this IEnumerable<Medicine> list) => list.Select(x => x.ToDto());
    public static IEnumerable<BatchDto> ToDto(this IEnumerable<Batch> list) => list.Select(x => x.ToDto());
    public static IEnumerable<SensorDto> ToDto(this IEnumerable<Sensor> list) => list.Select(x => x.ToDto());
    public static IEnumerable<ZoneDto> ToDto(this IEnumerable<Zone> list) => list.Select(x => x.ToDto());
    public static IEnumerable<ReadingDto> ToDto(this IEnumerable<Reading> list) => list.Select(x => x.ToDto());
}

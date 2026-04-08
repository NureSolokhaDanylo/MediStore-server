using Application.Interfaces;
using Application.Results.Base;

using Domain.Models;

using Infrastructure.Interfaces;

namespace Application.Services;

public class MedicineService : IMedicineService
{
    private readonly IReadOnlyService<Medicine> _readService;
    private readonly ICreateService<Medicine> _createService;
    private readonly IUpdateService<Medicine> _updateService;
    private readonly IDeleteService<Medicine> _deleteService;
    private readonly IMedicineRepository _medicineRepository;
    private readonly IRepository<Medicine> _repository;

    public MedicineService(
        IReadOnlyService<Medicine> readService,
        ICreateService<Medicine> createService,
        IUpdateService<Medicine> updateService,
        IDeleteService<Medicine> deleteService,
        IMedicineRepository repository)
    {
        _readService = readService;
        _createService = createService;
        _updateService = updateService;
        _deleteService = deleteService;
        _medicineRepository = repository;
        _repository = repository;
    }

    private Result Validate(Medicine m)
    {
        if (m.TempMin < -50 || m.TempMin > 100)
            return Result.Failure(Errors.Validation(ErrorCodes.Medicine.ValidationFailed, "TempMin must be between -50 and 100", "tempMin"));

        if (m.TempMax < -50 || m.TempMax > 100)
            return Result.Failure(Errors.Validation(ErrorCodes.Medicine.ValidationFailed, "TempMax must be between -50 and 100", "tempMax"));

        if (m.TempMin > m.TempMax)
            return Result.Failure(Errors.Validation(ErrorCodes.Medicine.ValidationFailed, "TempMin cannot be greater than TempMax", "tempMin"));

        if (m.HumidMin < 0 || m.HumidMin > 100)
            return Result.Failure(Errors.Validation(ErrorCodes.Medicine.ValidationFailed, "HumidMin must be between 0 and 100", "humidMin"));

        if (m.HumidMax < 0 || m.HumidMax > 100)
            return Result.Failure(Errors.Validation(ErrorCodes.Medicine.ValidationFailed, "HumidMax must be between 0 and 100", "humidMax"));

        if (m.HumidMin > m.HumidMax)
            return Result.Failure(Errors.Validation(ErrorCodes.Medicine.ValidationFailed, "HumidMin cannot be greater than HumidMax", "humidMin"));

        return Result.Success();
    }

    public async Task<Result<Medicine>> Add(string userId, Medicine entity)
    {
        var check = Validate(entity);
        if (!check.IsSucceed)
            return Result<Medicine>.Failure(check.Error!);

        return await _createService.Add(userId, entity);
    }

    public async Task<Result<Medicine>> Update(string userId, Medicine entity)
    {
        var existing = await _repository.GetAsync(entity.Id);
        if (existing is null)
            return Result<Medicine>.Failure(Errors.NotFound(ErrorCodes.Medicine.NotFound, "Not found", "medicineId", entity.Id));

        var check = Validate(entity);
        if (!check.IsSucceed)
            return Result<Medicine>.Failure(check.Error!);

        return await _updateService.Update(userId, entity);
    }

    public Task<Result<Medicine>> Get(int id) => _readService.Get(id);

    public Task<Result<IEnumerable<Medicine>>> GetAll() => _readService.GetAll();

    public Task<Result> Delete(string userId, int id) => _deleteService.Delete(userId, id);

    public async Task<Result<(IEnumerable<Medicine> items, int totalCount)>> Search(string userId, string query, int limit, int offset)
    {
        if (limit <= 0)
            return Result<(IEnumerable<Medicine>, int)>.Failure(PagingErrors.InvalidLimit(ErrorCodes.Medicine.InvalidSearchPaging));

        if (offset < 0)
            return Result<(IEnumerable<Medicine>, int)>.Failure(PagingErrors.InvalidOffset(ErrorCodes.Medicine.InvalidSearchPaging));

        var (items, totalCount) = await _medicineRepository.SearchAsync(query?.Trim() ?? "", limit, offset);
        return Result<(IEnumerable<Medicine>, int)>.Success((items, totalCount));
    }

}

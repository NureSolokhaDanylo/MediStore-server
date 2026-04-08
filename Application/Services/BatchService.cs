using Application.Interfaces;
using Application.Results.Base;
using Domain.Models;
using Infrastructure.Interfaces;
using Infrastructure.UOW;

namespace Application.Services;

public class BatchService : IBatchService
{
    private readonly IReadOnlyService<Batch> _readService;
    private readonly ICreateService<Batch> _createService;
    private readonly IUpdateService<Batch> _updateService;
    private readonly IDeleteService<Batch> _deleteService;
    private readonly IBatchRepository _batchRepository;
    private readonly IRepository<Batch> _repository;
    private readonly IUnitOfWork _uow;

    public BatchService(
        IReadOnlyService<Batch> readService,
        ICreateService<Batch> createService,
        IUpdateService<Batch> updateService,
        IDeleteService<Batch> deleteService,
        IBatchRepository repository,
        IUnitOfWork uow)
    {
        _readService = readService;
        _createService = createService;
        _updateService = updateService;
        _deleteService = deleteService;
        _batchRepository = repository;
        _repository = repository;
        _uow = uow;
    }

    private Result Validate(Batch b)
    {
        if (b.Quantity <= 0)
            return Result.Failure(Errors.Validation(ErrorCodes.Batch.ValidationFailed, "Quantity must be greater than 0", "quantity"));

        var now = DateTime.UtcNow;
        if (b.DateAdded.ToUniversalTime() > now.AddMinutes(1))
            return Result.Failure(Errors.Validation(ErrorCodes.Batch.ValidationFailed, "DateAdded cannot be in the future", "dateAdded"));

        if (b.ExpireDate.ToUniversalTime() < b.DateAdded.ToUniversalTime())
            return Result.Failure(Errors.Validation(ErrorCodes.Batch.ValidationFailed, "ExpireDate must be after DateAdded", "expireDate"));

        return Result.Success();
    }

    public async Task<Result<Batch>> Add(string userId, Batch entity)
    {
        var check = Validate(entity);
        if (!check.IsSucceed)
            return Result<Batch>.Failure(check.Error!);

        var medicine = await _uow.Medicines.GetAsync(entity.MedicineId);
        if (medicine is null)
            return Result<Batch>.Failure(Errors.NotFound(ErrorCodes.Batch.MedicineNotFound, "Referenced medicine not found", "medicineId", entity.MedicineId));

        var zone = await _uow.Zones.GetAsync(entity.ZoneId);
        if (zone is null)
            return Result<Batch>.Failure(Errors.NotFound(ErrorCodes.Batch.ZoneNotFound, "Referenced zone not found", "zoneId", entity.ZoneId));

        return await _createService.Add(userId, entity);
    }

    public async Task<Result<Batch>> Update(string userId, Batch entity)
    {
        var existing = await _repository.GetAsync(entity.Id);
        if (existing is null)
            return Result<Batch>.Failure(Errors.NotFound(ErrorCodes.Batch.NotFound, "Not found", "batchId", entity.Id));

        var check = Validate(entity);
        if (!check.IsSucceed)
            return Result<Batch>.Failure(check.Error!);

        var medicine = await _uow.Medicines.GetAsync(entity.MedicineId);
        if (medicine is null)
            return Result<Batch>.Failure(Errors.NotFound(ErrorCodes.Batch.MedicineNotFound, "Referenced medicine not found", "medicineId", entity.MedicineId));

        var zone = await _uow.Zones.GetAsync(entity.ZoneId);
        if (zone is null)
            return Result<Batch>.Failure(Errors.NotFound(ErrorCodes.Batch.ZoneNotFound, "Referenced zone not found", "zoneId", entity.ZoneId));

        return await _updateService.Update(userId, entity);
    }

    public Task<Result<Batch>> Get(int id) => _readService.Get(id);

    public Task<Result<IEnumerable<Batch>>> GetAll() => _readService.GetAll();

    public Task<Result> Delete(string userId, int id) => _deleteService.Delete(userId, id);

    public async Task<Result<(IEnumerable<Batch> items, int totalCount)>> SearchByBatchNumber(string userId, string query, int limit, int offset)
    {
        if (limit <= 0)
            return Result<(IEnumerable<Batch>, int)>.Failure(PagingErrors.InvalidLimit(ErrorCodes.Batch.InvalidSearchPaging));

        if (offset < 0)
            return Result<(IEnumerable<Batch>, int)>.Failure(PagingErrors.InvalidOffset(ErrorCodes.Batch.InvalidSearchPaging));

        var (items, totalCount) = await _batchRepository.SearchByBatchNumberAsync(query?.Trim() ?? "", limit, offset);
        return Result<(IEnumerable<Batch>, int)>.Success((items, totalCount));
    }

}

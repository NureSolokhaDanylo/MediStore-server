namespace Domain.Enums
{
    public enum AlertType
    {
        ExpirationSoon = 1, //msg: batch id + medicine name + expiration date. ids: batchId
        Expired = 2, //msg: batch id + medicine name + expiration date. ids: batchId
        BatchConditionWarning = 3, //msg: . ids: batchId, zoneId
        ZoneConditionAlert = 4 //msg . ids: zoneId
    }
}

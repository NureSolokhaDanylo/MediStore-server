namespace Domain.Enums
{
    public enum AlertType
    {
        //проверка по всем нужным айди + типу алерта + issolved
        ExpirationSoon = 1, //msg: batch id + medicine name + expiration date. ids: batchId
        Expired = 2, //msg: batch id + medicine name + expiration date. ids: batchId
        BatchConditionWarning = 3, //msg: . ids: batchId, zoneId, sensorId
        ZoneConditionAlert = 4 //msg . ids: zoneId, sensorId
    }
}

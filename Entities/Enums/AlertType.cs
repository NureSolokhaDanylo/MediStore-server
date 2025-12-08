namespace Domain.Enums
{
    public enum AlertType
    {
        //TODO можно еще добавить то что сенсор давно не обновлял значения и его надо проверить
        ExpirationSoon = 1,
        Expired = 2,
        BatchConditionWarning = 3,
        ZoneConditionAlert = 4
    }
}

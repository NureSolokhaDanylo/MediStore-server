namespace Application.Results.Base;

public static class ErrorCodes
{
    public static class Common
    {
        public static readonly ErrorCode ValidationError = ErrorCode.Create("common.validation_error");
        public static readonly ErrorCode InvalidRequest = ErrorCode.Create("common.invalid_request");
        public static readonly ErrorCode NotFound = ErrorCode.Create("common.not_found");
        public static readonly ErrorCode Conflict = ErrorCode.Create("common.conflict");
        public static readonly ErrorCode Unexpected = ErrorCode.Create("common.unexpected");
    }

    public static class Auth
    {
        public static readonly ErrorCode InvalidCredentials = ErrorCode.Create("auth.invalid_credentials");
        public static readonly ErrorCode Unauthorized = ErrorCode.Create("auth.unauthorized");
        public static readonly ErrorCode Forbidden = ErrorCode.Create("auth.forbidden");
        public static readonly ErrorCode CurrentPasswordRequired = ErrorCode.Create("auth.current_password_required");
        public static readonly ErrorCode CurrentPasswordIncorrect = ErrorCode.Create("auth.current_password_incorrect");
    }

    public static class Account
    {
        public static readonly ErrorCode RequesterNotFound = ErrorCode.Create("account.requester_not_found");
        public static readonly ErrorCode TargetUserNotFound = ErrorCode.Create("account.target_user_not_found");
        public static readonly ErrorCode RolesDoNotExist = ErrorCode.Create("account.roles_do_not_exist");
        public static readonly ErrorCode CannotChangeAdminRoles = ErrorCode.Create("account.cannot_change_admin_roles");
        public static readonly ErrorCode CannotDeleteSelf = ErrorCode.Create("account.cannot_delete_self");
        public static readonly ErrorCode CreateFailed = ErrorCode.Create("account.create_failed");
        public static readonly ErrorCode ChangePasswordFailed = ErrorCode.Create("account.change_password_failed");
        public static readonly ErrorCode ChangeRolesFailed = ErrorCode.Create("account.change_roles_failed");
        public static readonly ErrorCode DeleteFailed = ErrorCode.Create("account.delete_failed");
        public static readonly ErrorCode InvalidPaging = ErrorCode.Create("account.invalid_paging");
    }

    public static class Medicine
    {
        public static readonly ErrorCode NotFound = ErrorCode.Create("medicine.not_found");
        public static readonly ErrorCode ValidationFailed = ErrorCode.Create("medicine.validation_failed");
        public static readonly ErrorCode InvalidSearchPaging = ErrorCode.Create("medicine.invalid_search_paging");
    }

    public static class Zone
    {
        public static readonly ErrorCode NotFound = ErrorCode.Create("zone.not_found");
        public static readonly ErrorCode ValidationFailed = ErrorCode.Create("zone.validation_failed");
        public static readonly ErrorCode InvalidSearchPaging = ErrorCode.Create("zone.invalid_search_paging");
    }

    public static class Batch
    {
        public static readonly ErrorCode NotFound = ErrorCode.Create("batch.not_found");
        public static readonly ErrorCode ValidationFailed = ErrorCode.Create("batch.validation_failed");
        public static readonly ErrorCode MedicineNotFound = ErrorCode.Create("batch.medicine_not_found");
        public static readonly ErrorCode ZoneNotFound = ErrorCode.Create("batch.zone_not_found");
        public static readonly ErrorCode InvalidSearchPaging = ErrorCode.Create("batch.invalid_search_paging");
    }

    public static class Sensor
    {
        public static readonly ErrorCode NotFound = ErrorCode.Create("sensor.not_found");
        public static readonly ErrorCode ZoneNotFound = ErrorCode.Create("sensor.zone_not_found");
        public static readonly ErrorCode InvalidPaging = ErrorCode.Create("sensor.invalid_paging");
        public static readonly ErrorCode RetrievalFailed = ErrorCode.Create("sensor.retrieval_failed");
        public static readonly ErrorCode SensorOff = ErrorCode.Create("sensor.sensor_off");
    }

    public static class SensorApiKey
    {
        public static readonly ErrorCode EmptyKey = ErrorCode.Create("sensor_api_key.empty_key");
        public static readonly ErrorCode InvalidKey = ErrorCode.Create("sensor_api_key.invalid_key");
        public static readonly ErrorCode SensorNotFound = ErrorCode.Create("sensor_api_key.sensor_not_found");
        public static readonly ErrorCode NotBoundToSensor = ErrorCode.Create("sensor_api_key.not_bound_to_sensor");
    }

    public static class Reading
    {
        public static readonly ErrorCode SensorNotFound = ErrorCode.Create("reading.sensor_not_found");
        public static readonly ErrorCode InvalidTimeRange = ErrorCode.Create("reading.invalid_time_range");
        public static readonly ErrorCode InvalidCount = ErrorCode.Create("reading.invalid_count");
    }

    public static class Alert
    {
        public static readonly ErrorCode RetrievalFailed = ErrorCode.Create("alert.retrieval_failed");
    }

    public static class AuditLog
    {
        public static readonly ErrorCode InvalidEntityType = ErrorCode.Create("audit_log.invalid_entity_type");
        public static readonly ErrorCode InvalidPaging = ErrorCode.Create("audit_log.invalid_paging");
        public static readonly ErrorCode InvalidCount = ErrorCode.Create("audit_log.invalid_count");
    }

    public static class AppSettings
    {
        public static readonly ErrorCode NotFound = ErrorCode.Create("app_settings.not_found");
        public static readonly ErrorCode ValidationFailed = ErrorCode.Create("app_settings.validation_failed");
    }

    public static class Push
    {
        public static readonly ErrorCode UserMismatch = ErrorCode.Create("push.user_mismatch");
        public static readonly ErrorCode RegistrationFailed = ErrorCode.Create("push.registration_failed");
    }

    public static class Report
    {
        public static readonly ErrorCode InvalidTimeRange = ErrorCode.Create("report.invalid_time_range");
    }
}

namespace WebApi.DTOs;

public static class ApiErrorCodes
{
    public static class Common
    {
        public const string ValidationError = "common.validation_error";
        public const string InvalidRequest = "common.invalid_request";
        public const string NotFound = "common.not_found";
        public const string Conflict = "common.conflict";
        public const string Unexpected = "common.unexpected";
    }

    public static class Auth
    {
        public const string InvalidCredentials = "auth.invalid_credentials";
        public const string Unauthorized = "auth.unauthorized";
        public const string Forbidden = "auth.forbidden";
        public const string CurrentPasswordRequired = "auth.current_password_required";
        public const string CurrentPasswordIncorrect = "auth.current_password_incorrect";
    }

    public static class Account
    {
        public const string RequesterNotFound = "account.requester_not_found";
        public const string TargetUserNotFound = "account.target_user_not_found";
        public const string RolesDoNotExist = "account.roles_do_not_exist";
        public const string CannotChangeAdminRoles = "account.cannot_change_admin_roles";
        public const string CannotDeleteSelf = "account.cannot_delete_self";
        public const string CreateFailed = "account.create_failed";
        public const string ChangePasswordFailed = "account.change_password_failed";
        public const string ChangeRolesFailed = "account.change_roles_failed";
        public const string DeleteFailed = "account.delete_failed";
        public const string InvalidPaging = "account.invalid_paging";
    }

    public static class Medicine
    {
        public const string NotFound = "medicine.not_found";
        public const string ValidationFailed = "medicine.validation_failed";
        public const string InvalidSearchPaging = "medicine.invalid_search_paging";
    }

    public static class Zone
    {
        public const string NotFound = "zone.not_found";
        public const string ValidationFailed = "zone.validation_failed";
        public const string InvalidSearchPaging = "zone.invalid_search_paging";
    }

    public static class Batch
    {
        public const string NotFound = "batch.not_found";
        public const string ValidationFailed = "batch.validation_failed";
        public const string MedicineNotFound = "batch.medicine_not_found";
        public const string ZoneNotFound = "batch.zone_not_found";
        public const string InvalidSearchPaging = "batch.invalid_search_paging";
    }

    public static class Sensor
    {
        public const string NotFound = "sensor.not_found";
        public const string ZoneNotFound = "sensor.zone_not_found";
        public const string InvalidPaging = "sensor.invalid_paging";
        public const string RetrievalFailed = "sensor.retrieval_failed";
        public const string SensorOff = "sensor.sensor_off";
    }

    public static class SensorApiKey
    {
        public const string EmptyKey = "sensor_api_key.empty_key";
        public const string InvalidKey = "sensor_api_key.invalid_key";
        public const string SensorNotFound = "sensor_api_key.sensor_not_found";
        public const string NotBoundToSensor = "sensor_api_key.not_bound_to_sensor";
    }

    public static class Reading
    {
        public const string SensorNotFound = "reading.sensor_not_found";
        public const string InvalidTimeRange = "reading.invalid_time_range";
        public const string InvalidCount = "reading.invalid_count";
    }

    public static class Alert
    {
        public const string RetrievalFailed = "alert.retrieval_failed";
    }

    public static class AuditLog
    {
        public const string InvalidEntityType = "audit_log.invalid_entity_type";
        public const string InvalidPaging = "audit_log.invalid_paging";
        public const string InvalidCount = "audit_log.invalid_count";
    }

    public static class AppSettings
    {
        public const string NotFound = "app_settings.not_found";
        public const string ValidationFailed = "app_settings.validation_failed";
    }

    public static class Push
    {
        public const string UserMismatch = "push.user_mismatch";
        public const string RegistrationFailed = "push.registration_failed";
    }

    public static class Report
    {
        public const string InvalidTimeRange = "report.invalid_time_range";
    }
}

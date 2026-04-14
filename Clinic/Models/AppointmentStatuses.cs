namespace Clinic.Models
{
    /// <summary>
    /// DB-level status constants matching the MySQL ENUM values.
    /// Use these for all database reads/writes/comparisons.
    /// Use localized resource keys (Status_Expected, Status_NoShow, Status_Completed)
    /// only for UI display via StatusDisplay property.
    /// </summary>
    public static class AppointmentStatuses
    {
        public const string Expected  = "Expected";
        public const string NoShow    = "NoShow";
        public const string Completed = "Completed";
    }
}

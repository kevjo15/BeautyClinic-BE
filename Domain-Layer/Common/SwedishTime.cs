namespace Domain_Layer.Common
{
    /// <summary>
    /// Booking times are stored as Swedish wall-clock time (Kind Unspecified).
    /// Comparisons against "now" must therefore use the Swedish clock,
    /// not the server clock (Azure runs in UTC).
    /// </summary>
    public static class SwedishTime
    {
        private static readonly TimeZoneInfo Zone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Stockholm");

        public static DateTime Now => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, Zone);
    }
}

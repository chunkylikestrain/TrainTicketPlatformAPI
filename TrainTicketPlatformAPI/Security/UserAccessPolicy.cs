using TrainTicketPlatformAPI.Models;

namespace TrainTicketPlatformAPI.Security
{
    public static class UserAccessPolicy
    {
        public const string AdminRole = "Admin";
        public const string PassengerRole = "Passenger";

        public const string ActiveStatus = "Active";
        public const string InactiveStatus = "Inactive";
        public const string SuspendedStatus = "Suspended";

        private static readonly string[] ValidRoles = [AdminRole, PassengerRole];
        private static readonly string[] ValidStatuses = [ActiveStatus, InactiveStatus, SuspendedStatus];

        public static string NormalizeRole(string? role)
        {
            var normalized = MatchAllowedValue(role, ValidRoles);
            if (normalized is null)
                throw new InvalidOperationException("Invalid user role.");

            return normalized;
        }

        public static string NormalizeStatus(string? status)
        {
            var normalized = MatchAllowedValue(status, ValidStatuses);
            if (normalized is null)
                throw new InvalidOperationException("Invalid user status.");

            return normalized;
        }

        public static bool IsActive(User user)
            => string.Equals(
                NormalizeStatus(user.Status),
                ActiveStatus,
                StringComparison.Ordinal);

        public static bool IsAdmin(User user)
            => string.Equals(
                NormalizeRole(user.Role),
                AdminRole,
                StringComparison.Ordinal);

        private static string? MatchAllowedValue(string? value, IEnumerable<string> allowedValues)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            var trimmed = value.Trim();
            return allowedValues.FirstOrDefault(allowed =>
                string.Equals(allowed, trimmed, StringComparison.OrdinalIgnoreCase));
        }
    }
}

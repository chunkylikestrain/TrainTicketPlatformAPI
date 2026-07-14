using Microsoft.AspNetCore.Mvc;

namespace TrainTicketPlatformAPI.Security
{
    public static class DangerousActionGuard
    {
        public const string ConfirmationHeader = "X-RailBook-Confirm";
        public const string Delete = "DELETE";
        public const string CancelRefund = "CANCEL_REFUND";
        public const string Import = "IMPORT";

        public static ActionResult? RequireHeader(ControllerBase controller, string expectedValue)
        {
            var actualValue = controller.Request.Headers[ConfirmationHeader].FirstOrDefault();
            if (string.Equals(actualValue, expectedValue, StringComparison.Ordinal))
                return null;

            return controller.BadRequest(new ProblemDetails
            {
                Title = "Dangerous action confirmation required",
                Detail = $"Send {ConfirmationHeader}: {expectedValue} to confirm this action.",
                Status = StatusCodes.Status400BadRequest
            });
        }
    }
}

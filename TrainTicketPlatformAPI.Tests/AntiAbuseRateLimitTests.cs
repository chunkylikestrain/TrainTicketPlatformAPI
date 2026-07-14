using System.Reflection;
using Microsoft.AspNetCore.RateLimiting;
using TrainTicketPlatformAPI.Controllers;
using TrainTicketPlatformAPI.Controllers.Admin;
using TrainTicketPlatformAPI.Security;

namespace TrainTicketPlatformAPI.Tests
{
    [TestFixture]
    public class AntiAbuseRateLimitTests
    {
        [TestCase(typeof(AuthController), nameof(AuthController.Register), RateLimitPolicyNames.Auth)]
        [TestCase(typeof(AuthController), nameof(AuthController.Login), RateLimitPolicyNames.Auth)]
        [TestCase(typeof(TripsController), nameof(TripsController.Search), RateLimitPolicyNames.PublicSearch)]
        [TestCase(typeof(TripsController), nameof(TripsController.SearchItineraries), RateLimitPolicyNames.PublicSearch)]
        [TestCase(typeof(StationsController), nameof(StationsController.GetAll), RateLimitPolicyNames.PublicSearch)]
        [TestCase(typeof(BookingsController), nameof(BookingsController.Create), RateLimitPolicyNames.BookingWrite)]
        [TestCase(typeof(BookingsController), nameof(BookingsController.CreateOrder), RateLimitPolicyNames.BookingWrite)]
        [TestCase(typeof(BookingsController), nameof(BookingsController.GetGuestTickets), RateLimitPolicyNames.TicketAccess)]
        [TestCase(typeof(BookingsController), nameof(BookingsController.RefundTicket), RateLimitPolicyNames.TicketAccess)]
        [TestCase(typeof(PaymentsController), nameof(PaymentsController.CreateIntent), RateLimitPolicyNames.Payment)]
        [TestCase(typeof(PaymentsController), nameof(PaymentsController.Confirm), RateLimitPolicyNames.Payment)]
        public void AbuseSensitiveActionsHaveExpectedRateLimit(Type controllerType, string actionName, string policyName)
        {
            var action = GetAction(controllerType, actionName);

            Assert.That(
                HasRateLimit(action, policyName),
                Is.True,
                $"{controllerType.Name}.{actionName} must use the {policyName} rate-limit policy.");
        }

        [Test]
        public void OpenRailwayImportControllerIsRateLimited()
        {
            Assert.That(
                HasRateLimit(typeof(AdminOpenRailwayImportController), RateLimitPolicyNames.AdminImport),
                Is.True,
                "Open Railway import endpoints must be rate-limited to protect external quota and server work.");
        }

        [Test]
        public void AnonymousActionsAreRateLimited()
        {
            var anonymousActions = typeof(Program).Assembly
                .GetTypes()
                .Where(type =>
                    !type.IsAbstract &&
                    typeof(Microsoft.AspNetCore.Mvc.ControllerBase).IsAssignableFrom(type) &&
                    type.Name.EndsWith("Controller", StringComparison.Ordinal))
                .SelectMany(controller => controller
                    .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
                    .Where(method => method.GetCustomAttributes<Microsoft.AspNetCore.Mvc.HttpGetAttribute>().Any() ||
                                     method.GetCustomAttributes<Microsoft.AspNetCore.Mvc.HttpPostAttribute>().Any() ||
                                     method.GetCustomAttributes<Microsoft.AspNetCore.Mvc.HttpPutAttribute>().Any() ||
                                     method.GetCustomAttributes<Microsoft.AspNetCore.Mvc.HttpDeleteAttribute>().Any())
                    .Select(method => new { Controller = controller, Method = method }))
                .Where(action => action.Method.GetCustomAttributes<Microsoft.AspNetCore.Authorization.AllowAnonymousAttribute>(inherit: true).Any())
                .ToList();

            Assert.That(anonymousActions, Is.Not.Empty);

            foreach (var action in anonymousActions)
            {
                Assert.That(
                    action.Method.GetCustomAttributes<EnableRateLimitingAttribute>(inherit: true).Any(),
                    Is.True,
                    $"{action.Controller.Name}.{action.Method.Name} allows anonymous access and must have a rate limit.");
            }
        }

        private static MethodInfo GetAction(Type controllerType, string actionName)
        {
            return controllerType
                .GetMethods(BindingFlags.Instance | BindingFlags.Public)
                .Single(method => method.Name == actionName);
        }

        private static bool HasRateLimit(MemberInfo member, string policyName)
        {
            return member
                .GetCustomAttributes<EnableRateLimitingAttribute>(inherit: true)
                .Any(attribute => attribute.PolicyName == policyName);
        }
    }
}

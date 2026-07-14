using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TrainTicketPlatformAPI.Contracts.Bookings;
using TrainTicketPlatformAPI.Controllers;
using TrainTicketPlatformAPI.Models;

namespace TrainTicketPlatformAPI.Tests
{
    [TestFixture]
    public class AdminEndpointAuthorizationTests
    {
        [Test]
        public void AdminControllersRequireAdminRoleAndNeverAllowAnonymous()
        {
            var adminControllers = GetControllerTypes()
                .Where(type => type.Namespace == "TrainTicketPlatformAPI.Controllers.Admin")
                .ToList();

            Assert.That(adminControllers, Is.Not.Empty);

            foreach (var controller in adminControllers)
            {
                Assert.Multiple(() =>
                {
                    Assert.That(
                        HasAdminAuthorize(controller),
                        Is.True,
                        $"{controller.Name} must require the Admin role.");

                    Assert.That(
                        HasAllowAnonymous(controller),
                        Is.False,
                        $"{controller.Name} must not allow anonymous access.");
                });

                foreach (var action in GetActionMethods(controller))
                {
                    Assert.That(
                        HasAllowAnonymous(action),
                        Is.False,
                        $"{controller.Name}.{action.Name} must not allow anonymous access.");
                }
            }
        }

        [Test]
        public void ApiAdminRoutesRequireAdminRole()
        {
            var adminRouteControllers = GetControllerTypes()
                .Where(HasApiAdminRoute)
                .ToList();

            Assert.That(adminRouteControllers, Is.Not.Empty);

            foreach (var controller in adminRouteControllers)
            {
                Assert.That(
                    HasAdminAuthorize(controller),
                    Is.True,
                    $"{controller.Name} exposes an api/admin route and must require the Admin role.");
            }
        }

        [Test]
        public void BookingControllerDoesNotBindBookingEntityForPassengerUpdate()
        {
            var update = typeof(BookingsController).GetMethod(nameof(BookingsController.Update));

            Assert.Multiple(() =>
            {
                Assert.That(update, Is.Not.Null);
                Assert.That(
                    update!.GetParameters().Any(parameter => parameter.ParameterType == typeof(Booking)),
                    Is.False,
                    "Passenger booking update must use a request DTO so status, payment, ticket, and ownership fields cannot be overposted.");
                Assert.That(
                    update.GetParameters().Any(parameter => parameter.ParameterType == typeof(UpdateBookingRequest)),
                    Is.True);
            });
        }

        [Test]
        public void LegacyManagementControllersAreRemoved()
        {
            var controllerNames = GetControllerTypes()
                .Select(type => type.Name)
                .ToList();

            Assert.Multiple(() =>
            {
                Assert.That(controllerNames, Does.Not.Contain("UsersController"));
                Assert.That(controllerNames, Does.Not.Contain("TrainsController"));
                Assert.That(controllerNames, Does.Not.Contain("SeatsController"));
            });
        }

        private static IEnumerable<Type> GetControllerTypes()
        {
            return typeof(Program).Assembly
                .GetTypes()
                .Where(type =>
                    !type.IsAbstract &&
                    typeof(ControllerBase).IsAssignableFrom(type) &&
                    type.Name.EndsWith("Controller", StringComparison.Ordinal));
        }

        private static IEnumerable<MethodInfo> GetActionMethods(Type controller)
        {
            return controller
                .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
                .Where(method =>
                    !method.IsSpecialName &&
                    method.GetCustomAttributes().Any(attribute =>
                        attribute.GetType().Name.StartsWith("Http", StringComparison.Ordinal)));
        }

        private static bool HasApiAdminRoute(Type controller)
        {
            return controller
                .GetCustomAttributes<RouteAttribute>(inherit: true)
                .Any(route => route.Template?.StartsWith("api/admin", StringComparison.OrdinalIgnoreCase) == true);
        }

        private static bool HasAdminAuthorize(MemberInfo member)
        {
            return member
                .GetCustomAttributes<AuthorizeAttribute>(inherit: true)
                .Any(attribute => SplitRoles(attribute.Roles).Contains("Admin", StringComparer.OrdinalIgnoreCase));
        }

        private static bool HasAllowAnonymous(MemberInfo member)
        {
            return member.GetCustomAttributes<AllowAnonymousAttribute>(inherit: true).Any();
        }

        private static IEnumerable<string> SplitRoles(string? roles)
        {
            return (roles ?? string.Empty)
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        }

    }
}

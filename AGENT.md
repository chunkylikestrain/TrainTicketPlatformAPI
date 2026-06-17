Analysis Summary
This repository already contains two major application surfaces:
TrainTicketPlatformAPI — an ASP.NET Core Web API project with Entity Framework Core, SQL Server, JWT setup, controllers, services, models, and migrations.
TrainTicketApp — a WinForms desktop application that directly consumes the same backend service classes and database context through dependency injection rather than calling the Web API over HTTP.
I did not change any files, did not commit, and did not create a PR, because you explicitly requested analysis only.
Existing Project Structure
Solution-level projects
The repository contains:
TrainTicketPlatformAPI — ASP.NET Core Web API backend.
TrainTicketApp — WinForms desktop UI.
TrainTicketPlatformAPI.Tests — unit test project.
PasswordHasher — small helper project, likely for generating password hashes.
The Web API project registers controllers, Swagger, EF Core SQL Server, domain services, and JWT bearer authentication in Program.cs. Program.cs (lines 11-22)Program.cs (lines 25-47)
Existing Domain Entities
User
User represents an application account with identity, credentials, phone, role, and related bookings. User.cs (lines 3-11)
Current fields:
Id
Email
PasswordHash
Phone
Role
Bookings
Observations:
This is a custom user model, not ASP.NET Core Identity.
Role is a free-form string, currently used for "Passenger" and "Admin" logic.
The API currently risks exposing PasswordHash if returning User entities directly from controllers.
Train
Train represents a scheduled train journey, not just a physical train. It includes name, stations, departure/arrival times, price, and bookings. Train.cs (lines 5-17)
Current fields:
Id
Name
DepartureStation
ArrivalStation
DepartureTime
ArrivalTime
Price
Bookings
Observations:
This model conflates a train service and a specific train trip/schedule.
For a ticket website, it may be better to split this later into:Train
Route
Station
Trip or Schedule
Fare

Seat
Seat represents a seat attached to a train, with coach, number, class type, availability, and booking relationship. Seat.cs (lines 3-14)
Current fields:
Id
TrainId
Coach
Number
ClassType
IsAvailable
Train
Bookings
Observations:
IsAvailable is stored globally on the seat.
That is problematic for a train ticket website because a seat should be available or unavailable per trip/date, not permanently unavailable after one booking.
The current booking service also checks same-day booking conflicts, but the global IsAvailable flag can still block future dates incorrectly. BookingService.cs (lines 99-111)
Booking
Booking represents a customer reservation for a train and seat on a travel date, including payment and cancellation state. Booking.cs (lines 3-17)
Current fields:
Id
UserId
TrainId
SeatId
BookingDate
TravelDate
PaymentStatus
IsCancelled
CancellationDate
User
Train
Seat
Observations:
BookingDate exists but is not set when creating bookings in BookingService.
PaymentStatus is a string, not an enum.
Cancellation is implemented as a soft cancellation using IsCancelled and CancellationDate.
There is no ticket number, passenger details, fare snapshot, booking reference, or booking status enum.
Payment
Payment represents a payment attempt tied to a booking, with payment date, status, and amount. Payment.cs (lines 5-14)
Current fields:
Id
BookingId
PaymentDate
Status
Amount
Booking
Observations:
There is no card token, processor transaction ID, provider, failure reason, refund information, or currency.
The payment service currently simulates card acceptance by checking Visa/MasterCard prefixes. PaymentService.cs (lines 19-33)
BookingReport
BookingReport is a report projection rather than a real entity. It has date range, total bookings, total revenue, and cancellation count. BookingReport.cs (lines 3-9)
Current fields:
From
To
TotalBookings
TotalRevenue
TotalCancellations
Observations:
It is configured as keyless in EF Core. TrainTicketDbContext.cs (lines 55-58)
It probably does not need a DbSet unless queried from SQL views/stored procedures.
DTOs
The project currently has:
LoginDto with required Email and Password. LoginDto.cs (lines 5-8)
RegisterDto with required Email, Password, and Phone. RegisterDto.cs (lines 5-9)
LoginResponseDto with Token, UserId, and Role, but it does not appear to be used by AuthController, which currently returns only an anonymous { token } object. LoginResponesDto.cs (lines 3-7)AuthController.cs (lines 30-37)
Existing Database Logic
EF Core DbContext
The API uses TrainTicketDbContext, which exposes these tables/projections:
Users
Trains
Seats
Bookings
bookingReports
Payments TrainTicketDbContext.cs (lines 12-17)
The app is configured for SQL Server using DefaultConnection. Program.cs (lines 15-16)
Relationships and delete behavior
The DbContext configures:
Booking -> Train with DeleteBehavior.Restrict. TrainTicketDbContext.cs (lines 24-29)
Booking -> Seat with DeleteBehavior.Cascade. TrainTicketDbContext.cs (lines 31-36)
Booking -> User with DeleteBehavior.Cascade. TrainTicketDbContext.cs (lines 38-43)
Train.Price precision as decimal(18,2). TrainTicketDbContext.cs (lines 45-48)
Payment.Amount precision as decimal(18,2). TrainTicketDbContext.cs (lines 50-53)
BookingReport as keyless. TrainTicketDbContext.cs (lines 55-58)
Important concern:
Cascading deletes from User or Seat into Booking may not be ideal for a ticketing platform because bookings are business records. A production website should generally preserve booking/payment history.
Migrations
The repository already has migrations, including:
Initial schema creation.
Auth fields added to users.
Booking cancellation fields.
Booking report table/projection.
The migration filenames indicate the schema has evolved toward auth and reporting.
Existing Service Layer
The backend uses a service layer behind interfaces:
IUserService
ITrainService
ISeatService
IBookingService
IPaymentService
These are registered in DI in the API startup. Program.cs (lines 18-22)
UserService
UserService handles user CRUD, registration, login, password hashing, and JWT creation. UserService.cs (lines 17-37)UserService.cs (lines 85-135)
Current behavior:
Lists all users.
Gets user by ID.
Creates user after checking unique email.
Updates email, phone, and role.
Prevents deleting users with bookings.
Registers users with BCrypt password hashing.
Assigns new users the default role "Passenger".
Logs in users by verifying BCrypt hash.
Creates JWT with:sub
email
role claim. UserService.cs (lines 114-120)

Important issues:
UserService currently has two constructors:One accepting only TrainTicketDbContext.
One accepting TrainTicketDbContext and IConfiguration. UserService.cs (lines 14-15)UserService.cs (lines 75-82)

The one-parameter constructor leaves _config unset, which is dangerous if that constructor is ever selected.
API controllers return User entities directly, which may expose password hashes. UsersController.cs (lines 15-27)
TrainService
TrainService handles searching and CRUD for trains. TrainService.cs (lines 12-72)
Current behavior:
Searches trains by departure station, arrival station, and date. TrainService.cs (lines 12-22)
Gets all trains.
Gets train by ID.
Creates train.
Updates train fields.
Deletes a train only if it has no seats and no bookings. TrainService.cs (lines 58-68)
Important issue:
The search uses StringComparison.OrdinalIgnoreCase inside an EF query. TrainService.cs (lines 17-21)
Depending on EF Core/provider behavior, this can cause translation problems. A database-friendly approach would use normalized columns, database collation, or EF.Functions.Like.
SeatService
SeatService handles seat CRUD and retrieval by train. SeatService.cs (lines 12-68)
Current behavior:
Gets all seats.
Gets seat by ID.
Gets seats for a train.
Creates seat only if the train exists.
Updates coach, number, class type, and availability.
Prevents deleting a seat with existing bookings. SeatService.cs (lines 56-64)
Important issue:
Seat availability is stored on the seat itself, which does not model trip/date-specific inventory well.
BookingService
BookingService handles booking creation, cancellation, update, lookup, availability checks, and reporting. BookingService.cs (lines 14-138)
Current behavior:
Creates a booking if the seat exists and IsAvailable is true. BookingService.cs (lines 14-26)
Marks the seat unavailable after booking. BookingService.cs (lines 21-26)
Cancels a booking only if cancellation is more than one hour before travel time. BookingService.cs (lines 31-40)
Soft-cancels bookings and frees seats. BookingService.cs (lines 42-51)
Updates booking seat/date.
Gets bookings by user.
Checks availability by train, seat, and travel date. BookingService.cs (lines 99-111)
Generates booking report from bookings and successful payments. BookingService.cs (lines 113-137)
Important issues:
CreateBookingAsync does not set BookingDate.
It does not check date-specific clashes during creation, only global Seat.IsAvailable.
It does not exclude cancelled bookings in the availability clash check. BookingService.cs (lines 107-111)
There is no transaction or concurrency control, so two users could potentially reserve the same seat at the same time.
Cancelling a booking frees the seat globally, which may incorrectly affect other travel dates.
PaymentService
PaymentService handles fake payment processing and payment retrieval. PaymentService.cs (lines 12-70)
Current behavior:
Validates that the booking exists.
Checks whether the card number starts with a Visa/MasterCard prefix.
Creates a payment record.
Updates Booking.PaymentStatus to "Successful" or "Failed".
Gets payment by ID.
Gets payments by booking.
Gets all payments.
Important issues:
It calls Substring(0, 4) without validating card length first. Short card input can throw an exception. PaymentService.cs (lines 21-23)
It stores raw card number input temporarily and treats it directly in service logic.
A production API should never store or log raw card numbers and should use a payment provider/tokenization.
Failed payments do not appear to release seats or expire reservations.
Existing API Controllers
AuthController
Endpoints:
POST api/Auth/register
POST api/Auth/login AuthController.cs (lines 15-37)
Behavior:
Register creates a passenger account.
Login returns { token }.
Issues:
LoginResponseDto exists but is not used.
Login does not return userId or role, even though the DTO supports them.
UsersController
Endpoints:
GET api/Users
GET api/Users/{id}
POST api/Users
PUT api/Users/{id}
DELETE api/Users/{id} UsersController.cs (lines 15-80)
Issues:
No [Authorize].
No role restrictions.
Returns entity models directly.
May expose PasswordHash.
TrainsController
Endpoints:
GET api/Trains/search?from=A&to=B&date=...
GET api/Trains
GET api/Trains/{id}
POST api/Trains
PUT api/Trains/{id}
DELETE api/Trains/{id} TrainController.cs (lines 14-78)
Issues:
Admin-only write operations are currently public.
Search returns raw Train entities.
SeatsController
Endpoints:
GET api/Seats
GET api/Seats/{id}
GET api/Seats/train/{trainId}
POST api/Seats
PUT api/Seats/{id}
DELETE api/Seats/{id} SeatController.cs (lines 15-85)
Issues:
Admin-only seat management is currently public.
Availability should be computed per trip/date rather than only read from Seat.IsAvailable.
BookingsController
Endpoints:
GET api/Bookings
GET api/Bookings/{id}
GET api/Bookings/user/{userId}
GET api/Bookings/availability
POST api/Bookings
PUT api/Bookings/{id}
DELETE api/Bookings/{id} for cancellation
GET api/Bookings/report BookingController.cs (lines 16-123)
Issues:
No [Authorize].
Users can theoretically request other users’ bookings.
Cancellation uses DELETE, but semantically a POST api/bookings/{id}/cancel endpoint may be clearer.
Reports should be admin-only.
PaymentsController
Endpoints:
GET api/Payments
GET api/Payments/{id}
GET api/Payments/booking/{bookingId}
POST api/Payments PaymentController.cs (lines 16-55)
Also defines PaymentCreateDto inside the controller file. PaymentController.cs (lines 63-74)
Issues:
Payment DTO should be moved into a DTO folder.
Payment endpoints should be authenticated and authorization-scoped.
Admin-only payment listing is currently public.
Raw card number should not be accepted by a production backend except through PCI-compliant tokenized/payment-provider flows.
Existing UI Logic
The WinForms app currently uses the backend services directly through dependency injection, not through HTTP API calls.
WinForms startup and dependency injection
TrainTicketApp/Program.cs builds a host, configures EF Core SQL Server, registers services, registers forms, and starts a form. Program.cs (lines 21-64)
Important observation:
The app currently starts SearchTrainsForm, not LoginForm. Program.cs (lines 67-69)
That bypasses the intended authentication-first flow.
Session state
AppSession stores current user ID and JWT token globally. AppSession.cs (lines 8-11)
This is used by UI forms after login/booking.
Login UI
LoginForm:
Reads email/password.
Calls IUserService.LoginAsync.
Parses the JWT.
Extracts user ID and role.
Stores session values.
Routes admins to AdminMainForm and passengers to SearchTrainsForm. LoginForm.cs (lines 39-89)
Issues:
The WinForms app is calling service classes directly, so the JWT is not actually used to authorize API calls.
There is a duplicate using System;. LoginForm.cs (lines 1-3)
The app parses claims but then continues using direct services/DbContext.
Search trains UI
SearchTrainsForm:
Reads departure, arrival, and travel date.
Calls GetAllTrainsAsync.
Filters trains in memory.
Binds matches to a DataGridView.
On double-click, opens SelectSeatForm with selected train and travel date. SearchTrainsForm.cs (lines 43-93)
Issues:
It should call SearchTrainsAsync instead of loading all trains and filtering locally.
For a web frontend, this logic should become a GET /api/trains/search call.
Seat selection UI
SelectSeatForm:
Loads seats for selected train.
Displays coach, number, and class type.
Creates a booking directly on button click.
Opens PaymentForm. SelectSeatForm.cs (lines 30-60)
Issues:
The seat list does not filter by date-specific availability.
It creates the booking before payment.
The form constructor does not call InitializeComponent() in the shown code, which may be a bug. SelectSeatForm.cs (lines 21-28)
Booking confirmation/payment UI
BookingForm:
Loads train and seat details.
Creates a booking with PaymentStatus = "Pending".
Opens PaymentForm. BookingForm.cs (lines 45-85)
PaymentForm:
Shows booking/payment details.
Calls IPaymentService.ProcessPaymentAsync.
Shows success/failure message. PaymentForm.cs (lines 26-60)
Issues:
BookingDate is not set when creating bookings. BookingForm.cs (lines 62-69)
PaymentForm.TravelDate is displayed but may not always be set by callers. PaymentForm.cs (lines 12-33)
Admin UI
AdminMainForm routes admins to:
Manage trains.
Manage seat maps.
View reports.
ManageTrainForm:
Loads all trains.
Opens upsert form for add/edit.
Deletes selected train. ManageTrainForm.cs (lines 20-69)
ManageSeatMapForm:
Loads all seats.
Opens upsert form for add/edit.
Deletes selected seat after confirmation. ManageSeatForm.cs (lines 20-83)
ViewBookingReportForm:
Defaults to last seven days.
Calls GenerateBookingReportAsync.
Displays total bookings, revenue, and cancellations. ViewBookingReportForm.cs (lines 19-43)
Issues:
Admin controls exist in WinForms, but the API endpoints themselves are not protected by [Authorize(Roles = "Admin")].
UpsertTrainForm appears incomplete; it has a DI constructor but does not call InitializeComponent() and does not implement save/load logic. UpsertTrainFormcs.cs (lines 9-20)
Current State Relative to “ASP.NET Core Web API Backend for a Train Ticket Website”
The project is already partly an ASP.NET Core Web API backend, but it still carries desktop-app assumptions:
WinForms directly references backend models/services.
Controllers expose EF entities directly.
Auth exists but is not enforced because UseAuthentication() is missing from the pipeline.
API endpoints are not protected with [Authorize].
Domain models are too simple for a real train ticket website.
Seat availability is modeled globally instead of per journey/date.
Payments are fake and unsafe for production-like design.
There is no clean DTO/request/response boundary for a frontend website.
One major technical bug: the API configures JWT authentication but only calls UseAuthorization(). It does not call UseAuthentication() before authorization. Program.cs (lines 59-63)
Proposed Repurposing Plan
Phase 1 — Stabilize the Existing API Backend
1. Make the Web API the primary application
Keep TrainTicketPlatformAPI as the backend and treat TrainTicketApp as legacy/reference UI.
Actions:
Keep TrainTicketPlatformAPI as the main deployable backend.
Do not let the website frontend reference backend services directly.
Expose all frontend behavior through HTTP endpoints.
Optionally move WinForms into an archive/legacy folder later.
2. Fix authentication pipeline
Current API registers JWT bearer auth but does not call UseAuthentication(). Program.cs (lines 30-47)Program.cs (lines 59-63)
Plan:
Add app.UseAuthentication(); before app.UseAuthorization();.
Add [Authorize] attributes to protected endpoints.
Add [Authorize(Roles = "Admin")] for admin-only management/report endpoints.
Add [AllowAnonymous] to login/register and train search endpoints if public search is desired.
Suggested access model:
Endpoint area	Access
Register/login	Anonymous
Train search	Anonymous or authenticated
Seat availability	Anonymous or authenticated
Create booking	Passenger
User’s own bookings	Passenger
Cancel own booking	Passenger
Manage trains/seats	Admin
Reports	Admin
Payment list	Admin
Pay for own booking	Passenger

3. Introduce DTOs and stop returning EF entities directly
Current controllers return entities like User, Train, Booking, Seat, and Payment directly. For example, UsersController.GetById returns User. UsersController.cs (lines 20-27)
Plan:
Create folders such as:
TrainTicketPlatformAPI/
  Contracts/
    Auth/
    Users/
    Trains/
    Seats/
    Bookings/
    Payments/
    Reports/
Examples:
RegisterRequest
LoginRequest
LoginResponse
TrainSearchRequest
TrainSearchResultDto
CreateBookingRequest
BookingDetailsDto
SeatAvailabilityDto
CreatePaymentIntentRequest
PaymentResultDto
BookingReportDto
This will:
Prevent leaking PasswordHash.
Avoid circular serialization from navigation properties.
Allow frontend-specific responses.
Make Swagger easier to consume.
4. Add consistent API error handling
Current controllers repeatedly catch KeyNotFoundException and InvalidOperationException. BookingController.cs (lines 24-32)BookingController.cs (lines 60-70)
Plan:
Add global exception handling middleware or filters.
Return standardized ProblemDetails.
Use domain-specific exceptions if desired:NotFoundException
ValidationException
ConflictException
ForbiddenException

Phase 2 — Remodel Core Ticketing Domain
5. Split “train” from “trip/schedule”
Current Train contains route, departure/arrival time, and price. Train.cs (lines 9-15)
For a website, consider this model:
Station
  Id
  Name
  Code
  City

Train
  Id
  Number
  Name
  Operator

Route
  Id
  OriginStationId
  DestinationStationId

Trip
  Id
  TrainId
  RouteId
  DepartureTime
  ArrivalTime
  Status

Coach
  Id
  TrainId or TripId
  Name
  ClassType

Seat
  Id
  CoachId
  SeatNumber
  SeatType

Fare
  Id
  TripId
  ClassType
  Price
  Currency
If you want minimum disruption, you can first rename the existing conceptual Train API to represent a Trip, then migrate cleanly later.
6. Make seat availability trip/date-specific
Current seat availability is global on Seat.IsAvailable. Seat.cs (lines 7-10)
Plan:
Do not rely on Seat.IsAvailable for booking availability.
Availability should be computed from active bookings for a specific trip/date.
Add unique constraints to prevent duplicate active seat bookings.
Minimum improvement:
Booking unique active constraint:
TripId + SeatId + TravelDate, where IsCancelled = false
In SQL Server, this can be a filtered unique index.
Booking logic should check:
Seat belongs to trip/train.
Travel date/trip exists.
No non-cancelled booking already exists for the same seat/trip/date.
Booking is created inside a transaction.
7. Add booking lifecycle statuses
Current booking uses PaymentStatus string plus IsCancelled. Booking.cs (lines 9-13)
Plan:
Use explicit statuses:
BookingStatus
{
    PendingPayment,
    Confirmed,
    Cancelled,
    Expired
}

PaymentStatus
{
    Pending,
    Successful,
    Failed,
    Refunded
}
Add:
BookingReference
CreatedAtUtc
ExpiresAtUtc
ConfirmedAtUtc
CancelledAtUtc
TotalAmount
Currency
This is important for web booking flows where users may reserve a seat briefly before payment.
Phase 3 — Refactor Services for Web API Use
8. Refactor BookingService
Current booking creation only checks global seat availability and marks the seat unavailable. BookingService.cs (lines 14-26)
Plan:
Set BookingDate/CreatedAtUtc automatically.
Use transactions.
Check date/trip-specific uniqueness.
Exclude cancelled bookings from availability checks.
Create booking with PendingPayment.
Optionally expire unpaid bookings after a time window.
Do not mutate global Seat.IsAvailable when a booking is made.
Suggested web flow:
User searches trips.
User views seat map for trip.
API returns available/unavailable seats for that trip.
User creates pending booking.
User pays.
Booking becomes confirmed.
If payment fails or expires, booking is released.
9. Refactor PaymentService
Current service checks card prefixes and marks status accordingly. PaymentService.cs (lines 19-50)
Plan:
Replace raw card number handling with a provider abstraction:IPaymentGateway
CreatePaymentIntentAsync
ConfirmPaymentAsync
RefundAsync

Store provider transaction IDs, not card numbers.
Handle failed payments safely.
Mark booking confirmed only after successful payment.
Support refunds on cancellation if required.
For an educational/demo backend, fake payments are okay, but the API should still accept safe test tokens rather than card numbers.
10. Refactor UserService
Current UserService has duplicate constructors and mixes CRUD, registration, login, and JWT generation. UserService.cs (lines 14-15)UserService.cs (lines 75-82)
Plan:
Keep only one constructor.
Split into:IAuthService
IUserService
ITokenService

Normalize/lowercase emails.
Add unique index on email.
Return DTOs instead of entity objects.
Consider ASP.NET Core Identity if the project needs password reset, email confirmation, lockouts, refresh tokens, etc.
Phase 4 — Redesign API Surface for Website Frontend
A practical API for a train ticket website could look like this:
Public/Auth endpoints
POST /api/auth/register
POST /api/auth/login
POST /api/auth/refresh
POST /api/auth/logout
GET  /api/auth/me
Current auth endpoints already exist in basic form. AuthController.cs (lines 15-37)
Search/trip endpoints
GET /api/stations
GET /api/trips/search?from=NYC&to=BOS&date=2026-07-01
GET /api/trips/{tripId}
GET /api/trips/{tripId}/seats
GET /api/trips/{tripId}/availability?date=2026-07-01
Current closest endpoint is GET api/Trains/search. TrainController.cs (lines 14-22)
Booking endpoints
POST /api/bookings
GET  /api/bookings/me
GET  /api/bookings/{bookingId}
POST /api/bookings/{bookingId}/cancel
POST /api/bookings/{bookingId}/confirm
Current controller already has create, get, user bookings, update, delete-as-cancel, availability, and report endpoints. BookingController.cs (lines 16-123)
Payment endpoints
POST /api/bookings/{bookingId}/payment-intent
POST /api/payments/confirm
GET  /api/bookings/{bookingId}/payments
Current payment controller has basic create and read endpoints. PaymentController.cs (lines 16-55)
Admin endpoints
GET    /api/admin/users
GET    /api/admin/bookings
GET    /api/admin/reports/bookings
POST   /api/admin/trains
PUT    /api/admin/trains/{id}
DELETE /api/admin/trains/{id}
POST   /api/admin/seats
PUT    /api/admin/seats/{id}
DELETE /api/admin/seats/{id}
Currently admin operations are mixed into public controllers. For example, train creation/deletion are in TrainsController without authorization. TrainController.cs (lines 45-78)
Phase 5 — Database Hardening
11. Add constraints and indexes
Recommended constraints:
Unique user email.
Required normalized station names/codes.
Unique seat per train/coach/number.
Unique active booking for trip/date/seat.
Required booking reference.
Required payment status/booking status values.
Recommended indexes:
Trips(DepartureStationId, ArrivalStationId, DepartureTime)
Bookings(UserId)
Bookings(TripId, TravelDate)
Bookings(SeatId, TravelDate)
Payments(BookingId)
Users(NormalizedEmail)
12. Preserve business records
Current model can cascade delete bookings from users/seats. TrainTicketDbContext.cs (lines 31-43)
Plan:
Avoid deleting users with historical bookings.
Avoid deleting seats/trips with historical bookings.
Use soft deletes or status fields for operational records.
Use restrictive delete behavior for important financial/booking entities.
Phase 6 — Website Integration
13. Prepare for SPA/frontend client
Add:
CORS policy for frontend origin.
Clean Swagger/OpenAPI descriptions.
JSON enum serialization.
API versioning if desired.
Health checks.
Pagination/filtering/sorting for admin lists.
Rate limiting on auth endpoints.
14. Replace WinForms assumptions
The WinForms app directly registers TrainTicketDbContext and services. Program.cs (lines 36-44)
For a web backend architecture:
Frontend should call HTTP APIs.
Backend should own database access.
Desktop app, if retained, should become an API client rather than directly using EF/services.
The AppSession.JwtToken pattern can be replaced by browser storage/session handling or secure cookies depending on frontend architecture. AppSession.cs (lines 8-11)
Suggested Implementation Roadmap
Step 1 — Safe backend cleanup
Add UseAuthentication.
Add [Authorize]/role policies.
Remove duplicate UserService constructor.
Add DTOs for auth/users.
Stop returning password hashes.
Move PaymentCreateDto out of controller file.
Step 2 — API contract cleanup
Create request/response DTOs for trains, seats, bookings, payments.
Update controllers to accept/return DTOs.
Add centralized error handling.
Add validation attributes and model validation responses.
Step 3 — Booking correctness
Set BookingDate automatically.
Stop mutating global Seat.IsAvailable.
Make availability date/trip-specific.
Add transaction around booking creation.
Add database uniqueness constraint for active seat booking.
Exclude cancelled bookings from availability checks.
Step 4 — Domain model upgrade
Introduce Station, Trip, maybe Coach.
Migrate existing Train data into trip-like records.
Add fare model or fare snapshot on booking.
Step 5 — Payment flow
Replace raw card-number flow with fake/test token or payment gateway abstraction.
Add payment intent/confirmation flow.
Confirm booking only after successful payment.
Add refund/cancellation rules.
Step 6 — Admin/reporting
Move report endpoint behind admin authorization.
Add admin dashboard endpoints.
Add date-range and route/train filters.
Ensure reporting excludes cancelled/failed states appropriately.
Step 7 — Production-readiness
Add CORS.
Add logging.
Add health checks.
Add integration tests.
Add seed data.
Add deployment configuration.
Add CI checks.
Key Risks to Address Before Using as Website Backend
Authentication is configured but not activated because UseAuthentication() is missing. Program.cs (lines 59-63)
No controllers are protected with [Authorize].
Users are returned directly, risking password hash exposure. UsersController.cs (lines 20-27)
Seat availability is globally stored, not trip/date-specific. Seat.cs (lines 7-10)
Booking creation is not concurrency-safe and does not check date-specific clashes in the create path. BookingService.cs (lines 14-26)
Payment processing accepts raw card numbers and has unsafe substring assumptions. PaymentService.cs (lines 21-23)
WinForms app bypasses the API and directly uses EF/services. Program.cs (lines 36-44)
Admin operations are public unless protected elsewhere, which they currently are not. TrainController.cs (lines 45-78)
# RailBook Frontend

This is the Vite + React + TypeScript frontend for RailBook.

It contains both passenger-facing pages and the JWT-protected admin control panel. The frontend talks to the ASP.NET Core API over HTTP and should not import backend classes directly.

## Main Areas

- Passenger home page, station search, filters, discounts, and traveler selection.
- Search results for direct, transfer, and round-trip itineraries.
- Class and seat-map selection.
- Booking summary, add-on dog/baggage tickets, checkout, payment, and ticket confirmation.
- My account, My tickets, My invoices, loyalty points, current trip, and help pages.
- Admin dashboard, trains, routes, schedules, pricing, discounts, users, bookings, revenue, and audit logs.

## Run Locally

```powershell
npm install
npm run dev
```

The frontend expects the API at `https://localhost:7246` unless the Axios base URL is changed.

## Build

```powershell
npm run build
```

## Notes For Contributors

- Keep passenger and admin UI patterns consistent with the existing components and CSS.
- Preserve booking route/search/seat/order state through URLs and API calls.
- Use the shared API clients in `src/api`.
- Add frontend types in `src/types` when backend DTOs change.
- Avoid hardcoded demo train values in booking flows.

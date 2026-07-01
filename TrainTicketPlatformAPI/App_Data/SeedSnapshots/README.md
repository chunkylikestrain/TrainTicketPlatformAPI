# Seed Snapshots

Place one-time exported Open Railway / PLK snapshot JSON files in this folder.

During development startup, `DevelopmentSeedData` reads `*.json` files here and
upserts stations, trains, routes, stops, trips, carriages, seats, and fares into
the local database without calling the external API.

Suggested flow:

1. Use the admin Open Railway importer to import and review a realistic operating date.
2. Export a snapshot from `GET /api/admin/open-railway/seed-snapshot?operatingDate=YYYY-MM-DD`.
3. Save the JSON response in this folder, for example `plk-2026-07-01.json`.
4. Recreate/update the development database from migrations and seed data.

The old handwritten schedule seed is disabled by default. Re-enable it only as
a fallback with `SeedData:UseHandWrittenDemoSchedules=true`.

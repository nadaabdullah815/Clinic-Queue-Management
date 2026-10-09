# ClinicQueue

A web application for managing patient queue turns at a dental clinic with a single doctor.

Instead of booking a fixed time, a patient takes a numbered turn for the day and follows
their position online, so they only go to the clinic when their turn is close.
The doctor manages the queue from a dashboard, and the system keeps every status up to date.

The user interface is in Arabic (right to left).

## Features

Patient:
- Register, log in, and take a queue number for today
- See the current number, own number, and how many patients are ahead (refreshes every 10 seconds)
- Cancel a waiting booking and view past bookings

Doctor:
- Dashboard with the current patient, the next patient, and today's queue
- Call the next patient, skip, cancel, or return a skipped patient to the queue
- Open or close bookings, and edit working hours and the daily booking limit
- Booking history filtered by date range, status, and patient name or phone

Main rules:
- Queue numbers are per day and increase sequentially
- A patient can hold only one active booking at a time
- Booking closes when the daily limit is reached, after the doctor's end-of-day time,
  or when the doctor closes it manually
- Unhandled bookings from previous days are marked as expired

## Tech Stack

- ASP.NET Core MVC (C#)
- Entity Framework Core (Code First, migrations)
- SQL Server
- Cookie authentication with roles
- Bootstrap 5 (RTL) and vanilla JavaScript

## Setup and Configuration

Requirements:
- .NET 10 SDK
- SQL Server (LocalDB is enough)
- Internet access, because Bootstrap, icons, and fonts are loaded from a CDN

Database connection:

The connection string is in `appsettings.json` and uses LocalDB by default.
To use another SQL Server, change the value of `DefaultConnection`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=ClinicQueueDb;Trusted_Connection=True;TrustServerCertificate=True"
}
```

No manual database step is needed. On startup the application applies the migrations,
creates the database if it does not exist, and inserts the initial data.

Initial data:

The seeder (`Data/DbSeeder.cs`) creates the doctor profile (name, specialty,
sub-specialties, location, working hours, daily limit) and a doctor account.
It only runs when the tables are empty. To change the doctor's fixed information,
edit the seeder and recreate the database with `dotnet ef database drop`.

## Running the Project

```bash
git clone <https://github.com/nadaabdullah815/Clinic-Queue-Management.git>
cd <ClinicQueue>
dotnet run
```

Open the address printed in the console (for example `http://localhost:5127`).

Demo doctor account:
- Phone: `0999999999`
- Password: `Doctor@123`

Patients create their own accounts from the registration page (phone format `09XXXXXXXX`).
Change the demo password and the connection string before any real deployment.

Quick test:
1. Register a patient and take a turn.
2. In a second browser (or a private window), log in as the doctor.
3. Press the button to call the first patient.
4. The patient page updates within about 10 seconds.

Use two different browsers or a private window for the second user, because
browsers share the login cookie between normal tabs.

## Project Structure

```
Controllers/   Account, Home, Booking (patient), DoctorQueue (doctor)
Services/      PatientQueueService, DoctorQueueService, DoctorProvider (interfaces in Services/Interfaces)
Helpers/       Shared queue helpers and the system clock
Models/        Entities and enums
ViewModels/    Data prepared for the views
Data/          Database context and seeder
Migrations/    EF Core migrations
Views/         Razor views
wwwroot/       CSS and JavaScript
```

## Main Technical Decisions

- Queue instead of appointments. Each day has sequential numbers, and every booking moves
  through clear statuses: waiting, in progress, done, skipped, cancelled, expired.
- Safe concurrent booking. A unique index on (doctor, date, queue number) makes the database
  reject duplicate numbers when two patients book at the same moment, and the service retries
  with the next number.
- Service layer split by role. Business rules live in `PatientQueueService` and
  `DoctorQueueService`, each behind an interface, with shared logic in a helper class.
  Controllers stay thin. Calling the next patient runs in a single transaction.
- Cookie authentication with roles (Patient, Doctor). Roles are assigned on the server only,
  so a user cannot choose their own role. Passwords are hashed with the ASP.NET Core
  password hasher, and all POST actions use anti-forgery tokens.
- Polling instead of SignalR for live updates. It is simpler and sufficient for a single clinic.
- Availability is computed, not stored. Booking is open unless the doctor closed it manually
  or the daily limit is reached, so it reopens automatically when a booking is cancelled.
- A single clock class (`QueueClock`) provides the current date and time, so the time zone
  can be configured in one place when deploying.

## Known Limitations

- Built for one doctor and one clinic.
- The current date uses the server clock, so the time zone should be set explicitly on a
  server in another region.
- Under extreme simultaneous booking, the daily limit could be exceeded by one booking.
- No automated tests yet.
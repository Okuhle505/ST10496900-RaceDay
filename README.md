# RaceDay


## Run

1. Update the `RaceDay` connection string in `src/RaceDay.Api/appsettings.json` for your SQL Server/LocalDB instance. The API runs the committed EF Core migrations automatically at startup. For a manual database update, run `dotnet ef database update --project src/RaceDay.Api`.
2. Open `RaceDay.sln` in Visual Studio 2022; set `RaceDay.Api` and `RaceDay.Web` as multiple startup projects.
3. Run the API (Swagger is at `/swagger`), then run the MVC web site.

The SQL script is the Part 1 hand-in schema; the API uses EF Core Code First migrations matching it. Do not use the sample password hashes to sign in; register accounts through the API.

For Docker, run `docker compose up --build`; the API listens on port 8080 and MVC on port 8081. Azure Blob Storage is represented by the `BannerImageUrl` and `ProfilePictureUrl` fields from the ERD; configure a storage account and save the returned blob URLs through the event/profile update endpoints before production submission.

# RaceDay - Part 2 RESTful API

RaceDay is a .NET 8 ASP.NET Core Web API for managing race events, age/distance categories, participant enrolments, and race results. It is designed for Visual Studio 2022 and uses SQL Server with Entity Framework Core Code First migrations.

The project follows the approved Part 1 ERD and REST API endpoint plan.

## Solution projects

| Project | Purpose |
|---|---|
| `src/RaceDay.Api` | ASP.NET Core REST API, EF Core data model, JWT/session authentication, Swagger |
| `tests/RaceDay.Tests` | xUnit unit tests for API success and failure paths |
| `src/RaceDay.Web` | MVC client which consumes the API only; it does not connect directly to SQL Server |

## Configure and run

1. Open `RaceDay.sln` in Visual Studio 2022.
2. Restore NuGet packages.
3. In `src/RaceDay.Api/appsettings.json`, update the `RaceDay` connection string if your SQL Server instance differs from LocalDB.
4. Build the solution.
5. Set `RaceDay.Api` and `RaceDay.Web` as **Multiple startup projects**, both set to **Start**.
6. Press `F5`.

The API applies its EF Core migrations when it starts. Swagger is available at `https://localhost:7001/swagger`.

## Database and Code First migrations

The EF Core model implements the Part 1 entities: Users, Organizers, Participants, Events, Categories, Enrollments, and Results.

The schema includes primary keys, foreign keys, unique usernames/emails, unique participant/category enrolments, and one result per enrolment. Relationship deletes use SQL Server `NO ACTION` rules to prevent SQL Server multiple-cascade-path errors.

The Part 1 SQL reference script is at `docs/RaceDayDatabase.sql`. EF Core migrations are in `src/RaceDay.Api/Migrations`.

## Authentication and authorisation

Users register as either an `Organiser` or a `Participant` through `POST /api/auth/register`.

- Passwords are hashed with BCrypt; plain-text passwords are never stored.
- Login accepts a username or email plus password.
- Successful registration and login create an HTTP-only server session containing user ID, username, and role.
- The API also returns a JWT Bearer token for Swagger and external API clients.
- Protected endpoints accept the authenticated session or a valid Bearer token.
- Organiser and Participant actions use role-based `[Authorize]` checks.

There are no usable default accounts. Register an account through Swagger before logging in to the MVC site.

## API endpoints

| Area | Endpoints |
|---|---|
| Authentication | `POST /api/auth/register`, `POST /api/auth/login` |
| Profile | `GET /api/users/profile`, `PUT /api/users/profile` |
| Events | `GET /api/events`, `GET /api/events/{id}`, `POST /api/events`, `PUT /api/events/{id}`, `DELETE /api/events/{id}` |
| Categories | `GET /api/events/{eventId}/categories`, `POST /api/events/{eventId}/categories`, `PUT /api/categories/{id}`, `DELETE /api/categories/{id}` |
| Enrolments | `POST /api/enrollments`, `GET /api/enrollments`, `GET /api/events/{eventId}/enrollments`, `DELETE /api/enrollments/{id}` |
| Results | `POST /api/results`, `PUT /api/results/{id}`, `GET /api/results`, `GET /api/events/{eventId}/results` |

Public users can browse events and categories. Creating/managing events, categories, event enrolments, and results is restricted to Organisers. Enrolling, cancelling enrolments, and viewing personal results is restricted to Participants.

## Testing

The xUnit tests are in `tests/RaceDay.Tests`. They cover registration, duplicate accounts, valid/invalid login, password hashing, endpoint-plan coverage, invalid events, duplicate enrolments, and public event retrieval. Test fixtures use Jarred-focused data.

## Continuous integration

GitHub Actions is configured in `.github/workflows/build.yml`. On every push and pull request it restores dependencies, builds the solution in Release configuration, and runs the test suite.

## MVC client

The MVC project uses `HttpClient` to call the REST API and does not use EF Core or SQL Server directly. Navigation is role-specific: Participants see **My enrolments** and **My results**; Organisers see **Organiser dashboard**. Unauthenticated users are redirected to Login instead of receiving a raw API 401 error.

## Troubleshooting

- **Connection refused to localhost:7001:** Start `RaceDay.Api` before or alongside `RaceDay.Web`.
- **Database/migration error:** Confirm the connection string points to a running SQL Server instance. For a disposable development database, drop `RaceDayDb` and start the API again to apply migrations from scratch.
- **401 on a participant/organiser page:** Log in with an account registered for the matching role.

## Docker

`docker-compose.yml` is included for local containerised development:

```powershell
docker compose up --build
```

The API is exposed on port 8080 and the MVC application on port 8081.

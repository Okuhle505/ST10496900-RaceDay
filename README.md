# RaceDay

Visual Studio 2022-ready .NET 8 solution for the RaceDay assignment.

## Run

1. Update the `RaceDay` connection string in `src/RaceDay.Api/appsettings.json` for your SQL Server/LocalDB instance. The API runs the committed EF Core migrations automatically at startup. For a manual database update, run `dotnet ef database update --project src/RaceDay.Api`.
2. Open `RaceDay.sln` in Visual Studio 2022; set `RaceDay.Api` and `RaceDay.Web` as multiple startup projects.
3. Run the API (Swagger is at `/swagger`), then run the MVC web site.

The SQL script is the Part 1 hand-in schema; the API uses EF Core Code First migrations matching it. Do not use the sample password hashes to sign in; register accounts through the API.

For Docker, run `docker compose up --build`; the API listens on port 8080 and MVC on port 8081. Azure Blob Storage is represented by the `BannerImageUrl` and `ProfilePictureUrl` fields from the ERD; configure a storage account and save the returned blob URLs through the event/profile update endpoints before production submission.

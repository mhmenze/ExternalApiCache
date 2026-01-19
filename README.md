# Project Structure
Based on the **ASP.NET Core WebAPI template**.

## Overview
- **Single Project** organized into:
  - **Controllers**
  - **Models**
  - **Services**

## Services
- **ExternalAPIService** – Fetch Data from Rick and Morty Characters public API.
- **LocalCacheService** – Manages database operations.

## Models
- Kept simple and made **Domain Class, API response and DB columns** have similar structure.
- **DTOs not used** due to the simple domain structure and time constraints.

## Caching
- Two APIs exposed:
  - **GetCharacterById** - Caching Implemented. If ID not in DB, it will be fetched by API and saved to DB, then displayed.
  - **GetAllCharacters** - Caching not Implemented. Fetches all records from DB (not external API).


### Reason for not implementing caching in GetAllCharacters:

Made an assumption that "get a list of records" means get all records. 

**Options considered:**

1. **Get All with caching** - If all records are fetched and saved to DB once, the external API wont be called again. This creates a copy of the API data in the DB, which is not what we want.

2. **Paginated results with caching** - Need to check which specific IDs on each page exist in the DB and fetching only missing ones. Ignored this as this is too complex for the 3 hour task.

3. **Get multiple by IDs** - User passes multiple Ids to the API. As it is very similar to the existing GetById, skipped this.

**Decision:** Implemented GetAll to return DB cached records only.

## Frontend
- Did not use any frontend framework/UI.
- Reasons:
	- The task requirement doesn't require a UI, APIs can be tested via Swagger/Postman.
	- Time constraint of 3 hours for the task.

## Database
- DB Schema File In "ExternalApiCache\SQL\CreationQueries.sql".
- Stored Procedures in "ExternalApiCache\SQL\StoredProcedures.sql".

# Running the Solution

## Prerequisites
- .NET 8 SDK
- MS SQL Server

## Step 1 - Create an MS SQL Server Instance
Make sure MS SQL Server is installed and running.

## Step 2 - Execute the CreationQueries.sql
Run the script located at "repo\SQL\CreationQueries.sql" to create the DB, schema and tables.

**Using Terminal (from repository root):**

```bash
sqlcmd -S localhost\SQLEXPRESS -i "SQL/CreationQueries.sql"
```

## Step 3 - Execute the StoredProcedures.sql
Run the script located at "repo\SQL\StoredProcedures.sql" to create all stored procedures.

**Using Terminal (from repository root):**
```bash
sqlcmd -S localhost\SQLEXPRESS -i "SQL/StoredProcedures.sql"
```

**You may use MS SQL Server Management Studio to Execute these SQL queries too without terminal.**

## Step 4 - Configure appsettings.json
Add your connection string to appsettings.json like shown in below example:
```json
{
    "ConnectionStrings": {
        "DefaultConnection": "Server=DESKTOP-DFVTI21\\SQLEXPRESS;Database=ExternalApiCache;Trusted_Connection=True;Encrypt=False;"
    },
    "RickAndMortyApi": {
        "BaseUrl": "https://rickandmortyapi.com/api/"
    },
    "Logging": {
        "LogLevel": {
            "Default": "Information",
            "Microsoft.AspNetCore": "Warning"
        }
    },
    "AllowedHosts": "*"
}
```

## Step 5 - Build and Run
Open terminal in Project root ("repo\ExternalApiCache\ExternalApiCache") and run:
```bash
dotnet restore
dotnet run --launch-profile https
```

The app will display the URLs it's running on. Note these ports for the next step.

## Step 6 - Test the APIs
Use the ports shown in your terminal output like in below example :

- **API 1:** `http://localhost:5087/character/all` - All Data from DB
- **API 2:** `http://localhost:5087/character/{id}` - If Id in DB, get from cache. Else get from API and cache in DB.

Alternatively, you may use Swagger UI (check the https port in the terminal and replace with given example): `https://localhost:7092/swagger/index.html`
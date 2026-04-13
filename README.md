# data2410-api-v1

A simple ASP.NET Core Web API for managing students, built with .NET 10 and Azure SQL.

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [SQL Server 2025 Developer](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) (Download **Standard Developer edition** — free for development and educational use)

## Clone the Repository

```bash
git clone https://github.com/YOUR_USERNAME/data2410-api-v1.git
cd data2410-api-v1
```

## Local Database Setup

Download and install [SQL Server 2025 Developer](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) — select the **Standard Developer edition** which is **free for development and educational use**.

You can also install [SQL Server Management Studio (SSMS)](https://learn.microsoft.com/en-us/ssms/download-sql-server-management-studio-ssms) to manage your database visually.

## Configure the Connection String

Create a file named `appsettings.Development.json` in the project root (this file is gitignored):

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=StudentsDb;User ID=sa;Password=YOUR_PASSWORD;TrustServerCertificate=True;"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

> **Note:** Replace `sa` and `YOUR_PASSWORD` with the SQL Server credentials you configured during installation. If your SQL Server uses a named instance, update the `Server` value accordingly, e.g. `Server=localhost\\MSSQLSERVER`.

The `Students` table is created automatically on startup — no manual database setup needed.

## Run the Project

```bash
dotnet restore
dotnet run
```

The API will start at `https://localhost:7010`.

## API Documentation

Once running, open the interactive API docs at:

```
https://localhost:7010/scalar/v1
```

## API Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/Students` | Get all students |
| GET | `/api/Students/{id}` | Get a student by ID |
| POST | `/api/Students` | Create a new student |
| PUT | `/api/Students/{id}` | Update a student |
| DELETE | `/api/Students/{id}` | Delete a student |
| POST | `/api/Students/calculate-grades` | Calculate grades for all students |
| GET | `/api/Students/report` | Generate course wise report of results |
| GET | `/health` | Check database connectivity |

### Example: Create a Student

Using **Scalar** (built-in at `/scalar/v1`) or **[Postman](https://www.postman.com/downloads/)**, send a `POST` request to:

```
POST https://localhost:7010/api/Students
```

With the following JSON body:

```json
{
  "name": "John Doe",
  "course": "DATA2410",
  "marks": 85
}
```


## Project Structure

```
data2410-api-v1/
??? Controllers/
?   ??? StudentsController.cs    # API endpoints
??? Models/
?   ??? Student.cs               # Student model
??? Properties/
?   ??? launchSettings.json      # Local launch configuration
??? appsettings.json             # App configuration (no secrets)
??? appsettings.Development.json # Local secrets (gitignored)
??? Program.cs                   # Application entry point
??? data2410-api-v1.csproj       # Project file
```

## Tech Stack

- **Framework:** ASP.NET Core (.NET 10)
- **Database:** SQL Server / Azure SQL
- **Data Access:** Microsoft.Data.SqlClient (raw ADO.NET)
- **API Docs:** Scalar (OpenAPI)

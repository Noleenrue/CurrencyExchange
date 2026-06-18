COURSE NAME: Network A pplication Development
PROJECT TITTLE: Currency Exchange
AUTHOR NAME: Noleen Ruvimbo Tsanyau
STUDENT ID NUMBER: 73936

**System Architecture and Functionality**

The Currency Exchange Office System is built using a three-tier architecture consisting of a Blazor user interface, an ASP.NET Core Web API backend, and a SQL Server database. The Blazor application serves as the presentation layer, allowing users to register, view exchange rates, manage their balances, and perform currency exchange operations. The frontend communicates with the Web API through HTTP requests using HttpClient.

The Web API implements the business logic of the system, including user management, wallet management, currency transactions, and exchange calculations. The API integrates with the National Bank of Poland (NBP) API to retrieve current and historical exchange rates, ensuring that all currency conversions use up-to-date market data.

The database layer uses Entity Framework Core and SQL Server to store user accounts, currency information, wallet balances, and transaction history. This ensures data persistence and allows users to track previous exchange operations.

The main functionalities of the system include user registration and authentication, viewing current and historical exchange rates, managing virtual currency balances, buying and selling currencies, and maintaining a complete transaction history. The architecture follows a clean separation of concerns, making the system scalable, maintainable, and easy to extend with additional features in the future.


Instructions for Running the Project

Prerequisites

Before running the application, ensure the following software is installed:

* .NET 8 SDK
* SQL Server or SQL Server Express
* Visual Studio 2022 (with ASP.NET and Web Development workload)
* Entity Framework Core Tools

Step 1: Clone or Open the Solution

Open the solution in Visual Studio and restore all NuGet packages.

Step 2: Configure the Database

Update the connection string in the `appsettings.json` file:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=YOUR_SERVER;Database=CurrencyExchangeDB;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

Step 3: Create the Database

Open the Package Manager Console or Terminal and execute:

```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

These commands create the database schema and apply all Entity Framework Core migrations.

Step 4: Run the Web API

Set the API project as the startup project and run it using:

```bash
dotnet run
```

or press **F5** in Visual Studio.

The API will start and expose endpoints for user management, currencies, wallets, transactions, and exchange rate operations.

Step 5: Run the Blazor Application

Set the Blazor project as the startup project and run it.

The application will open in a web browser and connect to the Web API through HTTP requests.

Step 6: Verify Functionality

After startup, test the following features:

* User registration and login
* Viewing current exchange rates
* Viewing historical exchange rates
* Wallet balance management
* Buying currencies
* Selling currencies
* Viewing transaction history

External API Integration

The system retrieves current and historical currency exchange rates from the National Bank of Poland (NBP) API. An active internet connection is required for exchange rate updates.

Default Workflow

1. Register a new user account.
2. Add funds to the PLN wallet.
3. View available exchange rates.
4. Buy or sell selected currencies.
5. Review transaction history and account balances.

If all operations complete successfully, the Currency Exchange Office System is functioning correctly.


# Mobile Provider Billing API

This project is a RESTful API built with ASP.NET Core 8.0 that manages mobile service provider billing operations. It provides comprehensive billing management for subscribers, including usage tracking, bill calculation, and payment processing.

## Features

- 🔄 API Versioning (V1 and V2)
- 📱 Subscriber Management
- 💰 Bill Generation and Management
- 📊 Usage Tracking (Phone and Internet)
- 💳 Payment Processing
- 📄 Detailed Bill Querying with Pagination
- 📈 Bill Statistics and Summaries (V2)
- 🔍 Swagger Documentation
- 🗃️ Azure SQL Database Integration
- 🪵 Comprehensive Logging

## Technology Stack

- ASP.NET Core 8.0
- Entity Framework Core 9.0.4
- SQL Server (Azure SQL)
- Swagger/OpenAPI
- Azure Cloud Services

## Prerequisites

- .NET 8.0 SDK
- SQL Server/Azure SQL Database
- Visual Studio 2022 or VS Code

## API Endpoints

### V1 Endpoints (api/v1/Bill)

#### GET Operations
- `GET /`: Get all bills
- `GET /QueryBill`: Query basic bill information
- `GET /QueryBillDetailed`: Query detailed bill information with pagination

#### POST Operations
- `POST /`: Create a new bill
- `POST /CalculateBill`: Calculate bill based on usage
- `POST /PayBill`: Process bill payment

### V2 Endpoints (api/v2/Bill)

#### Enhanced Features
- `GET /`: Get all bills (sorted by issue date)
- `GET /ByDateRange`: Get bills within a date range
- `GET /Summary`: Get billing statistics and summaries

## Billing Rules

### Phone Usage
- First 1000 minutes free
- ₺10 per 1000 minutes after free quota

### Internet Usage
- Base rate: ₺50 for first 20GB
- Additional: ₺10 per 10GB block
- Usage measured in GB


## Error Handling

The API implements comprehensive error handling:
- Validation errors
- Not found errors
- Business logic errors
- Database errors
- Server errors

All errors return appropriate HTTP status codes and detailed error messages.

## Logging

The application includes detailed logging:
- Request/Response logging
- Error logging
- Database operation logging
- Business operation logging

## Security

- HTTPS enabled
- Input validation
- SQL injection protection via Entity Framework
- Data validation using Data Annotations

## Contributing

1. Fork the repository
2. Create your feature branch
3. Commit your changes
4. Push to the branch
5. Create a new Pull Request

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Author

UMUT YILMAZ

## Acknowledgments

- ASP.NET Core team
- Entity Framework Core team
- Azure team

# LibSys — Library Management System

ASP.NET Core 8 Web API + MySQL + Docker + Azure Container Apps

## Tech Stack
- **Backend:** ASP.NET Core 8 Web API
- **ORM:** Entity Framework Core 8 + Pomelo MySQL
- **Auth:** ASP.NET Core Identity + JWT Bearer
- **Database:** MySQL (Azure Database for MySQL)
- **Deploy:** Docker → Azure Container Apps
- **CI/CD:** GitHub Actions

## Getting Started

### Prerequisites
- .NET 8 SDK
- Docker Desktop
- MySQL (local หรือ Azure)

### 1. Clone และ setup
```bash
git clone https://github.com/YOUR_USERNAME/libsys.git
cd libsys/LibSys.API
```

### 2. แก้ไข appsettings.json
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=libsys;User=root;Password=yourpassword;"
  },
  "Jwt": {
    "Key": "YourSuperSecretKeyAtLeast32CharactersLong!",
    "Issuer": "LibSys",
    "Audience": "LibSysUsers"
  }
}
```

### 3. Run Migrations
```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### 4. Run locally
```bash
dotnet run
# Swagger UI: https://localhost:5001/swagger
```

### 5. Build Docker image
```bash
docker build -t libsys-api .
docker run -p 8080:8080 \
  -e ConnectionStrings__DefaultConnection="Server=host.docker.internal;..." \
  -e Jwt__Key="YourSecretKey" \
  libsys-api
```

## Deploy to Azure

### Setup Azure resources
```bash
# Login
az login

# Create Resource Group
az group create --name libsys-rg --location southeastasia

# Create MySQL server
az mysql flexible-server create \
  --resource-group libsys-rg \
  --name libsys-mysql \
  --admin-user libsysadmin \
  --admin-password "YourPassword@123" \
  --sku-name Standard_B1ms \
  --tier Burstable \
  --public-access All

# Create database
az mysql flexible-server db create \
  --resource-group libsys-rg \
  --server-name libsys-mysql \
  --database-name libsys

# Create Container Apps Environment
az containerapp env create \
  --name libsys-env \
  --resource-group libsys-rg \
  --location southeastasia

# Create Container App
az containerapp create \
  --name libsys-api \
  --resource-group libsys-rg \
  --environment libsys-env \
  --image docker.io/YOUR_DOCKERHUB_USERNAME/libsys-api:latest \
  --target-port 8080 \
  --ingress external \
  --min-replicas 0 \
  --max-replicas 3 \
  --env-vars \
    "ConnectionStrings__DefaultConnection=Server=libsys-mysql.mysql.database.azure.com;Database=libsys;User=libsysadmin;Password=YourPassword@123;SslMode=Required;" \
    "Jwt__Key=YourProductionSecretKey" \
    "Jwt__Issuer=LibSys" \
    "Jwt__Audience=LibSysUsers"
```

### GitHub Secrets ที่ต้องตั้ง
| Secret | ค่า |
|--------|-----|
| `DOCKERHUB_USERNAME` | Docker Hub username |
| `DOCKERHUB_TOKEN` | Docker Hub access token |
| `AZURE_CREDENTIALS` | JSON จาก `az ad sp create-for-rbac` |
| `AZURE_RESOURCE_GROUP` | `libsys-rg` |
| `DB_CONNECTION_STRING` | MySQL connection string |
| `JWT_KEY` | JWT secret key |

## API Endpoints

### Auth
- `POST /api/auth/register` — สมัครสมาชิก
- `POST /api/auth/login` — เข้าสู่ระบบ

### Books (Public)
- `GET /api/books` — รายการหนังสือ (รองรับ search, category, pagination)
- `GET /api/books/{id}` — รายละเอียดหนังสือ

### Books (Admin only)
- `POST /api/books` — เพิ่มหนังสือ
- `PUT /api/books/{id}` — แก้ไขหนังสือ
- `DELETE /api/books/{id}` — ลบหนังสือ

### Borrowings
- `POST /api/borrowings` — ยืมหนังสือ
- `PUT /api/borrowings/{id}/return` — คืนหนังสือ
- `GET /api/borrowings/my` — ประวัติการยืมของตัวเอง
- `GET /api/borrowings` — รายการยืมทั้งหมด (Admin)

### Admin
- `GET /api/admin/dashboard` — สถิติและ Dashboard

## Default Admin Account
- Email: `admin@libsys.com`
- Password: `Admin@1234`

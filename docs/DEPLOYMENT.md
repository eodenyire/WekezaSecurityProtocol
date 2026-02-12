# Salama Security Protocol - Deployment Guide

## Prerequisites

- .NET 8 SDK
- PostgreSQL 15+
- Docker (optional, for containerized deployment)
- Kubernetes cluster (for production deployment)

## Local Development Setup

### 1. Install Dependencies

```bash
# Install .NET 8 SDK
# https://dotnet.microsoft.com/download/dotnet/8.0

# Verify installation
dotnet --version
```

### 2. Database Setup

```bash
# Install PostgreSQL
# https://www.postgresql.org/download/

# Create database
createdb salama_security_dev

# Run migrations (once implemented)
dotnet ef database update
```

### 3. Configuration

Edit `appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=salama_security_dev;Username=your_user;Password=your_password"
  },
  "JwtSettings": {
    "SecretKey": "your-development-secret-key-minimum-32-chars",
    "Issuer": "https://localhost:5001",
    "Audience": "https://localhost:5001"
  }
}
```

### 4. Run the Application

```bash
# Restore packages
dotnet restore

# Build
dotnet build

# Run
dotnet run --project WekezaSecurityProtocol.csproj

# Or use watch mode for development
dotnet watch run
```

The API will be available at:
- HTTPS: https://localhost:5001
- HTTP: http://localhost:5000
- Swagger UI: https://localhost:5001/swagger

## Docker Deployment

### Build Docker Image

```dockerfile
# Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["WekezaSecurityProtocol.csproj", "./"]
RUN dotnet restore "WekezaSecurityProtocol.csproj"
COPY . .
RUN dotnet build "WekezaSecurityProtocol.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "WekezaSecurityProtocol.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WekezaSecurityProtocol.dll"]
```

### Build and Run

```bash
# Build image
docker build -t wekeza-salama-protocol:latest .

# Run container
docker run -d \
  -p 8080:80 \
  -p 8443:443 \
  -e ConnectionStrings__DefaultConnection="Host=db;Database=salama_security;Username=wekeza;Password=secure_password" \
  -e JwtSettings__SecretKey="production-secret-key-minimum-32-characters-long" \
  --name salama-protocol \
  wekeza-salama-protocol:latest
```

### Docker Compose

```yaml
version: '3.8'

services:
  postgres:
    image: postgres:15
    environment:
      POSTGRES_DB: salama_security
      POSTGRES_USER: wekeza_user
      POSTGRES_PASSWORD: secure_password
    volumes:
      - postgres_data:/var/lib/postgresql/data
    ports:
      - "5432:5432"

  salama-api:
    build: .
    ports:
      - "8080:80"
      - "8443:443"
    environment:
      - ConnectionStrings__DefaultConnection=Host=postgres;Database=salama_security;Username=wekeza_user;Password=secure_password
      - JwtSettings__SecretKey=${JWT_SECRET_KEY}
      - ASPNETCORE_ENVIRONMENT=Production
    depends_on:
      - postgres

volumes:
  postgres_data:
```

Run with:
```bash
docker-compose up -d
```

## Kubernetes Deployment

### 1. Create Namespace

```bash
kubectl create namespace salama-protocol
```

### 2. Create Secrets

```bash
# Database credentials
kubectl create secret generic db-credentials \
  --from-literal=username=wekeza_user \
  --from-literal=password=secure_password \
  -n salama-protocol

# JWT secret
kubectl create secret generic jwt-secret \
  --from-literal=secretKey=your-production-secret-key-32-chars-minimum \
  -n salama-protocol

# Wekeza API keys
kubectl create secret generic api-keys \
  --from-literal=core-api-key=your-core-api-key \
  --from-literal=soc-api-key=your-soc-api-key \
  -n salama-protocol
```

### 3. Deploy PostgreSQL

```yaml
# postgres-deployment.yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: postgres
  namespace: salama-protocol
spec:
  replicas: 1
  selector:
    matchLabels:
      app: postgres
  template:
    metadata:
      labels:
        app: postgres
    spec:
      containers:
      - name: postgres
        image: postgres:15
        env:
        - name: POSTGRES_DB
          value: salama_security
        - name: POSTGRES_USER
          valueFrom:
            secretKeyRef:
              name: db-credentials
              key: username
        - name: POSTGRES_PASSWORD
          valueFrom:
            secretKeyRef:
              name: db-credentials
              key: password
        ports:
        - containerPort: 5432
        volumeMounts:
        - name: postgres-storage
          mountPath: /var/lib/postgresql/data
      volumes:
      - name: postgres-storage
        persistentVolumeClaim:
          claimName: postgres-pvc
---
apiVersion: v1
kind: Service
metadata:
  name: postgres
  namespace: salama-protocol
spec:
  selector:
    app: postgres
  ports:
  - port: 5432
    targetPort: 5432
```

### 4. Deploy Application

```yaml
# salama-deployment.yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: salama-protocol
  namespace: salama-protocol
spec:
  replicas: 3
  selector:
    matchLabels:
      app: salama-protocol
  template:
    metadata:
      labels:
        app: salama-protocol
    spec:
      containers:
      - name: salama-api
        image: wekeza-salama-protocol:latest
        ports:
        - containerPort: 80
        - containerPort: 443
        env:
        - name: ConnectionStrings__DefaultConnection
          value: "Host=postgres;Database=salama_security;Username=$(DB_USER);Password=$(DB_PASSWORD)"
        - name: DB_USER
          valueFrom:
            secretKeyRef:
              name: db-credentials
              key: username
        - name: DB_PASSWORD
          valueFrom:
            secretKeyRef:
              name: db-credentials
              key: password
        - name: JwtSettings__SecretKey
          valueFrom:
            secretKeyRef:
              name: jwt-secret
              key: secretKey
        - name: ASPNETCORE_ENVIRONMENT
          value: "Production"
        resources:
          requests:
            memory: "256Mi"
            cpu: "250m"
          limits:
            memory: "512Mi"
            cpu: "500m"
        livenessProbe:
          httpGet:
            path: /health
            port: 80
          initialDelaySeconds: 30
          periodSeconds: 10
        readinessProbe:
          httpGet:
            path: /health
            port: 80
          initialDelaySeconds: 5
          periodSeconds: 5
---
apiVersion: v1
kind: Service
metadata:
  name: salama-protocol
  namespace: salama-protocol
spec:
  type: LoadBalancer
  selector:
    app: salama-protocol
  ports:
  - name: http
    port: 80
    targetPort: 80
  - name: https
    port: 443
    targetPort: 443
```

### 5. Deploy

```bash
kubectl apply -f postgres-deployment.yaml
kubectl apply -f salama-deployment.yaml

# Check status
kubectl get pods -n salama-protocol
kubectl get services -n salama-protocol
```

## Production Checklist

### Security
- [ ] Use strong JWT secret (minimum 32 characters, random)
- [ ] Enable HTTPS with valid SSL certificate
- [ ] Configure certificate pinning
- [ ] Enable database encryption at rest
- [ ] Set up database backups
- [ ] Configure firewall rules
- [ ] Enable audit logging
- [ ] Set up intrusion detection

### Monitoring
- [ ] Configure application logging (e.g., Serilog to ELK stack)
- [ ] Set up APM (Application Performance Monitoring)
- [ ] Configure alerts for SOC
- [ ] Set up database monitoring
- [ ] Configure health checks
- [ ] Set up uptime monitoring

### Scaling
- [ ] Configure auto-scaling (HPA in Kubernetes)
- [ ] Set up load balancer
- [ ] Configure database connection pooling
- [ ] Enable caching where appropriate
- [ ] Configure CDN for static assets

### Compliance
- [ ] Review ODPC privacy requirements
- [ ] Ensure CBK compliance flags are set
- [ ] Configure data retention policies
- [ ] Set up compliance reporting
- [ ] Document incident response procedures

## Environment Variables

Required environment variables for production:

```bash
# Database
ConnectionStrings__DefaultConnection="Host=prod-db;Database=salama_security;Username=wekeza;Password=***"

# JWT
JwtSettings__SecretKey="***"
JwtSettings__Issuer="https://api.wekeza.com/security"
JwtSettings__Audience="https://wekeza.com"

# Wekeza APIs
WekezaApiSettings__CoreApiBaseUrl="https://api.wekeza.com/core"
WekezaApiSettings__ComprehensiveApiBaseUrl="https://api.wekeza.com/comprehensive"
WekezaApiSettings__Mvp4ApiBaseUrl="https://api.wekeza.com/mvp4"

# SOC Alerts
SocAlertSettings__AlertEndpoint="https://soc.wekeza.com/api/alerts"
SocAlertSettings__AlertApiKey="***"

# Application
ASPNETCORE_ENVIRONMENT="Production"
ASPNETCORE_URLS="https://+:443;http://+:80"
```

## Troubleshooting

### Database Connection Issues
```bash
# Test database connection
psql -h localhost -U wekeza_user -d salama_security

# Check logs
kubectl logs -n salama-protocol deployment/salama-protocol
```

### JWT Token Issues
- Verify secret key is at least 32 characters
- Check token expiration settings
- Validate issuer/audience configuration

### Performance Issues
- Check database query performance
- Review connection pool settings
- Monitor memory usage
- Check for N+1 query problems

## Support

For deployment support:
- DevOps Team: devops@wekeza.com
- Documentation: https://docs.wekeza.com/deployment
- Emergency: +254-XXX-XXXX (24/7)

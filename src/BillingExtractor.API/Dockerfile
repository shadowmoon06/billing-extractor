# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /src

# Copy solution and project files
COPY src/BillingExtractor.API/BillingExtractor.API.csproj src/BillingExtractor.API/
COPY src/BillingExtractor.Business/BillingExtractor.Business.csproj src/BillingExtractor.Business/
COPY src/BillingExtractor.Data/BillingExtractor.Data.csproj src/BillingExtractor.Data/

# Restore dependencies
RUN dotnet restore src/BillingExtractor.API/BillingExtractor.API.csproj

# Copy source code
COPY src/ src/

# Build and publish
WORKDIR /src/src/BillingExtractor.API
RUN dotnet publish -c Release -o /app/publish --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS runtime
WORKDIR /app

# Create uploads directory
RUN mkdir -p /app/Uploads/Invoices

# Copy published files
COPY --from=build /app/publish .

# Copy prompts folder (needed for AI extraction)
COPY src/BillingExtractor.Business/Prompts/ /app/Prompts/

# Set environment
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_HTTP_PORTS=8080

EXPOSE 8080

ENTRYPOINT ["dotnet", "BillingExtractor.API.dll"]

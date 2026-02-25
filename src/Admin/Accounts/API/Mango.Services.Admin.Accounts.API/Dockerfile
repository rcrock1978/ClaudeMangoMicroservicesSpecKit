FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution file and project files
COPY ["Mango.Microservices.slnx", "./"]
COPY ["src/Admin/Accounts/Domain/Mango.Services.Admin.Accounts.Domain/Mango.Services.Admin.Accounts.Domain.csproj", "src/Admin/Accounts/Domain/Mango.Services.Admin.Accounts.Domain/"]
COPY ["src/Admin/Accounts/Application/Mango.Services.Admin.Accounts.Application/Mango.Services.Admin.Accounts.Application.csproj", "src/Admin/Accounts/Application/Mango.Services.Admin.Accounts.Application/"]
COPY ["src/Admin/Accounts/Infrastructure/Mango.Services.Admin.Accounts.Infrastructure/Mango.Services.Admin.Accounts.Infrastructure.csproj", "src/Admin/Accounts/Infrastructure/Mango.Services.Admin.Accounts.Infrastructure/"]
COPY ["src/Admin/Accounts/API/Mango.Services.Admin.Accounts.API/Mango.Services.Admin.Accounts.API.csproj", "src/Admin/Accounts/API/Mango.Services.Admin.Accounts.API/"]

# Restore dependencies
RUN dotnet restore "src/Admin/Accounts/API/Mango.Services.Admin.Accounts.API/Mango.Services.Admin.Accounts.API.csproj"

# Copy project files
COPY ["src/Admin/Accounts/Domain/Mango.Services.Admin.Accounts.Domain/", "src/Admin/Accounts/Domain/Mango.Services.Admin.Accounts.Domain/"]
COPY ["src/Admin/Accounts/Application/Mango.Services.Admin.Accounts.Application/", "src/Admin/Accounts/Application/Mango.Services.Admin.Accounts.Application/"]
COPY ["src/Admin/Accounts/Infrastructure/Mango.Services.Admin.Accounts.Infrastructure/", "src/Admin/Accounts/Infrastructure/Mango.Services.Admin.Accounts.Infrastructure/"]
COPY ["src/Admin/Accounts/API/Mango.Services.Admin.Accounts.API/", "src/Admin/Accounts/API/Mango.Services.Admin.Accounts.API/"]

# Build
RUN dotnet build "src/Admin/Accounts/API/Mango.Services.Admin.Accounts.API/Mango.Services.Admin.Accounts.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "src/Admin/Accounts/API/Mango.Services.Admin.Accounts.API/Mango.Services.Admin.Accounts.API.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=publish /app/publish .

EXPOSE 5094
ENTRYPOINT ["dotnet", "Mango.Services.Admin.Accounts.API.dll"]

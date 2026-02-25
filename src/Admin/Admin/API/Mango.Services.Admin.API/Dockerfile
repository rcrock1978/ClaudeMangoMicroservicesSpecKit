FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution file and project files
COPY ["Mango.Microservices.slnx", "./"]
COPY ["src/Admin/Admin/Domain/Mango.Services.Admin.Domain/Mango.Services.Admin.Domain.csproj", "src/Admin/Admin/Domain/Mango.Services.Admin.Domain/"]
COPY ["src/Admin/Admin/Application/Mango.Services.Admin.Application/Mango.Services.Admin.Application.csproj", "src/Admin/Admin/Application/Mango.Services.Admin.Application/"]
COPY ["src/Admin/Admin/Infrastructure/Mango.Services.Admin.Infrastructure/Mango.Services.Admin.Infrastructure.csproj", "src/Admin/Admin/Infrastructure/Mango.Services.Admin.Infrastructure/"]
COPY ["src/Admin/Admin/API/Mango.Services.Admin.API/Mango.Services.Admin.API.csproj", "src/Admin/Admin/API/Mango.Services.Admin.API/"]

# Restore dependencies
RUN dotnet restore "src/Admin/Admin/API/Mango.Services.Admin.API/Mango.Services.Admin.API.csproj"

# Copy project files
COPY ["src/Admin/Admin/Domain/Mango.Services.Admin.Domain/", "src/Admin/Admin/Domain/Mango.Services.Admin.Domain/"]
COPY ["src/Admin/Admin/Application/Mango.Services.Admin.Application/", "src/Admin/Admin/Application/Mango.Services.Admin.Application/"]
COPY ["src/Admin/Admin/Infrastructure/Mango.Services.Admin.Infrastructure/", "src/Admin/Admin/Infrastructure/Mango.Services.Admin.Infrastructure/"]
COPY ["src/Admin/Admin/API/Mango.Services.Admin.API/", "src/Admin/Admin/API/Mango.Services.Admin.API/"]

# Build
RUN dotnet build "src/Admin/Admin/API/Mango.Services.Admin.API/Mango.Services.Admin.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "src/Admin/Admin/API/Mango.Services.Admin.API/Mango.Services.Admin.API.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=publish /app/publish .

EXPOSE 5095
ENTRYPOINT ["dotnet", "Mango.Services.Admin.API.dll"]

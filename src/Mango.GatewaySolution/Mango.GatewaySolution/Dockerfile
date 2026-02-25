FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution and project file
COPY ["Mango.Microservices.slnx", "./"]
COPY ["src/Mango.GatewaySolution/Mango.GatewaySolution/Mango.GatewaySolution.csproj", "src/Mango.GatewaySolution/Mango.GatewaySolution/"]

# Restore dependencies
RUN dotnet restore "src/Mango.GatewaySolution/Mango.GatewaySolution/Mango.GatewaySolution.csproj"

# Copy project files
COPY ["src/Mango.GatewaySolution/Mango.GatewaySolution/", "src/Mango.GatewaySolution/Mango.GatewaySolution/"]

# Build
RUN dotnet build "src/Mango.GatewaySolution/Mango.GatewaySolution/Mango.GatewaySolution.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "src/Mango.GatewaySolution/Mango.GatewaySolution/Mango.GatewaySolution.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=publish /app/publish .

EXPOSE 5090
ENTRYPOINT ["dotnet", "Mango.GatewaySolution.dll"]

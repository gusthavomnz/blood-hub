FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY BloodHub.slnx ./
COPY src/BloodHub/BloodHub.csproj src/BloodHub/
RUN dotnet restore src/BloodHub/BloodHub.csproj

COPY src/ src/
RUN dotnet publish src/BloodHub/BloodHub.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/runtime:10.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

USER $APP_UID

ENTRYPOINT ["dotnet", "BloodHub.dll"]

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["skview_kepware_influx_service/skview_kepware_influx_service.csproj", "skview_kepware_influx_service/"]
RUN dotnet restore "skview_kepware_influx_service/skview_kepware_influx_service.csproj"
COPY . .
WORKDIR "/src/skview_kepware_influx_service"
RUN dotnet build "skview_kepware_influx_service.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "skview_kepware_influx_service.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "skview_kepware_influx_service.dll"]

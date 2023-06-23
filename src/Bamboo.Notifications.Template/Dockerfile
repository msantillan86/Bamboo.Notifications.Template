FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS publish
COPY . .
RUN dotnet publish "/src/Bamboo.Notifications.Template/Bamboo.Notifications.Template.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Bamboo.Notifications.Template.dll"]
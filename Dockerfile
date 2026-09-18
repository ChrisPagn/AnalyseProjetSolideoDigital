FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Couche de restore séparée du reste du code : ne se réinvalide que si un .csproj change,
# pas à chaque modification de code — accélère nettement les rebuilds répétés.
COPY Shared/Shared.csproj Shared/
COPY Api/Api.csproj Api/
COPY Client/Client.csproj Client/
RUN dotnet restore Api/Api.csproj
RUN dotnet restore Client/Client.csproj

COPY . .
RUN dotnet publish Client/Client.csproj -c Release -o /app/client-publish --no-restore
RUN dotnet publish Api/Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
ENV ASPNETCORE_HTTP_PORTS=8080
COPY --from=build /app/publish .
COPY --from=build /app/client-publish/wwwroot ./wwwroot
EXPOSE 8080
USER $APP_UID
ENTRYPOINT ["dotnet", "Api.dll"]

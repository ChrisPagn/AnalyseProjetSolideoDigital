FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish Client/Client.csproj -c Release -o /app/client-publish
RUN dotnet publish Api/Api.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
COPY --from=build /app/client-publish/wwwroot ./wwwroot
EXPOSE 8080
ENTRYPOINT ["dotnet", "Api.dll"]

FROM mcr.microsoft.com/dotnet/sdk:6.0-alpine AS build-env
WORKDIR /App

# Copy everything
COPY . ./
# Restore as distinct layers
RUN dotnet restore
# Build and publish a release
RUN dotnet publish -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:6.0-alpine
WORKDIR /App
COPY --from=build-env /App/out .

EXPOSE 1234 
ENTRYPOINT ["dotnet", "TcpListenerApp.dll"]

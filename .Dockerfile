#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 1788
EXPOSE 1789

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["SimpleQRGenerator.csproj", "."]
RUN dotnet restore "./SimpleQRGenerator.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "SimpleQRGenerator.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SimpleQRGenerator.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SimpleQRGenerator.dll"]
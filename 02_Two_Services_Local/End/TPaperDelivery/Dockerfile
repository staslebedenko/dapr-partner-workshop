#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:3.1 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:3.1 AS build
WORKDIR /src
COPY ["TPaperDelivery/TPaperDelivery.csproj", "TPaperDelivery/"]
RUN dotnet restore "TPaperDelivery/TPaperDelivery.csproj"
COPY . .
WORKDIR "/src/TPaperDelivery"
RUN dotnet build "TPaperDelivery.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TPaperDelivery.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TPaperDelivery.dll"]

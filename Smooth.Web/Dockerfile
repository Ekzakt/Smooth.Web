FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app
EXPOSE 443

COPY ./Smooth.Web/Smooth.Web.csproj ./Smooth.Web/
RUN dotnet restore "./Smooth.Web/Smooth.Web.csproj"

COPY . ./

RUN dotnet publish "./Smooth.Web/Smooth.Web.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "Smooth.Web.dll"]

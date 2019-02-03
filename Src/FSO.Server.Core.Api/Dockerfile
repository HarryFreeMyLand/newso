FROM microsoft/dotnet:2.2-aspnetcore-runtime AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM microsoft/dotnet:2.2-sdk AS build
WORKDIR /src
COPY ["FSO.Server.Core.Api/FSO.Server.Core.Api.csproj", "FSO.Server.Core.Api/"]
RUN dotnet restore "FSO.Server.Core.Api/FSO.Server.Core.Api.csproj"
COPY . .
WORKDIR "/src/FSO.Server.Core.Api"
RUN dotnet build "FSO.Server.Core.Api.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "FSO.Server.Core.Api.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "FSO.Server.Core.Api.dll"]
# 使用官方的 .NET 8.0 运行时镜像
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5000

# 使用官方的 .NET 8.0 SDK 镜像进行构建
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["GameFrameX.Grafana.LokiPush/GameFrameX.Grafana.LokiPush.csproj", "GameFrameX.Grafana.LokiPush/"]
RUN dotnet restore "GameFrameX.Grafana.LokiPush/GameFrameX.Grafana.LokiPush.csproj"
COPY . .
WORKDIR "/src/GameFrameX.Grafana.LokiPush"
RUN dotnet build "GameFrameX.Grafana.LokiPush.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "GameFrameX.Grafana.LokiPush.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# 设置默认环境变量
ENV ASPNETCORE_URLS=http://+:5000
ENV ASPNETCORE_ENVIRONMENT=Production

# 创建非root用户
RUN adduser --disabled-password --gecos '' appuser && chown -R appuser /app
USER appuser

ENTRYPOINT ["dotnet", "GameFrameX.Grafana.LokiPush.dll"]
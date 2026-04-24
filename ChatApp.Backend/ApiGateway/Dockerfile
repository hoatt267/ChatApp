# 1. Base image để chạy ứng dụng (Rất nhẹ)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# 2. Build image chứa SDK để biên dịch code (Nặng hơn)
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
ARG BUILD_CONFIGURATION=Release

# Copy các file .csproj để Restore các package trước (Tối ưu cache của Docker)
COPY ["ApiGateway/ApiGateway.csproj", "ApiGateway/"]
RUN dotnet restore "ApiGateway/ApiGateway.csproj"

# Copy toàn bộ code còn lại
COPY . .
WORKDIR "/src/ApiGateway"

# 3. Publish ứng dụng ra file chạy
FROM build AS publish
RUN dotnet publish "ApiGateway.csproj" -c $BUILD_CONFIGURATION -o /app/publish --no-restore /p:UseAppHost=false

# 4. Đóng gói vào Base image để tạo ra Image cuối cùng cực nhẹ
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ApiGateway.dll"]
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
COPY ["Services/IdentityService/IdentityService.API/IdentityService.API.csproj", "Services/IdentityService/IdentityService.API/"]
COPY ["Services/IdentityService/IdentityService.Application/IdentityService.Application.csproj", "Services/IdentityService/IdentityService.Application/"]
COPY ["Services/IdentityService/IdentityService.Infrastructure/IdentityService.Infrastructure.csproj", "Services/IdentityService/IdentityService.Infrastructure/"]
COPY ["Services/IdentityService/IdentityService.Domain/IdentityService.Domain.csproj", "Services/IdentityService/IdentityService.Domain/"]
COPY ["BuildingBlocks/ChatApp.Shared/ChatApp.Shared.csproj", "BuildingBlocks/ChatApp.Shared/"]
RUN dotnet restore "Services/IdentityService/IdentityService.API/IdentityService.API.csproj"

# Copy toàn bộ code còn lại
COPY . .
WORKDIR "/src/Services/IdentityService/IdentityService.API"

# 3. Publish ứng dụng ra file chạy
FROM build AS publish
RUN dotnet publish "IdentityService.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish --no-restore /p:UseAppHost=false

# 4. Đóng gói vào Base image để tạo ra Image cuối cùng cực nhẹ
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "IdentityService.API.dll"]
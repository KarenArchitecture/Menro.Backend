# مرحله build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# کپی همه فایل‌ها و build
COPY . .
RUN dotnet publish -c Release -o out

# مرحله runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/out .

# اجرای برنامه
ENTRYPOINT ["dotnet", "Menro.Web.dll"]

# -----------------------------
# Stage 1: Build
# -----------------------------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# csproj files copy + restore (layer cache കൊള്ളാൻ വേറിട്ടു copy ചെയ്യുന്നു)
COPY ["CreditTrack/CreditTrack.csproj", "CreditTrack/"]
COPY ["CreditTrack.Application/CreditTrack.Application.csproj", "CreditTrack.Application/"]
COPY ["CreditTrack.Domain/CreditTrack.Domain.csproj", "CreditTrack.Domain/"]
COPY ["CreditTrack.Infrastructure/CreditTrack.Infrastructure.csproj", "CreditTrack.Infrastructure/"]

RUN dotnet restore "CreditTrack/CreditTrack.csproj"

# ശേഷിക്കുന്ന source code എല്ലാം copy ചെയ്യുക
COPY . .

# API project folder-ലേക്ക് മാറി publish ചെയ്യുക
WORKDIR "/src/CreditTrack"
RUN dotnet publish "CreditTrack.csproj" -c Release -o /app/publish /p:UseAppHost=false

# -----------------------------
# Stage 2: Runtime
# -----------------------------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Railway PORT=8080 ഉപയോഗിക്കും; Kestrel 0.0.0.0:8080-ൽ listen ചെയ്യാൻ പറയാം
ENV ASPNETCORE_URLS=http://0.0.0.0:8080
EXPOSE 8080

COPY --from=build /app/publish .

# CreditTrack.dll എന്നത് നിന്റെ main API assembly name ആണ്
ENTRYPOINT ["dotnet", "CreditTrack.dll"]

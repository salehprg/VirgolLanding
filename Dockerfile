# https://hub.docker.com/_/microsoft-dotnet-core
FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ./ ./virgollanding

WORKDIR /src/virgollanding

# restore solution
RUN dotnet restore

# build project   
RUN dotnet build --no-restore -c Release

# publish project
# WORKDIR /src/Presentation/virgollanding.School 
RUN dotnet publish -c Release -o /app/published --self-contained false

# final stage/image
FROM mcr.microsoft.com/dotnet/aspnet:5.0

# RUN apt update -yq \
#     && apt install nano -yq
WORKDIR /app

COPY --from=build /app/published ./

EXPOSE 80 443

ENTRYPOINT ["./virgollanding"]
# ENTRYPOINT ["tail", "-f", "/dev/null"]

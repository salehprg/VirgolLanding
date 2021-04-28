# https://hub.docker.com/_/microsoft-dotnet-core
FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
WORKDIR /src
COPY ./ ./virgollanding

WORKDIR /src/virgollanding

# restore solution
RUN dotnet restore

# build project   
RUN dotnet build virgollanding --no-restore -c Release

# Fetch and install Node 12 LTS
ENV APT_KEY_DONT_WARN_ON_DANGEROUS_USAGE 1
RUN curl -sL https://deb.nodesource.com/setup_lts.x | bash -  
RUN apt install -y nodejs 
RUN nodejs -v
RUN npm -v

# publish project
# WORKDIR /src/Presentation/virgollanding.School 
RUN dotnet publish -c Release -o /app/published --self-contained false

# final stage/image
FROM mcr.microsoft.com/dotnet/core/aspnet:3.1

# RUN apt update -yq \
#     && apt install nano -yq
WORKDIR /app

COPY --from=build /app/published ./

EXPOSE 80 443

ENTRYPOINT ["./virgollanding.School"]
# ENTRYPOINT ["tail", "-f", "/dev/null"]


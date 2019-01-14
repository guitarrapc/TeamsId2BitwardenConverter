FROM microsoft/dotnet:2.2-sdk-bionic AS build
WORKDIR /app

# restore
COPY *.csproj ./
RUN dotnet restore

# build
COPY . /app
RUN dotnet build -c Release -o out
RUN dotnet publish -c Release -o out

# pack
WORKDIR /app
VOLUME /app/pack
CMD ["dotnet", "pack", "--include-symbols", "-c", "Release", "-o", "pack"]
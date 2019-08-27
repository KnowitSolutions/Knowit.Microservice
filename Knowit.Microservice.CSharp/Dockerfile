FROM mcr.microsoft.com/dotnet/core/sdk:3.0 AS builder
RUN curl -sS https://dl.yarnpkg.com/debian/pubkey.gpg | apt-key add - && \
    echo "deb https://dl.yarnpkg.com/debian/ stable main" | tee /etc/apt/sources.list.d/yarn.list && \
    apt-get update && \
    apt-get install --yes yarn protobuf-compiler && \
    rm -rf /var/lib/apt/lists
ADD https://github.com/grpc/grpc-web/releases/download/1.0.6/protoc-gen-grpc-web-1.0.6-linux-x86_64 /usr/bin/protoc-gen-grpc-web
RUN chmod u+rwx,go+rx-w /usr/bin/protoc-gen-grpc-web
WORKDIR /build

COPY *.sln .
COPY Host/*.csproj Host/
COPY Interface/*.csproj Interface/package.json Interface/yarn.lock Interface/
COPY Service/*.csproj Service/
COPY Tests/*.csproj Tests/
RUN dotnet restore

COPY . .
ARG CONFIGURATION=Release
RUN dotnet publish Host \
    --self-contained false \
    --runtime linux-x64 \
    --configuration $CONFIGURATION \
    --output /output

FROM mcr.microsoft.com/dotnet/core/aspnet:3.0
WORKDIR /app
COPY --from=builder /output ./

ENTRYPOINT ["dotnet", "Host"]

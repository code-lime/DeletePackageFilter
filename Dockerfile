# Set the base image as the .NET 8.0 SDK (this includes the runtime)
FROM mcr.microsoft.com/dotnet/sdk:8.0 as build-env

# Copy everything and publish the release (publish implicitly restores and builds)
COPY . ./
RUN dotnet publish ./src/DotNet.GitHubAction/DotNet.GitHubAction.csproj -c Release -o out --no-self-contained

# Label as GitHub action
LABEL com.github.actions.name="Delete package versions"
LABEL com.github.actions.description="A Github action that delete selected package versions"
LABEL com.github.actions.icon="trash"
LABEL com.github.actions.color="green"

# Relayer the .NET SDK, anew with the build output
FROM mcr.microsoft.com/dotnet/sdk:8.0
COPY --from=build-env /out .
ENTRYPOINT [ "dotnet", "/DotNet.GitHubAction.dll" ]

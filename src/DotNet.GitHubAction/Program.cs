using CommandLine;
using DotNet.GitHubAction.Extensions;
using DotNet.GitHubAction;
using Octokit;
using Octokit.Internal;
using Serilog;

Log.Logger = new LoggerConfiguration()
        .WriteTo.Console(outputTemplate: "{Message:lj}{NewLine}{Exception}")
        .CreateLogger();

try
{
    await new Parser(v =>
    {
        v.CaseInsensitiveEnumValues = true;
    })
    .ParseArguments<ActionInputs>(() => new(), args.ModifyEnviromentArgs())
    .WithNotParsed(errors =>
    {
        Log.Logger.Error(string.Join(Environment.NewLine, errors.Select(error => error.ToString())));
        Environment.Exit(2);
    })
    .WithParsedAsync(async options =>
    {
        string[] repositoryParts = options.GitHubRepository.Split('/');

        string _owner = repositoryParts[0];
        string _repository = repositoryParts[1];
        PackageType _type = options.Type;
        string _token = options.Token;
        string _version = options.Version;

        GitHubClient github = new GitHubClient(
            new ProductHeaderValue("DeletePackageFilter"),
            new InMemoryCredentialStore(new Credentials(_token, AuthenticationType.Bearer)));

        var repo = await github.Repository.Get(_owner, _repository);

        var packageVersionsClient = github.Packages.PackageVersions;

        GetAllPackageVersions getVersions;
        DeletePackageVersion deleteVersion;
        if (repo.Owner.Type == AccountType.Organization)
        {
            getVersions = packageVersionsClient.GetAllForOrg;
            deleteVersion = packageVersionsClient.DeleteForOrg;
        }
        else
        {
            getVersions = packageVersionsClient.GetAllForUser;
            deleteVersion = packageVersionsClient.DeleteForUser;
        }

        IReadOnlyList<PackageVersion> versions = await getVersions(_owner, _type, _repository);

        foreach (var version in versions.Where(v => _version == v.Name))
        {
            if (version.Id > int.MaxValue)
                throw new IndexOutOfRangeException($"Version {version.Name} with id {version.Id} is limitted! BUG of Octokit");
            Log.Logger.Information($"Delete version {version.Name} with id {version.Id}");
            await deleteVersion(_owner, _type, _repository, (int)version.Id);
        }
    });
}
catch (Exception e)
{
    Log.Logger.Error(e, "Error");
    Environment.Exit(2);
}

delegate Task<IReadOnlyList<PackageVersion>> GetAllPackageVersions(string owner, PackageType packageType, string packageName, PackageVersionState state = PackageVersionState.Active, ApiOptions? options = null);
delegate Task DeletePackageVersion(string owner, PackageType packageType, string packageName, int packageVersionId);
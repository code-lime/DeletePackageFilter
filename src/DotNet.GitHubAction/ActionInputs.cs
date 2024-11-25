using CommandLine;
using Octokit;

namespace DotNet.GitHubAction;

public class ActionInputs
{
    [Option("github-repository",
        Required = true,
        HelpText = "Repository name in format <OWNER>/<REPO>")]
    public string GitHubRepository { get; set; } = null!;

    [Option("type",
        Required = true,
        HelpText = "Package type")]
    public PackageType Type { get; set; }

    [Option("token",
        Required = true,
        HelpText = "GitHub token")]
    public string Token { get; set; } = null!;

    [Option("version",
        Required = true,
        HelpText = "Version name")]
    public string Version { get; set; } = null!;
}

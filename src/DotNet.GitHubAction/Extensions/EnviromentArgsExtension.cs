using System.Collections;

namespace DotNet.GitHubAction.Extensions;

public static class EnviromentArgsExtension
{
    private static string ModifyEnviromentArg(this string arg, Dictionary<string, string> enviroments)
    {
        if (!arg.StartsWith('$'))
            return arg;

        string variableData = arg[1..];
        if (!variableData.StartsWith('{') || !variableData.EndsWith('}'))
            return enviroments.GetValueOrDefault(variableData, arg);

        variableData = variableData[1..^1];
        string[] parts = variableData.Split(':');

        string name;
        int position = 0;
        int? length = null;

        switch (parts.Length)
        {
            case 1:
                {
                    name = parts[0];
                    break;
                }
            case 2:
                {
                    name = parts[0];
                    position = int.Parse(parts[1]);
                    break;
                }
            case 3:
                {
                    name = parts[0];
                    position = int.Parse(parts[1]);
                    length = int.Parse(parts[2]);
                    break;
                }
            default:
                return arg;
        }

        if (!enviroments.TryGetValue(name, out string? value))
            return arg;

        if (value.Length <= position)
            return "";

        value = value[position..];

        if (length.HasValue && value.Length > length.Value)
            value = value[..length.Value];

        return value;
    }
    public static IEnumerable<string> ModifyEnviromentArgs(this IEnumerable<string> args)
    {
        Dictionary<string, string> enviroments = [];
        foreach (DictionaryEntry entry in Environment.GetEnvironmentVariables())
            enviroments[entry.Key.ToString() ?? ""] = entry.Value?.ToString() ?? "";

        foreach (string arg in args)
            yield return arg.ModifyEnviromentArg(enviroments);
    }
}

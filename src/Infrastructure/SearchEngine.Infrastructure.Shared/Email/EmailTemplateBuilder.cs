namespace SearchEngine.Infrastructure.Shared.Email;

public static class EmailTemplateBuilder
{
    public static string BuildTemplate(
        string templateName,
        Dictionary<string, string> values)
    {
        var path =
            Path.Combine(
                AppContext.BaseDirectory,
                "Email",
                "Templates",
                $"{templateName}.html");

        var html =
            File.ReadAllText(path);

        foreach (var item in values)
        {
            html = html.Replace(
                $"{{{{{item.Key}}}}}",
                item.Value);
        }

        return html;
    }
}

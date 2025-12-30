using System.Text;
using Microsoft.AspNetCore.Hosting;

namespace Promptino.API.Tools;
public sealed class HtmlToString
{
    private readonly IWebHostEnvironment _env;

    public HtmlToString(IWebHostEnvironment env)
    {
        _env = env;
    }

    public async Task<string> LoadAsync(
        string fileName,
        IDictionary<string, string> values)
    {
        var path = Path.Combine(
            _env.ContentRootPath,
            "EmailTemplates",
            fileName
        );

        var html = await File.ReadAllTextAsync(path, Encoding.UTF8);

        foreach (var item in values)
        {
            html = html.Replace($"{{{{{item.Key}}}}}", item.Value);
        }

        return html;
    }

}

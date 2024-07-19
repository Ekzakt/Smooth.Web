using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Reflection;

namespace Smooth.Web.TagHelpers;

[HtmlTargetElement("version")]
public class VersionTagHelper : TagHelper
{
    public string? Prefix { get; set; } = null;

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        var version = Assembly.GetExecutingAssembly().GetName().Version?.ToString();

        if (!string.IsNullOrEmpty(version))
        {
            version = new Version(version).ToString(3);
        }

        if (!string.IsNullOrEmpty(Prefix))
        {
            version = $"{Prefix}{version}".Trim();
        }

        output.TagName = "span";

        output.Content.SetContent($"{version}");
    }
}

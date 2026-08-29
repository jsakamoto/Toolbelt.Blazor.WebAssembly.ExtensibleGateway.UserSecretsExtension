using Microsoft.AspNetCore.Http;
using Toolbelt.Blazor.WebAssembly.ExtensibleGateway.UserSecretsExtension;

namespace UserSecretsExtension.Test;

public class UserSecretsExtensionStartupFilterTests
{
    [TestCase("/appsettings.json", true)]
    [TestCase("/appsettings.Development.json", true)]
    [TestCase("/APPSETTINGS.JSON", true)]
    // The gateway can host an app under a path prefix, and this middleware sees the request before
    // that prefix is stripped off.
    [TestCase("/my-app/appsettings.json", true)]
    [TestCase("/my-app/appsettings.Development.json", true)]
    [TestCase("/", false)]
    [TestCase("/index.html", false)]
    [TestCase("/_framework/blazor.webassembly.js", false)]
    [TestCase("/appsettings.json/something", false)]
    [TestCase("/my-appsettings.json", false)]
    [TestCase("/appsettings.json.txt", false)]
    public void IsAppSettingsJsonPath_ReturnsExpected(string path, bool expected)
    {
        UserSecretsExtensionStartupFilter.IsAppSettingsJsonPath(new PathString(path)).Is(expected);
    }

    [Test]
    public void IsAppSettingsJsonPath_EmptyPath_ReturnsFalse()
    {
        UserSecretsExtensionStartupFilter.IsAppSettingsJsonPath(PathString.Empty).IsFalse();
    }
}

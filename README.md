# User Secrets Extension for Blazor WebAssembly Extensible Gateway

[![tests](https://github.com/jsakamoto/Toolbelt.Blazor.WebAssembly.ExtensibleGateway.UserSecretsExtension/actions/workflows/tests.yml/badge.svg)](https://github.com/jsakamoto/Toolbelt.Blazor.WebAssembly.ExtensibleGateway.UserSecretsExtension/actions/workflows/tests.yml) [![NuGet Package](https://img.shields.io/nuget/v/Toolbelt.Blazor.WebAssembly.ExtensibleGateway.UserSecretsExtension.svg)](https://www.nuget.org/packages/Toolbelt.Blazor.WebAssembly.ExtensibleGateway.UserSecretsExtension/) [![Discord](https://img.shields.io/discord/798312431893348414?style=flat&logo=discord&logoColor=white&label=Blazor%20Community&labelColor=5865f2&color=gray)](https://discord.com/channels/798312431893348414/1202165955900473375)

An extension that lets you use User Secrets in a Blazor WebAssembly Standalone project hosted with the [Toolbelt.Blazor.WebAssembly.ExtensibleGateway](https://github.com/jsakamoto/Toolbelt.Blazor.WebAssembly.ExtensibleGateway).

With it, you can keep your own configuration values out of `appsettings.json` and `appsettings.Development.json`, so you never commit them by mistake.

This package is the .NET 11 version of [Toolbelt.Blazor.WebAssembly.ExtensibleDevServer.UserSecretsExtension](https://github.com/jsakamoto/Toolbelt.Blazor.WebAssembly.ExtensibleDevServer.UserSecretsExtension). Starting from .NET 11, a Blazor WebAssembly Standalone app is served by the new gateway (`Microsoft.AspNetCore.Components.Gateway`) instead of the old dev server, so a new extension was needed.

## The problem

In a Blazor WebAssembly standalone project, the app configuration is served from JSON files under the `wwwroot` folder, such as `appsettings.json` and `appsettings.Development.json`. These files are usually committed to source control, so that every team member gets a working configuration.

But each developer often wants to change some values only on their own machine. For example.

- **REST API base URL**, to point at a local API server instead of the shared one.
- **APM (Application Performance Monitoring) API key**, to use a personal key.
- **Logging level**, to turn on verbose logging for one category while debugging.

Without this extension, the only way is to edit `appsettings.Development.json` directly. That file is then easy to commit by mistake, and the change hits the whole team.

## The solution

This extension adds .NET **User Secrets** as a third configuration layer, on top of `appsettings.json` and `appsettings.Development.json`.

Once installed, it catches HTTP GET requests for `appsettings.*.json` and merges your User Secrets into the response. The JSON files on disk are never touched. Only the HTTP response changes.

So each developer can keep their own values in their own User Secrets store, apart from the shared configuration files.

> [!CAUTION]
> **User Secrets are NOT secret in this context.**
>
> The values you store here are **not protected** at all. They are sent as plain text in the response to an anonymous HTTP GET request for `appsettings.json`, just like any other configuration value. In this extension, User Secrets are only a third configuration store, nothing more. Please do **not** put real secrets, such as passwords or tokens, in it.

## How to use

### Prerequisites

Your Blazor WebAssembly standalone project has to use [Toolbelt.Blazor.WebAssembly.ExtensibleGateway](https://github.com/jsakamoto/Toolbelt.Blazor.WebAssembly.ExtensibleGateway) in place of the stock `Microsoft.AspNetCore.Components.Gateway`. If you have not done that yet, please see the [ExtensibleGateway README](https://github.com/jsakamoto/Toolbelt.Blazor.WebAssembly.ExtensibleGateway#how-to-use).

### 1. Install this package

```shell
dotnet add package Toolbelt.Blazor.WebAssembly.ExtensibleGateway.UserSecretsExtension
```

That is all. No extra code and no extra configuration are needed.

### 2. Set your User Secrets

If you use **Visual Studio**, right click your project in Solution Explorer and pick **"Manage User Secrets"** to open the `secrets.json` file.

You can also use the .NET CLI.

```shell
dotnet user-secrets init   # only once per project
dotnet user-secrets set "SomeSection:SomeKey" "my-custom-value"
```

### 3. Run your project

When you run your Blazor WebAssembly project, an HTTP GET request for `appsettings.json` or `appsettings.Development.json` returns the file content merged with your User Secrets. Your own values take effect, and the files on disk stay as they are.

The secrets file is read on every request, so you do not have to restart the app after you change a value.

## How it works

```
Browser requests GET /appsettings.Development.json
        │
        ▼
┌──────────────────────────────────┐
│  Extensible Gateway              │
│                                  │
│  ┌────────────────────────────┐  │
│  │ User Secrets Extension     │  │
│  │                            │  │
│  │ 1. Turn off the compressed │  │
│  │    variant of the file     │  │
│  │ 2. Let the gateway serve   │  │
│  │    the file from wwwroot/  │  │
│  │ 3. Merge the User Secrets  │  │
│  │    into that JSON          │  │
│  │ 4. Return the merged JSON  │  │
│  └────────────────────────────┘  │
└──────────────────────────────────┘
        │
        ▼
Browser receives merged configuration
```

The extension is loaded by the gateway as a hosting startup, and it adds one middleware in front of the gateway pipeline. The middleware runs before routing, so it also turns off the compressed variant of the file. That way it always has the plain JSON text to merge into.

The MSBuild targets of this package pass the `UserSecretsId` of your project to the gateway through the `DOTNET_USER_SECRETS_ID` environment variable. If your project has no `UserSecretsId`, the extension does nothing.

## License

This project is licensed under the Mozilla Public License v2.0. See the [LICENSE](https://github.com/jsakamoto/Toolbelt.Blazor.WebAssembly.ExtensibleGateway.UserSecretsExtension/blob/main/LICENSE) file for details.

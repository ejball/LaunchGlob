using System;
using Faithlife.Build;
using static Faithlife.Build.DotNetRunner;

internal static class Build
{
	public static int Main(string[] args) => BuildRunner.Execute(args, build =>
	{
		var buildOptions = new DotNetBuildOptions();

		build.AddDotNetTargets(
			new DotNetBuildSettings
			{
				NuGetApiKey = Environment.GetEnvironmentVariable("NUGET_API_KEY"),
				DocsSettings = new DotNetDocsSettings
				{
					GitLogin = new GitLoginInfo("ejball", Environment.GetEnvironmentVariable("BUILD_BOT_PASSWORD") ?? ""),
					GitAuthor = new GitAuthorInfo("ejball", "ejball@gmail.com"),
					SourceCodeUrl = "https://github.com/ejball/LaunchGlob/tree/master/src",
				},
				BuildOptions = buildOptions,
				Verbosity = DotNetBuildVerbosity.Minimal,
			});

		build.Target("package")
			.Describe("Create a standalone executable")
			.ClearActions()
			.Does(() =>
			{
				RunDotNet("publish",
					"-c", buildOptions.ConfigurationOption!.Value,
					"-r", "win-x86",
					"--self-contained", "true",
					"-p:PublishSingleFile=true",
					"-p:PublishTrimmed=true");
			});
	});
}

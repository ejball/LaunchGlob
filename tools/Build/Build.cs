return BuildRunner.Execute(args, build =>
{
	var buildOptions = new DotNetBuildOptions();

	build.AddDotNetTargets(
		new DotNetBuildSettings
		{
			BuildOptions = buildOptions,
		});

	build.Target("package")
		.Describe("Creates a standalone executable")
		.ClearActions()
		.Does(() =>
		{
			RunDotNet("publish",
				Path.Combine("src", "LaunchGlob", "LaunchGlob.csproj"),
				"-c", buildOptions.ConfigurationOption!.Value,
				"-r", "win-x64",
				"--self-contained", "true",
				"-p:PublishSingleFile=true");
		});
});

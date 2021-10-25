using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using ArgsReading;
using GlobExpressions;

namespace LaunchGlob
{
	internal static class Program
	{
		[STAThread]
		public static void Main(string[] args)
		{
			Application.SetHighDpiMode(HighDpiMode.SystemAware);
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

			try
			{
				Run(new ArgsReader(args) { NoOptionsAfterDoubleDash = true });
			}
			catch (Exception exception)
			{
				ShowMessageBox($"Error: {exception.Message}");
			}
		}

		private static void Run(ArgsReader args)
		{
			var workingDirectory = args.ReadOption("w") ?? Environment.CurrentDirectory;
			var arguments = args.ReadArguments().ToList();

			var globIndex = arguments.FindIndex(x => x.IndexOfAny(new[] { '*', '?', '[', '{' }) != -1);

			void LaunchFile(string filePath)
			{
				var processArgs = arguments.Take(globIndex).Append(filePath).Concat(arguments.Skip(globIndex + 1)).ToList();

				var processStartInfo = new ProcessStartInfo
				{
					FileName = processArgs[0],
					WorkingDirectory = workingDirectory,
					UseShellExecute = true,
				};
				foreach (var processArg in processArgs.Skip(1))
					processStartInfo.ArgumentList.Add(processArg);
				Process.Start(processStartInfo);
			}

			if (arguments.Count == 0)
			{
				ShowMessageBox(
					"Usage: LaunchGlob <args...> [options] [-- <args...>]",
					"",
					"One argument must be a glob (e.g. *.sln). If there is one match,",
					"it will be launched immediately; otherwise, choose from the prompt.",
					"",
					"Options:",
					"  -w <working-directory>");
			}
			else if (globIndex == -1)
			{
				ShowMessageBox("Error: glob not found in arguments.");
			}
			else
			{
				var glob = arguments[globIndex];
				var filePaths = Glob.Files(workingDirectory, glob, GlobOptions.CaseInsensitive).ToList();
				if (filePaths.Count == 0)
				{
					ShowMessageBox(
						"File not found!",
						"",
						$"Glob: {glob}",
						$"Directory: {workingDirectory}");
				}
				else if (filePaths.Count == 1)
				{
					LaunchFile(filePaths[0]);
				}
				else
				{
					var listBox = new ListBox
					{
						Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
						ItemHeight = 15,
						Location = new Point(12, 12),
						Size = new Size(460, 94),
						TabIndex = 0,
					};

					var launchButton = new Button
					{
						Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
						Location = new Point(397, 116),
						Size = new Size(75, 23),
						TabIndex = 1,
						Text = "Launch",
					};

					var form = new Form
					{
						AcceptButton = launchButton,
						AutoScaleDimensions = new SizeF(7F, 15F),
						AutoScaleMode = AutoScaleMode.Font,
						ClientSize = new Size(484, 151),
						Controls = { listBox, launchButton },
						KeyPreview = true,
						MinimumSize = new Size(300, 150),
						ShowIcon = false,
						StartPosition = FormStartPosition.CenterScreen,
						Text = c_appCaption,
					};

					foreach (var filePath in filePaths)
						listBox.Items.Add(filePath);
					listBox.SelectedIndex = 0;

					listBox.MouseDoubleClick += (s, e) => LaunchSelectedFile();
					launchButton.Click += (s, e) => LaunchSelectedFile();
					form.KeyDown += (s, e) =>
					{
						if (e.KeyCode == Keys.Escape)
							form.Close();
					};

					Application.Run(form);

					void LaunchSelectedFile()
					{
#pragma warning disable 8602 // https://github.com/dotnet/roslyn/issues/40821
						LaunchFile(filePaths[listBox.SelectedIndex]);
						form.Close();
#pragma warning restore 8602
					}
				}
			}
		}

		private static void ShowMessageBox(params string[] lines)
		{
			MessageBox.Show(
				text: string.Join(Environment.NewLine, lines),
				caption: c_appCaption);
		}

		private const string c_appCaption = "LaunchGlob";
	}
}

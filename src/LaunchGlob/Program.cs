using System.Diagnostics;
using ArgsReading;
using GlobExpressions;

namespace LaunchGlob
{
	internal static class Program
	{
		[STAThread]
		public static void Main(string[] args)
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.SetHighDpiMode(HighDpiMode.SystemAware);

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

					var form = new Form();
					form.SuspendLayout();

					form.AcceptButton = launchButton;
					form.AutoScaleDimensions = new SizeF(7F, 15F);
					form.AutoScaleMode = AutoScaleMode.Font;
					form.ClientSize = new Size(484, 151);
					form.Controls.Add(listBox);
					form.Controls.Add(launchButton);
					form.KeyPreview = true;
					form.MinimumSize = new Size(300, 150);
					form.ShowIcon = false;
					form.StartPosition = FormStartPosition.CenterScreen;
					form.Text = c_appCaption;

					foreach (var filePath in filePaths)
						listBox.Items.Add(filePath);
					listBox.SelectedIndex = 0;

					listBox.MouseDoubleClick += (_, _) => LaunchSelectedFile();
					launchButton.Click += (_, _) => LaunchSelectedFile();
					form.KeyDown += (_, e) =>
					{
						if (e.KeyCode == Keys.Escape)
							form.Close();
					};

					form.ResumeLayout(performLayout: false);
					Application.Run(form);

					void LaunchSelectedFile()
					{
						LaunchFile(filePaths[listBox.SelectedIndex]);
						form.Close();
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

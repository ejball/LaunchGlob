# LaunchGlob

Launches the specified app or file, allowing [one glob argument](https://github.com/kthompson/glob#supported-pattern-expressions).

## Usage

* `LaunchGlob *.txt` – Opens the text file in the current directory with the default application. If there are multiple text files, it displays a prompt.
* `LaunchGlob *.txt -w C:\Temp` – Opens the text file from the specified directory.
* `LaunchGlob notepad *.txt` – Opens the text file using Notepad.
* `LaunchGlob **/*.txt` – Opens a text file anywhere in the directory tree.
* `LaunchGlob code *.txt -- --new-window` – Opens the text file using Visual Studio Code, forcing a new window.

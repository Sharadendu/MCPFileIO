using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using ModelContextProtocol.Server;

/// <summary>
/// MCP tools for compiler, debug, execution, and log monitoring capabilities.
/// These tools enable compilation, execution, debugging, and log analysis for the MCP server.
/// </summary>
internal class CompilerDebugExecutionTools
{
    [McpServerTool]
    [Description("Compiles the C# project using dotnet build command.")]
    public string CompileProject(
        [Description("The project directory path")] string projectPath,
        [Description("The build configuration (Debug or Release)")] string configuration = "Debug",
        [Description("Whether to restore NuGet packages first")] bool restore = true)
    {
        try
        {
            if (!Directory.Exists(projectPath))
            {
                return $"Error: Project directory not found at '{projectPath}'";
            }

            var commands = new List<string>();

            if (restore)
            {
                commands.Add("dotnet restore");
            }

            commands.Add($"dotnet build --configuration {configuration} --no-incremental");

            var output = new StringBuilder();

            foreach (var command in commands)
            {
                var result = RunCommand(projectPath, command);
                output.AppendLine($"Command: {command}");
                output.AppendLine(result);
                output.AppendLine();
            }

            return output.ToString();
        }
        catch (Exception ex)
        {
            return $"Error compiling project: {ex.Message}";
        }
    }

    [McpServerTool]
    [Description("Checks for compilation errors in the project without building.")]
    public string CheckCompileErrors(
        [Description("The project directory path")] string projectPath)
    {
        try
        {
            if (!Directory.Exists(projectPath))
            {
                return $"Error: Project directory not found at '{projectPath}'";
            }

            var result = RunCommand(projectPath, "dotnet build --no-build --configuration Debug");
            return result;
        }
        catch (Exception ex)
        {
            return $"Error checking compilation: {ex.Message}";
        }
    }

    [McpServerTool]
    [Description("Executes a dotnet application and captures its output.")]
    public string ExecuteApplication(
        [Description("The project directory path")] string projectPath,
        [Description("Arguments to pass to the application")] string? arguments = null,
        [Description("Timeout in seconds")] int timeoutSeconds = 30)
    {
        try
        {
            if (!Directory.Exists(projectPath))
            {
                return $"Error: Project directory not found at '{projectPath}'";
            }

            var command = "dotnet run";
            if (!string.IsNullOrEmpty(arguments))
            {
                command += $" -- {arguments}";
            }

            var result = RunCommand(projectPath, command, timeoutSeconds);
            return result;
        }
        catch (Exception ex)
        {
            return $"Error executing application: {ex.Message}";
        }
    }

    [McpServerTool]
    [Description("Runs an executable file directly and captures output.")]
    public string RunExecutable(
        [Description("The absolute path to the executable file")] string executablePath,
        [Description("Arguments to pass to the executable")] string? arguments = null,
        [Description("Working directory for execution")] string? workingDirectory = null,
        [Description("Timeout in seconds")] int timeoutSeconds = 30)
    {
        try
        {
            if (!File.Exists(executablePath))
            {
                return $"Error: Executable not found at '{executablePath}'";
            }

            var psi = new ProcessStartInfo
            {
                FileName = executablePath,
                Arguments = arguments ?? string.Empty,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                WorkingDirectory = workingDirectory ?? Path.GetDirectoryName(executablePath) ?? Directory.GetCurrentDirectory()
            };

            using (var process = Process.Start(psi))
            {
                if (process == null)
                {
                    return "Error: Failed to start process";
                }

                var output = new StringBuilder();
                var error = new StringBuilder();

                if (!process.WaitForExit(timeoutSeconds * 1000))
                {
                    process.Kill();
                    return $"Error: Process execution timed out after {timeoutSeconds} seconds";
                }

                output.Append(process.StandardOutput.ReadToEnd());
                error.Append(process.StandardError.ReadToEnd());

                var result = new StringBuilder();
                result.AppendLine($"Exit Code: {process.ExitCode}");
                result.AppendLine();

                if (!string.IsNullOrEmpty(output.ToString()))
                {
                    result.AppendLine("Output:");
                    result.AppendLine(output.ToString());
                }

                if (!string.IsNullOrEmpty(error.ToString()))
                {
                    result.AppendLine("Errors:");
                    result.AppendLine(error.ToString());
                }

                return result.ToString();
            }
        }
        catch (Exception ex)
        {
            return $"Error running executable: {ex.Message}";
        }
    }

    [McpServerTool]
    [Description("Executes a C# code snippet and returns the result.")]
    public string ExecuteCodeSnippet(
        [Description("The C# code to execute")] string code,
        [Description("Namespaces to include")] string? additionalNamespaces = null)
    {
        try
        {
            var script = BuildCSharpScript(code, additionalNamespaces);
            var tempFile = Path.Combine(Path.GetTempPath(), $"snippet_{Guid.NewGuid()}.cs");

            try
            {
                File.WriteAllText(tempFile, script);
                var compileResult = RunCommand(Path.GetTempPath(), $"csc {tempFile} -out:{tempFile}.exe");

                if (compileResult.Contains("error"))
                {
                    return $"Compilation failed:\n{compileResult}";
                }

                var exePath = $"{tempFile}.exe";
                if (File.Exists(exePath))
                {
                    var execResult = RunCommand(Path.GetTempPath(), exePath, 10);
                    return execResult;
                }

                return "Error: Could not locate compiled executable";
            }
            finally
            {
                if (File.Exists(tempFile))
                    File.Delete(tempFile);
                if (File.Exists($"{tempFile}.exe"))
                    File.Delete($"{tempFile}.exe");
            }
        }
        catch (Exception ex)
        {
            return $"Error executing code snippet: {ex.Message}";
        }
    }

    [McpServerTool]
    [Description("Reads a log file and returns its contents.")]
    public string ReadLogFile(
        [Description("The absolute path to the log file")] string logFilePath)
    {
        try
        {
            if (!File.Exists(logFilePath))
            {
                return $"Error: Log file not found at '{logFilePath}'";
            }

            return File.ReadAllText(logFilePath);
        }
        catch (Exception ex)
        {
            return $"Error reading log file: {ex.Message}";
        }
    }

    [McpServerTool]
    [Description("Tails a log file (returns last N lines).")]
    public string TailLogFile(
        [Description("The absolute path to the log file")] string logFilePath,
        [Description("Number of lines to return from the end")] int lineCount = 50)
    {
        try
        {
            if (!File.Exists(logFilePath))
            {
                return $"Error: Log file not found at '{logFilePath}'";
            }

            var lines = File.ReadAllLines(logFilePath);
            var start = Math.Max(0, lines.Length - lineCount);
            var tailLines = lines.Skip(start).ToArray();

            return string.Join(Environment.NewLine, tailLines);
        }
        catch (Exception ex)
        {
            return $"Error tailing log file: {ex.Message}";
        }
    }

    [McpServerTool]
    [Description("Searches a log file for lines matching a pattern.")]
    public string SearchLogFile(
        [Description("The absolute path to the log file")] string logFilePath,
        [Description("The search pattern or text")] string searchPattern,
        [Description("Whether to use regex matching")] bool useRegex = false,
        [Description("Case-sensitive search")] bool caseSensitive = false)
    {
        try
        {
            if (!File.Exists(logFilePath))
            {
                return $"Error: Log file not found at '{logFilePath}'";
            }

            var lines = File.ReadAllLines(logFilePath);
            var result = new StringBuilder();
            var comparison = caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
            var matchCount = 0;

            for (int i = 0; i < lines.Length; i++)
            {
                bool matches = false;

                if (useRegex)
                {
                    try
                    {
                        var regexOptions = caseSensitive
                            ? System.Text.RegularExpressions.RegexOptions.None
                            : System.Text.RegularExpressions.RegexOptions.IgnoreCase;
                        matches = System.Text.RegularExpressions.Regex.IsMatch(lines[i], searchPattern, regexOptions);
                    }
                    catch
                    {
                        return $"Error: Invalid regex pattern '{searchPattern}'";
                    }
                }
                else
                {
                    matches = lines[i].Contains(searchPattern, comparison);
                }

                if (matches)
                {
                    result.AppendLine($"[{i + 1}]: {lines[i]}");
                    matchCount++;
                }
            }

            if (matchCount == 0)
            {
                return $"No matches found for pattern '{searchPattern}'";
            }

            return $"Found {matchCount} matching line(s):\n{result}";
        }
        catch (Exception ex)
        {
            return $"Error searching log file: {ex.Message}";
        }
    }

    [McpServerTool]
    [Description("Monitors a log file and returns new entries since last position.")]
    public string MonitorLogFile(
        [Description("The absolute path to the log file")] string logFilePath,
        [Description("Number of lines to check from the end")] int recentLines = 100)
    {
        try
        {
            if (!File.Exists(logFilePath))
            {
                return $"Error: Log file not found at '{logFilePath}'";
            }

            var fileInfo = new FileInfo(logFilePath);
            var lines = File.ReadAllLines(logFilePath);
            var start = Math.Max(0, lines.Length - recentLines);

            var result = new StringBuilder();
            result.AppendLine($"File: {logFilePath}");
            result.AppendLine($"Size: {fileInfo.Length} bytes");
            result.AppendLine($"Last Modified: {fileInfo.LastWriteTime:yyyy-MM-dd HH:mm:ss}");
            result.AppendLine($"Total Lines: {lines.Length}");
            result.AppendLine($"Recent Lines ({recentLines}):");
            result.AppendLine(new string('-', 80));

            foreach (var line in lines.Skip(start))
            {
                result.AppendLine(line);
            }

            return result.ToString();
        }
        catch (Exception ex)
        {
            return $"Error monitoring log file: {ex.Message}";
        }
    }

    [McpServerTool]
    [Description("Filters log file by log level (Error, Warning, Info, Debug).")]
    public string FilterLogByLevel(
        [Description("The absolute path to the log file")] string logFilePath,
        [Description("Log level to filter (Error, Warning, Info, Debug)")] string logLevel)
    {
        try
        {
            if (!File.Exists(logFilePath))
            {
                return $"Error: Log file not found at '{logFilePath}'";
            }

            var lines = File.ReadAllLines(logFilePath);
            var filtered = lines.Where(l => l.Contains(logLevel, StringComparison.OrdinalIgnoreCase)).ToArray();

            if (filtered.Length == 0)
            {
                return $"No entries found with log level '{logLevel}'";
            }

            var result = new StringBuilder();
            result.AppendLine($"Found {filtered.Length} entries with level '{logLevel}':");
            result.AppendLine();

            foreach (var line in filtered)
            {
                result.AppendLine(line);
            }

            return result.ToString();
        }
        catch (Exception ex)
        {
            return $"Error filtering log file: {ex.Message}";
        }
    }

    [McpServerTool]
    [Description("Gets system environment variables.")]
    public string GetEnvironmentVariables()
    {
        try
        {
            var result = new StringBuilder();
            var vars = Environment.GetEnvironmentVariables();

            foreach (var key in vars.Keys)
            {
                result.AppendLine($"{key}={vars[key]}");
            }

            return result.ToString();
        }
        catch (Exception ex)
        {
            return $"Error getting environment variables: {ex.Message}";
        }
    }

    [McpServerTool]
    [Description("Gets a specific environment variable value.")]
    public string GetEnvironmentVariable(
        [Description("The name of the environment variable")] string variableName)
    {
        try
        {
            var value = Environment.GetEnvironmentVariable(variableName);
            return value ?? $"Environment variable '{variableName}' not found";
        }
        catch (Exception ex)
        {
            return $"Error getting environment variable: {ex.Message}";
        }
    }

    [McpServerTool]
    [Description("Lists all running processes.")]
    public string ListRunningProcesses()
    {
        try
        {
            var processes = Process.GetProcesses();
            var result = new StringBuilder();

            result.AppendLine($"Total Processes: {processes.Length}");
            result.AppendLine(new string('-', 100));
            result.AppendLine(string.Format("{0,-8} {1,-40} {2,-15} {3}", "PID", "Process Name", "Memory (MB)", "Start Time"));
            result.AppendLine(new string('-', 100));

            foreach (var process in processes.OrderByDescending(p => p.WorkingSet64).Take(50))
            {
                try
                {
                    var memoryMB = process.WorkingSet64 / (1024 * 1024);
                    var startTime = process.StartTime.ToString("yyyy-MM-dd HH:mm:ss");
                    result.AppendLine(string.Format("{0,-8} {1,-40} {2,-15} {3}", 
                        process.Id, 
                        process.ProcessName, 
                        memoryMB, 
                        startTime));
                }
                catch
                {
                    // Skip processes we can't access
                }
            }

            return result.ToString();
        }
        catch (Exception ex)
        {
            return $"Error listing processes: {ex.Message}";
        }
    }

    [McpServerTool]
    [Description("Gets detailed information about a specific process.")]
    public string GetProcessInfo(
        [Description("The process ID or process name")] string processIdentifier)
    {
        try
        {
            Process? process = null;

            if (int.TryParse(processIdentifier, out var pid))
            {
                process = Process.GetProcessById(pid);
            }
            else
            {
                var processes = Process.GetProcessesByName(processIdentifier);
                if (processes.Length == 0)
                {
                    return $"Error: Process '{processIdentifier}' not found";
                }
                process = processes[0];
            }

            if (process == null)
            {
                return $"Error: Could not get process information";
            }

            var result = new StringBuilder();
            result.AppendLine($"Process Name: {process.ProcessName}");
            result.AppendLine($"Process ID: {process.Id}");
            result.AppendLine($"Memory Usage: {process.WorkingSet64 / (1024 * 1024)} MB");
            result.AppendLine($"CPU Time: {process.TotalProcessorTime}");
            result.AppendLine($"Start Time: {process.StartTime:yyyy-MM-dd HH:mm:ss}");
            result.AppendLine($"Status: {(process.HasExited ? "Exited" : "Running")}");

            try
            {
                result.AppendLine($"Main Window Title: {process.MainWindowTitle}");
                result.AppendLine($"Priority: {process.PriorityClass}");
            }
            catch
            {
                // Some properties might not be accessible
            }

            return result.ToString();
        }
        catch (Exception ex)
        {
            return $"Error getting process info: {ex.Message}";
        }
    }

    [McpServerTool]
    [Description("Kills a process by ID or name.")]
    public string KillProcess(
        [Description("The process ID or process name")] string processIdentifier,
        [Description("Wait for graceful shutdown (seconds)")] int gracefulWaitSeconds = 5)
    {
        try
        {
            Process? process = null;

            if (int.TryParse(processIdentifier, out var pid))
            {
                process = Process.GetProcessById(pid);
            }
            else
            {
                var processes = Process.GetProcessesByName(processIdentifier);
                if (processes.Length == 0)
                {
                    return $"Error: Process '{processIdentifier}' not found";
                }
                process = processes[0];
            }

            if (process == null || process.HasExited)
            {
                return $"Error: Process is not running";
            }

            process.CloseMainWindow();

            if (!process.WaitForExit(gracefulWaitSeconds * 1000))
            {
                process.Kill();
            }

            return $"Successfully terminated process '{process.ProcessName}' (PID: {process.Id})";
        }
        catch (Exception ex)
        {
            return $"Error killing process: {ex.Message}";
        }
    }

    // Helper method to run commands
    private string RunCommand(string workingDirectory, string command, int timeoutSeconds = 30)
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/c {command}",
                WorkingDirectory = workingDirectory,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using (var process = Process.Start(psi))
            {
                if (process == null)
                {
                    return "Error: Failed to start process";
                }

                if (!process.WaitForExit(timeoutSeconds * 1000))
                {
                    process.Kill();
                    return $"Command timed out after {timeoutSeconds} seconds";
                }

                var output = process.StandardOutput.ReadToEnd();
                var error = process.StandardError.ReadToEnd();

                var result = new StringBuilder();
                if (!string.IsNullOrEmpty(output))
                    result.AppendLine(output);
                if (!string.IsNullOrEmpty(error))
                    result.AppendLine(error);

                return result.Length > 0 ? result.ToString() : $"Command completed with exit code {process.ExitCode}";
            }
        }
        catch (Exception ex)
        {
            return $"Error running command: {ex.Message}";
        }
    }

    // Helper method to build C# script
    private string BuildCSharpScript(string code, string? additionalNamespaces)
    {
        var namespaces = new StringBuilder();
        namespaces.AppendLine("using System;");
        namespaces.AppendLine("using System.Collections.Generic;");
        namespaces.AppendLine("using System.Linq;");
        namespaces.AppendLine("using System.Text;");

        if (!string.IsNullOrEmpty(additionalNamespaces))
        {
            foreach (var ns in additionalNamespaces.Split(','))
            {
                namespaces.AppendLine($"using {ns.Trim()};");
            }
        }

        var script = new StringBuilder();
        script.Append(namespaces.ToString());
        script.AppendLine();
        script.AppendLine("class Program");
        script.AppendLine("{");
        script.AppendLine("    static void Main()");
        script.AppendLine("    {");
        script.AppendLine(code);
        script.AppendLine("    }");
        script.AppendLine("}");

        return script.ToString();
    }
}

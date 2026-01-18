using System.ComponentModel;
using System.Text;
using ModelContextProtocol.Server;

/// <summary>
/// MCP tools for file operations including read, write, edit, delete, and more.
/// These tools provide comprehensive file system access for MCP clients.
/// </summary>
internal class FileOperationTools
{
    [McpServerTool]
    [Description("Reads the contents of a file from the specified path.")]
    public string ReadFile(
        [Description("The absolute path to the file to read")] string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                return $"Error: File not found at path '{filePath}'";
            }

            return File.ReadAllText(filePath);
        }
        catch (Exception ex)
        {
            return $"Error reading file: {ex.Message}";
        }
    }

    [McpServerTool]
    [Description("Reads specific lines from a file.")]
    public string ReadFileLines(
        [Description("The absolute path to the file to read")] string filePath,
        [Description("Starting line number (1-based)")] int startLine = 1,
        [Description("Ending line number (1-based, inclusive)")] int? endLine = null)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                return $"Error: File not found at path '{filePath}'";
            }

            var lines = File.ReadAllLines(filePath);
            var actualEndLine = endLine ?? lines.Length;

            if (startLine < 1 || startLine > lines.Length)
            {
                return $"Error: Start line {startLine} is out of range (1-{lines.Length})";
            }

            if (actualEndLine < startLine || actualEndLine > lines.Length)
            {
                return $"Error: End line {actualEndLine} is out of range ({startLine}-{lines.Length})";
            }

            var selectedLines = lines.Skip(startLine - 1).Take(actualEndLine - startLine + 1);
            return string.Join(Environment.NewLine, selectedLines);
        }
        catch (Exception ex)
        {
            return $"Error reading file lines: {ex.Message}";
        }
    }

    [McpServerTool]
    [Description("Writes content to a file. Creates the file if it doesn't exist, or overwrites if it does.")]
    public string WriteFile(
        [Description("The absolute path to the file to write")] string filePath,
        [Description("The content to write to the file")] string content)
    {
        try
        {
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(filePath, content);
            return $"Successfully wrote to file: {filePath}";
        }
        catch (Exception ex)
        {
            return $"Error writing file: {ex.Message}";
        }
    }

    [McpServerTool]
    [Description("Appends content to the end of a file. Creates the file if it doesn't exist.")]
    public string AppendToFile(
        [Description("The absolute path to the file")] string filePath,
        [Description("The content to append")] string content)
    {
        try
        {
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.AppendAllText(filePath, content);
            return $"Successfully appended to file: {filePath}";
        }
        catch (Exception ex)
        {
            return $"Error appending to file: {ex.Message}";
        }
    }

    [McpServerTool]
    [Description("Replaces text in a file. Finds the first occurrence of oldText and replaces it with newText.")]
    public string ReplaceInFile(
        [Description("The absolute path to the file")] string filePath,
        [Description("The text to find and replace")] string oldText,
        [Description("The new text to replace with")] string newText)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                return $"Error: File not found at path '{filePath}'";
            }

            var content = File.ReadAllText(filePath);

            if (!content.Contains(oldText))
            {
                return $"Error: Text to replace not found in file";
            }

            var newContent = content.Replace(oldText, newText);
            File.WriteAllText(filePath, newContent);
            return $"Successfully replaced text in file: {filePath}";
        }
        catch (Exception ex)
        {
            return $"Error replacing text in file: {ex.Message}";
        }
    }

    [McpServerTool]
    [Description("Replaces a specific line in a file with new content.")]
    public string ReplaceLineInFile(
        [Description("The absolute path to the file")] string filePath,
        [Description("The line number to replace (1-based)")] int lineNumber,
        [Description("The new content for the line")] string newContent)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                return $"Error: File not found at path '{filePath}'";
            }

            var lines = File.ReadAllLines(filePath).ToList();

            if (lineNumber < 1 || lineNumber > lines.Count)
            {
                return $"Error: Line number {lineNumber} is out of range (1-{lines.Count})";
            }

            lines[lineNumber - 1] = newContent;
            File.WriteAllLines(filePath, lines);
            return $"Successfully replaced line {lineNumber} in file: {filePath}";
        }
        catch (Exception ex)
        {
            return $"Error replacing line in file: {ex.Message}";
        }
    }

    [McpServerTool]
    [Description("Inserts content at a specific line number in a file.")]
    public string InsertLineInFile(
        [Description("The absolute path to the file")] string filePath,
        [Description("The line number to insert at (1-based)")] int lineNumber,
        [Description("The content to insert")] string content)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                return $"Error: File not found at path '{filePath}'";
            }

            var lines = File.ReadAllLines(filePath).ToList();

            if (lineNumber < 1 || lineNumber > lines.Count + 1)
            {
                return $"Error: Line number {lineNumber} is out of range (1-{lines.Count + 1})";
            }

            lines.Insert(lineNumber - 1, content);
            File.WriteAllLines(filePath, lines);
            return $"Successfully inserted line at position {lineNumber} in file: {filePath}";
        }
        catch (Exception ex)
        {
            return $"Error inserting line in file: {ex.Message}";
        }
    }

    [McpServerTool]
    [Description("Deletes a specific line from a file.")]
    public string DeleteLineInFile(
        [Description("The absolute path to the file")] string filePath,
        [Description("The line number to delete (1-based)")] int lineNumber)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                return $"Error: File not found at path '{filePath}'";
            }

            var lines = File.ReadAllLines(filePath).ToList();

            if (lineNumber < 1 || lineNumber > lines.Count)
            {
                return $"Error: Line number {lineNumber} is out of range (1-{lines.Count})";
            }

            lines.RemoveAt(lineNumber - 1);
            File.WriteAllLines(filePath, lines);
            return $"Successfully deleted line {lineNumber} from file: {filePath}";
        }
        catch (Exception ex)
        {
            return $"Error deleting line from file: {ex.Message}";
        }
    }

    [McpServerTool]
    [Description("Deletes a file from the specified path.")]
    public string DeleteFile(
        [Description("The absolute path to the file to delete")] string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                return $"Error: File not found at path '{filePath}'";
            }

            File.Delete(filePath);
            return $"Successfully deleted file: {filePath}";
        }
        catch (Exception ex)
        {
            return $"Error deleting file: {ex.Message}";
        }
    }

    [McpServerTool]
    [Description("Checks if a file exists at the specified path.")]
    public bool FileExists(
        [Description("The absolute path to check")] string filePath)
    {
        return File.Exists(filePath);
    }

    [McpServerTool]
    [Description("Gets information about a file including size, creation time, and last write time.")]
    public string GetFileInfo(
        [Description("The absolute path to the file")] string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                return $"Error: File not found at path '{filePath}'";
            }

            var fileInfo = new FileInfo(filePath);
            var info = new StringBuilder();
            info.AppendLine($"File: {filePath}");
            info.AppendLine($"Size: {fileInfo.Length} bytes");
            info.AppendLine($"Created: {fileInfo.CreationTime:yyyy-MM-dd HH:mm:ss}");
            info.AppendLine($"Modified: {fileInfo.LastWriteTime:yyyy-MM-dd HH:mm:ss}");
            info.AppendLine($"Accessed: {fileInfo.LastAccessTime:yyyy-MM-dd HH:mm:ss}");
            info.AppendLine($"Extension: {fileInfo.Extension}");
            info.AppendLine($"Is Read-Only: {fileInfo.IsReadOnly}");

            return info.ToString();
        }
        catch (Exception ex)
        {
            return $"Error getting file info: {ex.Message}";
        }
    }

    [McpServerTool]
    [Description("Copies a file from source to destination.")]
    public string CopyFile(
        [Description("The source file path")] string sourcePath,
        [Description("The destination file path")] string destinationPath,
        [Description("Whether to overwrite if destination exists")] bool overwrite = false)
    {
        try
        {
            if (!File.Exists(sourcePath))
            {
                return $"Error: Source file not found at path '{sourcePath}'";
            }

            var directory = Path.GetDirectoryName(destinationPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.Copy(sourcePath, destinationPath, overwrite);
            return $"Successfully copied file from '{sourcePath}' to '{destinationPath}'";
        }
        catch (Exception ex)
        {
            return $"Error copying file: {ex.Message}";
        }
    }

    [McpServerTool]
    [Description("Moves (renames) a file from source to destination.")]
    public string MoveFile(
        [Description("The source file path")] string sourcePath,
        [Description("The destination file path")] string destinationPath,
        [Description("Whether to overwrite if destination exists")] bool overwrite = false)
    {
        try
        {
            if (!File.Exists(sourcePath))
            {
                return $"Error: Source file not found at path '{sourcePath}'";
            }

            var directory = Path.GetDirectoryName(destinationPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.Move(sourcePath, destinationPath, overwrite);
            return $"Successfully moved file from '{sourcePath}' to '{destinationPath}'";
        }
        catch (Exception ex)
        {
            return $"Error moving file: {ex.Message}";
        }
    }

    [McpServerTool]
    [Description("Counts the number of lines in a file.")]
    public int CountLines(
        [Description("The absolute path to the file")] string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                return -1;
            }

            return File.ReadAllLines(filePath).Length;
        }
        catch
        {
            return -1;
        }
    }

    [McpServerTool]
    [Description("Searches for text in a file and returns matching lines.")]
    public string SearchInFile(
        [Description("The absolute path to the file")] string filePath,
        [Description("The text to search for")] string searchText,
        [Description("Whether the search is case-sensitive")] bool caseSensitive = false)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                return $"Error: File not found at path '{filePath}'";
            }

            var lines = File.ReadAllLines(filePath);
            var comparison = caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
            var matchingLines = new StringBuilder();
            var matchCount = 0;

            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].Contains(searchText, comparison))
                {
                    matchingLines.AppendLine($"Line {i + 1}: {lines[i]}");
                    matchCount++;
                }
            }

            if (matchCount == 0)
            {
                return $"No matches found for '{searchText}' in file";
            }

            return $"Found {matchCount} matching line(s):\n{matchingLines}";
        }
        catch (Exception ex)
        {
            return $"Error searching file: {ex.Message}";
        }
    }

    [McpServerTool]
    [Description("Lists all files in a directory.")]
    public string ListFiles(
        [Description("The directory path to list")] string directoryPath,
        [Description("File filter pattern (e.g., '*.txt')")] string? searchPattern = null)
    {
        try
        {
            if (!Directory.Exists(directoryPath))
            {
                return $"Error: Directory not found at path '{directoryPath}'";
            }

            var files = searchPattern != null
                ? Directory.GetFiles(directoryPath, searchPattern)
                : Directory.GetFiles(directoryPath);

            if (files.Length == 0)
            {
                return $"No files found in directory '{directoryPath}'";
            }

            return string.Join(Environment.NewLine, files.Select(f => new FileInfo(f).Name));
        }
        catch (Exception ex)
        {
            return $"Error listing files: {ex.Message}";
        }
    }

    [McpServerTool]
    [Description("Creates a new directory if it doesn't exist.")]
    public string CreateDirectory(
        [Description("The directory path to create")] string directoryPath)
    {
        try
        {
            if (Directory.Exists(directoryPath))
            {
                return $"Directory already exists at path '{directoryPath}'";
            }

            Directory.CreateDirectory(directoryPath);
            return $"Successfully created directory: {directoryPath}";
        }
        catch (Exception ex)
        {
            return $"Error creating directory: {ex.Message}";
        }
    }

    [McpServerTool]
    [Description("Deletes a directory and all its contents.")]
    public string DeleteDirectory(
        [Description("The directory path to delete")] string directoryPath,
        [Description("Whether to recursively delete contents")] bool recursive = false)
    {
        try
        {
            if (!Directory.Exists(directoryPath))
            {
                return $"Error: Directory not found at path '{directoryPath}'";
            }

            Directory.Delete(directoryPath, recursive);
            return $"Successfully deleted directory: {directoryPath}";
        }
        catch (Exception ex)
        {
            return $"Error deleting directory: {ex.Message}";
        }
    }

    [McpServerTool]
    [Description("Checks if a directory exists at the specified path.")]
    public bool DirectoryExists(
        [Description("The directory path to check")] string directoryPath)
    {
        return Directory.Exists(directoryPath);
    }

    [McpServerTool]
    [Description("Gets the absolute path of a file or directory.")]
    public string GetAbsolutePath(
        [Description("The relative or absolute path")] string path)
    {
        try
        {
            return Path.GetFullPath(path);
        }
        catch (Exception ex)
        {
            return $"Error getting absolute path: {ex.Message}";
        }
    }
}

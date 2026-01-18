# MCPFileIO

A comprehensive Model Context Protocol (MCP) server providing file operations, compiler tools, execution capabilities, log monitoring, and process management for AI agent interactions.

## Features

### 📁 File Operations (21 tools)
- **Read Operations**: Read files, specific line ranges, count lines, search within files
- **Write Operations**: Create/overwrite files, append content, insert/replace/delete specific lines
- **File Management**: Copy, move, delete files, check existence, get file info
- **Directory Operations**: Create, delete, list directories and files with pattern filtering

### 🔨 Compiler & Execution (5 tools)
- **Compilation**: Build .NET projects with configuration options (Debug/Release)
- **Execution**: Run dotnet applications, executables, and C# code snippets
- **Error Checking**: Validate compilation without full builds

### 📊 Log Monitoring & Analysis (5 tools)
- **Reading**: Full log content, tail last N lines
- **Search**: Pattern matching with regex support
- **Filtering**: Filter by log level (Error, Warning, Info, Debug)
- **Monitoring**: Track recent entries and file statistics

### ⚙️ Process Management (3 tools)
- **Listing**: View all running processes with memory usage
- **Inspection**: Detailed process information by ID or name
- **Control**: Gracefully terminate or force-kill processes

### 🌐 System Information (2 tools)
- View all environment variables
- Get specific environment variable values

### 🎲 Utility Functions (1 tool)
- Random number generation for testing and utilities

## Installation

1. Clone this repository:
```bash
git clone https://github.com/Sharadendu/MCPFileIO.git
cd MCPFileIO
```

2. Build the project:
```bash
dotnet build
```

3. Run the MCP server:
```bash
dotnet run
```

## Usage

This MCP server uses stdio transport for communication. Configure your MCP client to connect to this server and invoke the available tools.

### Example Tool Invocations

**Read a file:**
```json
{
  "tool": "ReadFile",
  "parameters": {
    "filePath": "C:\\path\\to\\file.txt"
  }
}
```

**Compile a project:**
```json
{
  "tool": "CompileProject",
  "parameters": {
    "projectPath": "C:\\path\\to\\project",
    "configuration": "Release"
  }
}
```

**Search logs:**
```json
{
  "tool": "SearchLogFile",
  "parameters": {
    "logFilePath": "C:\\logs\\app.log",
    "searchPattern": "error",
    "useRegex": false,
    "caseSensitive": false
  }
}
```

## Project Structure

```
LocalAIAction/
├── Program.cs                          # Main entry point and MCP server configuration
├── Tools/
│   ├── RandomNumberTools.cs           # Random number generation utilities
│   ├── FileOperationTools.cs          # Comprehensive file and directory operations
│   └── CompilerDebugExecutionTools.cs # Compiler, execution, debug, and monitoring tools
└── .mcp/
    └── server.json                    # MCP server configuration
```

## Requirements

- .NET 9.0 or later
- Windows OS (some tools are Windows-specific)

## Contributing

Contributions are welcome! Please feel free to submit pull requests or open issues for bugs and feature requests.

## License

This project is provided as-is for educational and development purposes.

## Author

Sharadendu

## Repository

https://github.com/Sharadendu/MCPFileIO

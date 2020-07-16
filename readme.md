# Simple Bulk Rename Tool for Windows

## Requirements

- .NET Core SDK ([download](https://dotnet.microsoft.com/download))
- VS Code
- `PATH` environment variable contains location of VS Code

## Installation

- build with .NET Core: `dotnet build`
- execute install script `install.ps1` with elevated rights

## Usage

- right-click on the background of a directory in the file explorer
- select `Rename with Code`
- edit the file/directory names in the file opened with VS Code
- save the file
- *the file is now automatically deleted and the files/directories are renamed*
- close the (deleted) file in VS Code

*Note: it is also possible to save the file without any changes*

*Note: if you close the file without saving, a temporary file `.bulk_rename` will remain in the directory and an instance of the Bulk Rename Tool will live on in idle state; if you manually delete the file, the program instance will shutdown automatically*

using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;

// Small utility: extract a .zip or .cab into a target folder.
// Usage: the app will prompt for a destination folder and then the path to a .zip or .cab file.

// Top-level program: prompts for destination and archive, then extracts.
Console.WriteLine("Enter the destination folder where the archive should be extracted:");
string? destFolder = Console.ReadLine()?.Trim();

if (string.IsNullOrWhiteSpace(destFolder))
{
    Console.WriteLine("Destination folder cannot be empty. Exiting.");
    return;
}

try
{
    Directory.CreateDirectory(destFolder);
}
catch (Exception ex)
{
    Console.WriteLine($"Failed to create or access destination folder: {ex.Message}");
    return;
}

Console.WriteLine("Enter the path to a .zip or .cab file to extract:");
string? archivePath = Console.ReadLine()?.Trim('"', ' ');

if (string.IsNullOrWhiteSpace(archivePath) || !File.Exists(archivePath))
{
    Console.WriteLine("Archive path is invalid or the file does not exist. Exiting.");
    return;
}

string ext = Path.GetExtension(archivePath).ToLowerInvariant();

try
{
    if (ext == ".zip" || ext == ".cab")
    {
        ExtractZip(archivePath, destFolder);
    }

    else
    {
        Console.WriteLine("Unsupported file type. Only .zip and .cab are supported.");
        return;
    }

    Console.WriteLine("Extraction completed successfully.");
}
catch (Exception ex)
{
    Console.WriteLine($"Extraction failed: {ex.Message}");
}

// Local helper functions for top-level program
void ExtractZip(string zipPath, string destFolder)
{
    // Overwrite existing files
    using var archive = ZipFile.OpenRead(zipPath);
    foreach (var entry in archive.Entries)
    {
        // Skip directory entries
        if (string.IsNullOrEmpty(entry.Name) && entry.FullName.EndsWith("/"))
            continue;

        string destinationPath = Path.GetFullPath(Path.Combine(destFolder, entry.FullName));

        // Prevent zip-slip vulnerability
        if (!destinationPath.StartsWith(Path.GetFullPath(destFolder), StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Entry is outside the target dir: " + entry.FullName);

        string? directory = Path.GetDirectoryName(destinationPath);
        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);

        entry.ExtractToFile(destinationPath, overwrite: true);
    }
}

void ExtractCab(string cabPath, string destFolder)
{
    var psi = new ProcessStartInfo
    {
        FileName = "expand.exe",
        Arguments = $"-F:* \"{cabPath}\" \"{destFolder}\"",
        UseShellExecute = false,
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        CreateNoWindow = true
    };

    using var proc = Process.Start(psi) ?? throw new InvalidOperationException("Failed to start expand.exe");
    string stdout = proc.StandardOutput.ReadToEnd();
    string stderr = proc.StandardError.ReadToEnd();
    proc.WaitForExit();

    if (proc.ExitCode != 0)
    {
        throw new InvalidOperationException($"expand.exe failed (exit {proc.ExitCode}): {stderr}\n{stdout}");
    }
}


Console.WriteLine("What feature are you trying to troubleshoot?\n1. High memory\n2. HighCPU\n");
int tsSelection;
string input = Console.ReadLine()?.Trim();
while (!int.TryParse(input, out tsSelection) || (tsSelection != 1 && tsSelection != 2))
{
    Console.WriteLine("Please enter 1 or 2:");
    input = Console.ReadLine()?.Trim();
}

string tsFeature;
if (tsSelection == 1)
{
    tsFeature = "High memory";
}
else // tsSelection == 2
{
    tsFeature = "High CPU";
}

Console.WriteLine($"You selected {tsFeature}");

Console.WriteLine("Press Enter to begin troubleshooting...");
Console.ReadLine();

if (tsSelection == 1)
{
    HighMemoryTroubleshooting();
}
else
{
    HighCpuTroubleshooting();
}

// Performs troubleshooting steps for high memory usage.
void HighMemoryTroubleshooting()
{
    Console.WriteLine("Starting high memory troubleshooting...");
    // Add your high memory troubleshooting logic here.
    // in the files extracted above, look for a .txt file titled "highmemory.txt" and print the contents to the console.
    try
    {
        string[] files = Directory.GetFiles(destFolder, "highmemory.txt", SearchOption.AllDirectories);
        if (files.Length > 0)
        {
            Console.WriteLine("Contents of highmemory.txt:");
            string contents = File.ReadAllText(files[0]);
            Console.WriteLine(contents);
        }
        else
        {
            Console.WriteLine("highmemory.txt not found in the extracted files.");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error reading highmemory.txt: {ex.Message}");
    }
    
    Console.WriteLine("High memory troubleshooting completed.");
}

// Performs troubleshooting steps for high CPU usage.
void HighCpuTroubleshooting()
{
    Console.WriteLine("Starting high CPU troubleshooting...");
    // in the files extracted above, look for a .txt file titled "highcpu.txt" and print the contents to the console.
    try
    {
        string[] files = Directory.GetFiles(destFolder, "highcpu.txt", SearchOption.AllDirectories);
        if (files.Length > 0)
        {
            Console.WriteLine("Contents of highcpu.txt:");
            string contents = File.ReadAllText(files[0]);
            Console.WriteLine(contents);
        }
        else
        {
            Console.WriteLine("highcpu.txt not found in the extracted files.");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error reading highcpu.txt: {ex.Message}");
    }
    
    Console.WriteLine("High CPU troubleshooting completed.");
}
Console.ReadLine();
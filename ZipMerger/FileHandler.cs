using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using ZipMerger.ViewModels;
using ZipMerger.Views;

namespace ZipMerger;

public static class FileHandler
{
    internal static MainView MainView { get; set; } = null!;

    /// <summary>
    /// Opens a file picker dialog to select files for importing Zip Archives.
    /// </summary>
    public static async Task BrowseFiles()
    {
        // Gets the top level from the main view
        var topLevel = TopLevel.GetTopLevel(MainView);
        
        var files = await topLevel!.StorageProvider.OpenFilePickerAsync(new()
        {
            Title = "Open Zip Files",
            AllowMultiple = true,
            FileTypeFilter = new List<FilePickerFileType>
            {
                new("Zip Files")
                {
                    Patterns = new List<string> { "*.zip", "*.7z", "*.rar" }
                }
            }
        });
        
        foreach (var file in files)
        {
            ConsoleExt.WriteLineWithPretext($"Selected File: '{file.Name}'");
            AddPathToQueue(new(file.Path.LocalPath), MainViewModel.SelectedPathDisplay);
        }
    }
    
    /// <summary>
    /// Opens a folder picker dialog to select folders for choosing the output path.
    /// </summary>
    public static async Task BrowseFolders(MainViewModel mainViewModel)
    {
        // Gets the top level from the main view
        var topLevel = TopLevel.GetTopLevel(MainView);
        
        var folder = await topLevel!.StorageProvider.OpenFolderPickerAsync(new()
        {
            Title = "Choose Output Folder",
        });

        mainViewModel.OutputPath = folder[0].Path.LocalPath;
        ConsoleExt.WriteLineWithPretext($"Selected Output Folder: '{mainViewModel.OutputPath}'");
    }
    
    /// <summary>
    /// Adds the selected path to the queue for Scanning.
    /// </summary>
    /// <param name="importSettings">Import settings</param>
    /// <param name="selectedPathDisplay">List of selected paths</param>
    public static void AddPathToQueue(ImportSettings? importSettings, ObservableCollection<ImportSettings> selectedPathDisplay)
    {
        if (!Path.Exists(importSettings?.SelectedPath))
        {
            ConsoleExt.WriteLineWithPretext($"Path does not exist: '{importSettings?.SelectedPath}'");
            return;
        }
        selectedPathDisplay.Add(importSettings);
        ConsoleExt.WriteLineWithPretext($"Added Path: '{importSettings.SelectedPath}'");
    }

    /// <summary>
    /// Merges the selected Folders into a single Folder and deduplicates the files.
    /// Source Directory 1 will be merged into Source Directory 2
    /// </summary>
    /// <param name="sourceDirectory1">Source Directory 1</param>
    /// <param name="sourceDirectory2">Source Directory 2</param>
    /// <param name="mainViewModel">Main View Model</param>
    public static void MergeFolders(string sourceDirectory1, string sourceDirectory2, MainViewModel mainViewModel)
    {
        // Get all files from both directories
        var files1 = Directory.GetFiles(sourceDirectory1, "*.*", SearchOption.AllDirectories);
        var files2 = Directory.GetFiles(sourceDirectory2, "*.*", SearchOption.AllDirectories);

        ProgressBarSettings progressBarSettings = new ProgressBarSettings(files1.Length);
        mainViewModel.AddProgressItem(progressBarSettings);

        HashSet<string> uniqueFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase); // Case-insensitive hash set for unique hashes
        
        foreach (var file in files1)
        {
            string hash = HelperClass.GetMd5Checksum(file);
            if (!uniqueFiles.Add(hash)) continue;
            string destinationPath = Path.Combine(sourceDirectory2, Path.GetFileName(file));
            File.Copy(file, destinationPath, true);
            progressBarSettings.ProgressValue++;
        }
        
        ConsoleExt.WriteLineWithPretext($"Merged folders: '{sourceDirectory1}' into '{sourceDirectory2}'");
    }
}
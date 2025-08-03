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
    /// Merges the selected Folders into a single Folder and deduplicates the files by simple overwriting.
    /// </summary>
    /// <param name="sourceDirectories">List of source directories</param>
    /// <param name="mainViewModel">Main View Model</param>
    public static string MergeFolders(List<string> sourceDirectories, MainViewModel mainViewModel)
    {
        string folderMergeDirectory = sourceDirectories[0];
        for (int i = 1; i < sourceDirectories.Count; i++)
        {
            // Get all files from the indexed directory [i]
            var files1 = Directory.GetFiles(sourceDirectories[i], "*.*", SearchOption.AllDirectories);

            ProgressBarSettings progressBarSettings = new ProgressBarSettings(files1.Length);
            mainViewModel.AddProgressItem(progressBarSettings);

            HashSet<string> uniqueFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase); // Case-insensitive hash set for unique hashes
        
            foreach (var file in files1)
            {
                string hash = HelperClass.GetMd5Checksum(file);
                if (!uniqueFiles.Add(hash)) continue;
                string destinationPath = Path.Combine(folderMergeDirectory, Path.GetFileName(file));
                File.Copy(file, destinationPath, true);
                progressBarSettings.ProgressValue++;
            }
        
            mainViewModel.AppendToConsole(ConsoleExt.WriteLineWithStepPretext($"Merged folders: '{sourceDirectories[i]}' into '{folderMergeDirectory}'", ConsoleExt.CurrentStep.MergingFiles).output);
            
            mainViewModel.CurrentPass++;
        }
        
        return folderMergeDirectory;
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using ZipMerger.ViewModels;

namespace ZipMerger;

public abstract class ZipHandler
{
    public static async Task StartExtractingAsync(List<ImportSettings> selectedPaths, MainViewModel mainViewModel, string? outputPath = null)
    {
        mainViewModel.IsCompressionLevelEnabled = false;
        ProgressBarSettings progressBarSettings = new ProgressBarSettings(selectedPaths.Count);
        mainViewModel.AddProgressItem(progressBarSettings);
        mainViewModel.TotalPasses = (selectedPaths.Count * 2) + selectedPaths.Count; // 2 passes per zip to extract and merge then 1 pass per two sets of zips for merging the extracted folders plus 1 for the final compression
        string destinationDirectory = outputPath ?? Path.Combine(Path.GetDirectoryName(Environment.CurrentDirectory)!, "ExtractedZips");

        try
        {
            
            foreach (var path in selectedPaths)
            {
                await ExtractZipAsync(path.SelectedPath, destinationDirectory, mainViewModel);
                progressBarSettings.ProgressValue++;
                mainViewModel.CurrentPass++;
            }
            MergeAndZip(Directory.GetDirectories(destinationDirectory).ToList(), outputPath!, mainViewModel);
            mainViewModel.IsCompressionLevelEnabled = true;
        }             
        catch (Exception ex)
        {
            mainViewModel.IsCompressionLevelEnabled = true;
            ConsoleExt.WriteLineWithPretext($"An error occurred: {ex.Message}", ConsoleExt.OutputType.Error, ex);
        }
    }

    public static async Task ExtractZipAsync(string? zipFilePath, string destinationDirectory, MainViewModel mainViewModel)
    {
        ProgressBarSettings? progressBarSettings = null;
        await Task.Run(() =>
        {
            if (!File.Exists(zipFilePath))
                throw new FileNotFoundException($"The file {zipFilePath} does not exist.");

            if (!Directory.Exists(destinationDirectory))
                Directory.CreateDirectory(destinationDirectory);

            using var archive = ZipFile.OpenRead(zipFilePath);
            
            // Set the progress bar max value to the number of entries in the zip file
            progressBarSettings = new ProgressBarSettings(archive.Entries.Count);
            mainViewModel.AddProgressItem(progressBarSettings);
            
            foreach (var entry in archive.Entries)
            {
                mainViewModel.CurrentFile = entry.FullName;
                ConsoleExt.WriteLineWithPretext($"Extracting: {entry.FullName}");
                var destinationPath = Path.Combine(destinationDirectory, entry.FullName);
                ConsoleExt.WriteLineWithPretext($"Extracting To: {destinationPath}");

                if (entry.Name == "")
                {
                    Directory.CreateDirectory(destinationPath);
                }
                else
                {
                    var destDir = Path.GetDirectoryName(destinationPath);
                    if (!string.IsNullOrEmpty(destDir) && !Directory.Exists(destDir))
                        Directory.CreateDirectory(destDir);

                    entry.ExtractToFile(destinationPath, overwrite: true); // I need to set this to extract to the extracted folder without the .zip extension and don't have the test 1 and test 2 folders combine
                }
                progressBarSettings.ProgressValue++;
            }
        });
        if (progressBarSettings != null)
        {
            mainViewModel.RemoveProgressItem(progressBarSettings);
        }
    }


    private static void CompressZip(string sourceDirectory, string zipFilePath, MainViewModel mainViewModel)
    {
        List<string> files = Directory.GetFiles(sourceDirectory, "*.*", SearchOption.AllDirectories).ToList();
        ProgressBarSettings progressBarSettings = new ProgressBarSettings(files.Count);
        mainViewModel.AddProgressItem(progressBarSettings);
        
        using var archive = ZipFile.Open(zipFilePath, ZipArchiveMode.Create); // something goes wrong here where the F:\Rider Projects\Zip Merger\ZipMerger\ZipMerger.Desktop\Testing\Output\TestOutput 0.zip gets an access violation
        foreach (var file in files)
        {
            var entry = archive.CreateEntry(Path.GetFileName(file), MainViewModel.SelectedOption);
            using var stream = entry.Open();
            using var fileStream = File.OpenRead(file);
            fileStream.CopyTo(stream);
            progressBarSettings.ProgressValue++;
        }
    }

    private static void MergeAndZip(List<string> sourceDirectories, string zipFilePath, MainViewModel mainViewModel)
    {
        string folderMergeDirectory = sourceDirectories[0];
        for (int i = 1; i < sourceDirectories.Count; i++)
        {
            FileHandler.MergeFolders(sourceDirectories[i], folderMergeDirectory, mainViewModel);
            mainViewModel.CurrentPass++;
        }
        
        CompressZip(folderMergeDirectory, zipFilePath, mainViewModel);
        mainViewModel.CurrentPass++;
    }
}
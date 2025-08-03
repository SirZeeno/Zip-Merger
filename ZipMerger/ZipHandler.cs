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
    public static async Task StartZipMergeAsync(List<ImportSettings> selectedPaths, MainViewModel mainViewModel, string? outputPath = null)
    {
        mainViewModel.IsCompressionLevelEnabled = false;
        ProgressBarSettings progressBarSettings = new ProgressBarSettings(selectedPaths.Count);
        mainViewModel.AddProgressItem(progressBarSettings);
        //TODO: Fix the amount of passes to be correct
        mainViewModel.TotalPasses = (selectedPaths.Count * 2) + selectedPaths.Count; // 2 passes per zip to extract and merge, then 1 pass per two sets of zips for merging the extracted folders plus 1 for the final compression
        string destinationDirectory = Path.Combine(outputPath, "Temp");
        mainViewModel.AppendToConsole(ConsoleExt.WriteLineWithStepPretext($"Destination Directory: {destinationDirectory}", ConsoleExt.CurrentStep.Main).output);
        
        try
        {
            
            foreach (var path in selectedPaths)
            {
                await ExtractZipAsync(path.SelectedPath, destinationDirectory, mainViewModel);
                progressBarSettings.ProgressValue++;
                mainViewModel.CurrentPass++;
            }
            outputPath ??= Path.Combine(Path.GetDirectoryName(Environment.CurrentDirectory)!, "Output");
            CompressZip(FileHandler.MergeFolders(Directory.GetDirectories(destinationDirectory).ToList(), mainViewModel), outputPath, mainViewModel);
            mainViewModel.IsCompressionLevelEnabled = true;
        }             
        catch (Exception ex)
        {
            mainViewModel.IsCompressionLevelEnabled = true;
            mainViewModel.AppendToConsole(ConsoleExt.WriteLineWithStepPretext($"An error occurred: {ex.Message}", ConsoleExt.CurrentStep.Main, ConsoleExt.OutputType.Error, ex).output);
        }
        Directory.Delete(destinationDirectory, true);
        
        mainViewModel.AppendToConsole(ConsoleExt.WriteLineWithStepPretext($"Finished! Your Zips have been merged and the output is at {outputPath}", ConsoleExt.CurrentStep.Main).output);
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
                var destinationPath = Path.Combine(destinationDirectory, entry.FullName);
                mainViewModel.AppendToConsole(ConsoleExt.WriteLineWithStepPretext($"Extracting {entry.FullName} --To--> {destinationPath}", ConsoleExt.CurrentStep.ExtractingFiles).output);

                if (entry.Name == "")
                {
                    Directory.CreateDirectory(destinationPath);
                }
                else
                {
                    var destDir = Path.GetDirectoryName(destinationPath);
                    if (!string.IsNullOrEmpty(destDir) && !Directory.Exists(destDir))
                        Directory.CreateDirectory(destDir);

                    entry.ExtractToFile(destinationPath, overwrite: true);
                }
                progressBarSettings.ProgressValue++;
            }
        });
        if (progressBarSettings != null)
        {
            mainViewModel.RemoveProgressItem(progressBarSettings);
        }
    }


    //TODO: Allow the user to define the name of the output Zip
    public static void CompressZip(string sourceDirectory, string zipFilePath, MainViewModel mainViewModel)
    {
        List<string> files = Directory.GetFiles(sourceDirectory, "*.*", SearchOption.AllDirectories).ToList();
        ProgressBarSettings progressBarSettings = new ProgressBarSettings(files.Count);
        mainViewModel.AddProgressItem(progressBarSettings);
        
        using var archive = ZipFile.Open(Path.Combine(zipFilePath, "Output.zip"), ZipArchiveMode.Create);
        foreach (var file in files)
        {
            var entry = archive.CreateEntry(Path.GetFileName(file), MainViewModel.SelectedCompressionOption);
            using var stream = entry.Open();
            using var fileStream = File.OpenRead(file);
            fileStream.CopyTo(stream);
            progressBarSettings.ProgressValue++;
        }
        mainViewModel.CurrentPass++;
    }
}
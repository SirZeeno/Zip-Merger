using ZipMerger.ViewModels;

namespace ZipMerger.UnitTesting;

public class Tests
{
    private MainViewModel _mainViewModel;
    private readonly string? _parentFullName = new DirectoryInfo(Directory.GetCurrentDirectory()).Parent?.Parent?.Parent?.Parent?.FullName;
    
    [OneTimeSetUp]
    public void Setup()
    {
        // Initialize the ViewModel
        _mainViewModel= new MainViewModel();
    }
    
    [Test, Order(1)]
    public async Task TestZipExtractorDesktop()
    {
        string testZipLocation = "F:\\Rider Projects\\Zip Merger\\ZipMerger\\ZipMerger.Desktop\\Testing\\Test";
        string[] testZips = Directory.GetFiles(testZipLocation, "*.zip", SearchOption.AllDirectories);
        string destinationDirectory = "F:\\Rider Projects\\Zip Merger\\ZipMerger\\ZipMerger.Desktop\\Testing\\Output\\Extracted";
        string finishedExtractionDirectory = "F:\\Rider Projects\\Zip Merger\\ZipMerger\\ZipMerger.Desktop\\Testing\\Output\\Extracted";
        string testingDirectory = "F:\\Rider Projects\\Zip Merger\\ZipMerger\\ZipMerger.Desktop\\Testing";

        foreach (var testZip in testZips)
        {
            await ZipHandler.ExtractZipAsync(testZip, destinationDirectory, _mainViewModel);
        }
        
        if (ConsoleExt.ExceptionOccurred)
        {
            Assert.Fail();
            ConsoleExt.WriteLineWithPretext($"Test failed due to exception(s): {ConsoleExt.Exceptions}");
        }

        bool passed = true;
        
        string[] finishedExtractionDirectories = Directory.GetDirectories(finishedExtractionDirectory);
        foreach (var directory in finishedExtractionDirectories)
        {
            string testName = new DirectoryInfo(directory).Name;
            string solutionDirectory = Path.Combine(testingDirectory, testName);
            
            if (!HelperClass.CompareDirectories(finishedExtractionDirectory, solutionDirectory))
            {
                passed = false;
            }
        }
        
        if (passed)
        {
            Assert.Pass();
            ConsoleExt.WriteLineWithPretext("Extract Test passed!");
        }
    }
    
    [Test, Order(2)]
    public void TestFolderMergerDesktop()
    {
        string extractedFolder = "F:\\Rider Projects\\Zip Merger\\ZipMerger\\ZipMerger.Desktop\\Testing\\Output\\Extracted";
        string solutionFolder = "F:\\Rider Projects\\Zip Merger\\ZipMerger\\ZipMerger.Desktop\\Testing\\Test Solution";

        string mergedFolder = FileHandler.MergeFolders(Directory.GetDirectories(extractedFolder).ToList(), _mainViewModel);
        
        if (ConsoleExt.ExceptionOccurred)
        {
            Assert.Fail();
            ConsoleExt.WriteLineWithPretext($"Test failed due to exception(s): {ConsoleExt.Exceptions}");
        }

        if (!HelperClass.CompareDirectories(mergedFolder, solutionFolder))
        {
            Assert.Fail();
            ConsoleExt.WriteLineWithPretext($"Test failed due to Folder Mismatch!");
        }
        Assert.Pass();
        ConsoleExt.WriteLineWithPretext("Merge Test passed!");
    }

    [Test, Order(3)]
    public void TestZipCompressorDesktop()
    {
        string extractedFolder = "F:\\Rider Projects\\Zip Merger\\ZipMerger\\ZipMerger.Desktop\\Testing\\Output\\Extracted";
        string zipFilePath = "F:\\Rider Projects\\Zip Merger\\ZipMerger\\ZipMerger.Desktop\\Testing\\Output\\Output\\TestOutput.zip";
        string solutionFolder = "F:\\Rider Projects\\Zip Merger\\ZipMerger\\ZipMerger.Desktop\\Testing\\Zip Solution\\TestOutput.zip";

        ZipHandler.CompressZip(extractedFolder, zipFilePath, _mainViewModel);
        
        if (ConsoleExt.ExceptionOccurred)
        {
            Assert.Fail();
            ConsoleExt.WriteLineWithPretext($"Test failed due to exception(s): {ConsoleExt.Exceptions}");
        }
        
        if (!HelperClass.CompareFiles(zipFilePath, solutionFolder)) return;
        Assert.Pass();
        ConsoleExt.WriteLineWithPretext("Compression Test passed!");
    }

    [OneTimeTearDown]
    public void TestCleanup()
    {
        if (_parentFullName == null) return;
        string extractedFolder = "F:\\Rider Projects\\Zip Merger\\ZipMerger\\ZipMerger.Desktop\\Testing\\Output\\Extracted";
        string outputFolder = "F:\\Rider Projects\\Zip Merger\\ZipMerger\\ZipMerger.Desktop\\Testing\\Output\\Output";
        string testFolder = "F:\\Rider Projects\\Zip Merger\\ZipMerger\\ZipMerger.Desktop\\Testing\\Output";
        Directory.GetDirectories(extractedFolder).ToList().ForEach(d => Directory.Delete(d, true));
        Directory.GetFiles(outputFolder).ToList().ForEach(File.Delete);
        Directory.GetFiles(testFolder, "*.zip*").ToList().ForEach(File.Delete); // throws an exception if the files are in use
        
        ConsoleExt.WriteLineWithPretext("Test Cleanup complete!");
    }
}
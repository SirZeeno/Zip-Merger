using ZipMerger.ViewModels;

namespace ZipMerger.UnitTesting;

public class Tests
{
    private MainViewModel _mainViewModel;
    private readonly string? _parentFullName = new DirectoryInfo(Directory.GetCurrentDirectory()).Parent?.Parent?.Parent?.Parent?.FullName;
    
    [SetUp]
    public void Setup()
    {
        // Initialize the ViewModel
        _mainViewModel= new MainViewModel();
    }
    
    //[Test]
    public async Task TestZipExtractor()
    {
        string testZip = "F:\\Rider Projects\\Zip Merger\\ZipMerger\\ZipMerger.Desktop\\Testing\\Test\\Test 1.zip";
        string destinationDirectory = "F:\\Rider Projects\\Zip Merger\\ZipMerger\\ZipMerger.Desktop\\Testing\\Output\\Extracted";
        await ZipHandler.ExtractZipAsync(testZip, destinationDirectory, _mainViewModel);
        
        if (ConsoleExt.ExceptionOccurred)
        {
            Assert.Fail();
            ConsoleExt.WriteLineWithPretext($"Test failed due to exception(s): {ConsoleExt.Exceptions}");
        }
    }

    [Test] 
    // Tests if the output of ZipMerger is merged and deduplicated correctly
    public async Task TestZipMergerOutputDesktop()
    {
        // get the expected output results folder
        if (_parentFullName != null)
        {
            string expectedOutputResultsFolder = "F:\\Rider Projects\\Zip Merger\\ZipMerger\\ZipMerger.Desktop\\Testing\\Test Solution";
            string actualOutputResultsFolder = "F:\\Rider Projects\\Zip Merger\\ZipMerger\\ZipMerger.Desktop\\Testing\\Output";

            // merge the test zip files
            string testZip1 = "F:\\Rider Projects\\Zip Merger\\ZipMerger\\ZipMerger.Desktop\\Testing\\Test\\Test 1.zip";
            string testZip2 = "F:\\Rider Projects\\Zip Merger\\ZipMerger\\ZipMerger.Desktop\\Testing\\Test\\Test 2.zip";
            string outputZip = Path.Combine(actualOutputResultsFolder, "TestOutput.zip");
            
            await ZipHandler.StartExtractingAsync([new(testZip1), new(testZip2)], _mainViewModel, outputZip);
        
            // extract the output zip file
            string outputFolder = "F:\\Rider Projects\\Zip Merger\\ZipMerger\\ZipMerger.Desktop\\Testing\\Output\\Output";
            List<string> outputZips = Directory.GetFiles(outputFolder,"*.zip", SearchOption.AllDirectories).ToList();
            for (int i = 0; i < outputZips.Count; i++)
            {
                string zipFile = outputZips[i];
                string extractedOutputFolder = $"F:\\Rider Projects\\Zip Merger\\ZipMerger\\ZipMerger.Desktop\\Testing\\Output\\Extracted\\TestOutput {i}";
                await ZipHandler.ExtractZipAsync(zipFile, extractedOutputFolder, _mainViewModel);
            }
        
            // compare the contents of both folders
            bool testFailed = false;
            List<string> extractedDirectories = Directory.GetDirectories(Path.Combine(actualOutputResultsFolder,"Extracted")).ToList();
            foreach (var directory in extractedDirectories)
            {
                if (!HelperClass.CompareDirectories(directory, expectedOutputResultsFolder))
                {
                    testFailed = true;
                }
            }
            if (testFailed || ConsoleExt.ExceptionOccurred)
            {
                Assert.Fail();
                ConsoleExt.WriteLineWithPretext(ConsoleExt.ExceptionOccurred
                    ? $"Test failed due to exception(s): {ConsoleExt.Exceptions}"
                    : "Test failed!");
            }
            ConsoleExt.WriteLineWithPretext("Test passed!");
            Assert.Pass();
        }
    }

    //[TearDown]
    public void TestCleanup()
    {
        if (_parentFullName == null) return;
        string extractedFolder = "F:\\Rider Projects\\Zip Merger\\ZipMerger\\ZipMerger.Desktop\\Testing\\Output\\Extracted";
        string outputFolder = "F:\\Rider Projects\\Zip Merger\\ZipMerger\\ZipMerger.Desktop\\Testing\\Output\\Output";
        string testFolder = "F:\\Rider Projects\\Zip Merger\\ZipMerger\\ZipMerger.Desktop\\Testing\\Output";
        Directory.GetDirectories(extractedFolder).ToList().ForEach(d => Directory.Delete(d, true));
        Directory.GetFiles(outputFolder).ToList().ForEach(File.Delete);
        Directory.GetFiles(testFolder, "*.zip*").ToList().ForEach(File.Delete);
    }
}
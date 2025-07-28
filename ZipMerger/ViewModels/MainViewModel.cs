using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO.Compression;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Windows.Input;
using ReactiveUI;

namespace ZipMerger.ViewModels;

public class MainViewModel : ViewModelBase
{
    private string? _inputPath;
    private string? _outputPath; 
    private int _currentPass;
    private int _totalPasses;
    private string? _currentFile;
    private double _progressMaxValue;
    private double _progressValue;
    private bool _isProgressVisible;
    private string _consoleOutput = "";

    public static ObservableCollection<ImportSettings> SelectedPathDisplay { get; set; } = [];
    public static ObservableCollection<ProgressBarSettings> OutputDisplay { get; set; } = [];
    public static ObservableCollection<ProgressBarSettings> ProgressBarDisplay { get; set; } = [];
    private ObservableCollection<ImportSettings> Items => SelectedPathDisplay;
    public ICommand BrowseFilesCommand { get; }
    public ICommand AddPathToQueueCommand { get; }
    public ICommand StartMergeCommand { get; set; }
    public ICommand BrowseFolderCommand { get; }
    public ICommand OpenHyperlinkCommand { get; }
    public ReactiveCommand<ImportSettings, Unit> RemovePathFromQueueCommand { get; }
    public bool IsCompressionLevelEnabled { get; set; } = true;

    public string? PathTextBox
    {
        get => _inputPath;
        set => this.RaiseAndSetIfChanged(ref _inputPath, value);
    }
    
    public string? OutputPath
    {
        get => _outputPath;
        set => this.RaiseAndSetIfChanged(ref _outputPath, value);
    }
    
    public int CurrentPass
    {
        get => _currentPass;
        set => this.RaiseAndSetIfChanged(ref _currentPass, value);
    }
    
    public int TotalPasses
    {
        get => _totalPasses;
        set => this.RaiseAndSetIfChanged(ref _totalPasses, value);
    }
    
    public string? CurrentFile
    {
        get => _currentFile;
        set => this.RaiseAndSetIfChanged(ref _currentFile, value);
    }
    
    public static CompressionLevel SelectedOption { get; set; }
    public IEnumerable<CompressionLevel> Options { get; } = Enum.GetValues(typeof(CompressionLevel)).Cast<CompressionLevel>();
    
    public double ProgressMaxValue
    {
        get => _progressMaxValue;
        set => this.RaiseAndSetIfChanged(ref _progressMaxValue, value);
    }
    
    public double ProgressValue
    {
        get => _progressValue;
        set => this.RaiseAndSetIfChanged(ref _progressValue, value);
    }
    
    public bool IsProgressVisible
    {
        get => _isProgressVisible;
        set => this.RaiseAndSetIfChanged(ref _isProgressVisible, value);
    }
    
    public string ConsoleOutput
    {
        get => _consoleOutput;
        set => this.RaiseAndSetIfChanged(ref _consoleOutput, value);
    }
    
    void RemoveItem(ImportSettings item)
    {
        SelectedPathDisplay.Remove(item);
        ConsoleExt.WriteLineWithPretext($"Removed Path: '{item.SelectedPath}'");
    }
    
    public void AddProgressItem(ProgressBarSettings item)
    {
        ProgressBarDisplay.Add(item);
    }
    
    public void RemoveProgressItem(ProgressBarSettings item)
    {
        ProgressBarDisplay.Remove(item);
    }
    
    public void AppendToConsole(string message)
    {
        ConsoleOutput += message + Environment.NewLine;
    }

    public MainViewModel()
    {
        IObservable<bool> canExecuteAdd = this.WhenAnyValue(vm => vm.PathTextBox, path => !string.IsNullOrEmpty(path));
        IObservable<bool> canExecuteMerge = this.WhenAnyValue(x => x.Items.Count).Select(count => count >= 2);
        OpenHyperlinkCommand = ReactiveCommand.Create<string>(url => Process.Start(new ProcessStartInfo { FileName = url, UseShellExecute = true }));
        StartMergeCommand = ReactiveCommand.Create(() => ZipHandler.StartExtractingAsync(SelectedPathDisplay.ToList(), this, OutputPath), canExecuteMerge);
        BrowseFilesCommand = ReactiveCommand.Create(FileHandler.BrowseFiles);
        BrowseFolderCommand = ReactiveCommand.Create(() => FileHandler.BrowseFolders(this));
        AddPathToQueueCommand = ReactiveCommand.Create<string?>(path => FileHandler.AddPathToQueue(new ImportSettings(path), SelectedPathDisplay), canExecuteAdd);
        RemovePathFromQueueCommand = ReactiveCommand.Create<ImportSettings>(RemoveItem);
    }
}
using System;
using ReactiveUI;

namespace ZipMerger;

public class ImportSettings(string? selectedPath)
{
    public string? SelectedPath { get; } = selectedPath;
}

public class OutputText(ConsoleExt.OutputType outputType, string? output, Exception exception)
{
    ConsoleExt.OutputType OutputType { get; } = outputType;
    string? Output { get; } = output;
    Exception Exception { get; } = exception;
}

public class ProgressBarSettings : ReactiveObject
{
    private double _progressMaxValue;
    private double _progressValue;
    private bool _isProgressVisible;

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

    public ProgressBarSettings(double progressMaxValue, double progressValue = 0, bool isProgressVisible = true)
    {
        ProgressMaxValue = progressMaxValue;
        ProgressValue = progressValue;
        IsProgressVisible = isProgressVisible;
    }
}
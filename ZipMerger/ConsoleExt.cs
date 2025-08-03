﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


namespace ZipMerger;

public static class ConsoleExt
{
    public static bool ExceptionOccurred;
    public static readonly List<Exception> Exceptions = new();
    
    public enum OutputType
    {
        Error,
        Info,
        Warning,
        Question
    }
    
    // This is changeable to be whatever you want
    public enum CurrentStep
    {
        SelectingFiles,
        ExtractingFiles,
        MergingFiles,
        CompressingFiles,
        Main,
        None
    }

    /// <summary>
    /// Writes a line to the console with a pretext based on the output type.
    /// </summary>
    /// <param name="output">Output</param>
    /// <param name="outputType">Output type, default is info</param>
    /// <param name="exception">Exception, default is null</param>
    /// <typeparam name="T">Any type</typeparam>
    /// <returns>The length of the pretext</returns>
    public static (int length, string output) WriteLineWithPretext<T>(T output, OutputType outputType = OutputType.Info, Exception? exception = null)
    {
        var length1 = CurrentTime();
        var length2 = DetermineOutputType(outputType);
        if (output is IEnumerable enumerable && !(output is string))
        {
            Console.WriteLine(string.Join(", ", enumerable.Cast<object>()));
        }
        else
        {
            Console.WriteLine(output);
        }
        var constructedReturn = (length1.length + length2.length, length1.pretext + length2.pretext + output);
        
        if (exception == null) return constructedReturn;
        ExceptionOccurred = true;
        Exceptions.Add(exception);
        Console.WriteLine($"Exception: {exception.Message}");
        Console.WriteLine($"Stack Trace: {exception.StackTrace}");
        constructedReturn.Item2 += $"\nException: {exception.Message}\nStack Trace: {exception.StackTrace}";
        return constructedReturn;
    }
    
    /// <summary>
    /// Writes a line to the console with a pretext based on the output type.
    /// </summary>
    /// <param name="output">Output</param>
    /// <param name="outputType">Output type, default is info</param>
    /// <param name="exception">Exception, default is null</param>
    /// <param name="currentStep">Current step, default is none</param>
    /// <typeparam name="T">Any type</typeparam>
    /// <returns>The length of the pretext</returns>
    public static (int length, string output) WriteLineWithStepPretext<T>(T output, CurrentStep currentStep = CurrentStep.None, OutputType outputType = OutputType.Info, Exception? exception = null)
    {
        var length1 = CurrentTime();
        var length2 = DetermineCurrentStep(currentStep);
        var length3 = DetermineOutputType(outputType);
        if (output is IEnumerable enumerable && !(output is string))
        {
            Console.WriteLine(string.Join(", ", enumerable.Cast<object>()));
        }
        else
        {
            Console.WriteLine(output);
        }
        var constructedReturn = (length1.length + length2.length + length3.length, length1.pretext + length2.pretext + length3.pretext + output);
        
        if (exception == null) return constructedReturn;
        ExceptionOccurred = true;
        Exceptions.Add(exception);
        Console.WriteLine($"Exception: {exception.Message}");
        Console.WriteLine($"Stack Trace: {exception.StackTrace}");
        constructedReturn.Item2 += $"\nException: {exception.Message}\nStack Trace: {exception.StackTrace}";
        return constructedReturn;
    }

    /// <summary>
    /// Writes a single line to the console with a pretext based on the output type.
    /// </summary>
    /// <param name="output">Output</param>
    /// <param name="outputType">Output type, default is info</param>
    /// <param name="exception">Exception, default is null</param>
    /// <typeparam name="T">Any type</typeparam>
    /// <returns>The length of the pretext</returns>
    public static (int length, string ouput) WriteWithPretext<T>(T output, OutputType outputType = OutputType.Info, Exception? exception = null)
    {
        var length1 = CurrentTime();
        var length2 = DetermineOutputType(outputType);
        if (output is IEnumerable enumerable && !(output is string))
        {
            Console.WriteLine(string.Join(", ", enumerable.Cast<object>()));
        }
        else
        {
            Console.Write(output);
        }
        var constructedReturn = (length1.length + length2.length, length1.pretext + length2.pretext + output);
        
        if (exception == null) return constructedReturn;
        ExceptionOccurred = true;
        Exceptions.Add(exception);
        Console.WriteLine($"Exception: {exception.Message}");
        Console.WriteLine($"Stack Trace: {exception.StackTrace}");
        constructedReturn.Item2 += $"\nException: {exception.Message}\nStack Trace: {exception.StackTrace}";
        return constructedReturn;
    }

    /// <summary>
    /// Determines the output type and returns the length of the pretext.
    /// </summary>
    /// <param name="outputType">Output type</param>
    /// <returns>The length of the pretext</returns>
    private static (int length, string pretext) DetermineOutputType(OutputType outputType)
    {
        return outputType switch
        {
            OutputType.Error => CreateOutputType(nameof(OutputType.Error), ConsoleColor.DarkRed),
            OutputType.Info => CreateOutputType(nameof(OutputType.Info), ConsoleColor.Green),
            OutputType.Warning => CreateOutputType(nameof(OutputType.Info), ConsoleColor.DarkYellow),
            OutputType.Question => CreateOutputType(nameof(OutputType.Info), ConsoleColor.DarkGreen),
            _ => (0, String.Empty)
        };
    }

    /// <summary>
    /// Creates the pretext for the output type.
    /// </summary>
    /// <param name="outputType">Output type</param>
    /// <param name="consoleColor">Console color to use for the pretext</param>
    /// <returns>The length of the pretext</returns>
    private static (int length, string pretext) CreateOutputType(string outputType, ConsoleColor consoleColor)
    {
        var oldColor = Console.ForegroundColor;
        Console.ForegroundColor = consoleColor;
        Console.Write($"[{outputType}] ");
        Console.ForegroundColor = oldColor;
        return (outputType.Length, $"[{outputType}] ");
    }

    private static (int length, string pretext) CurrentTime()
    {
        var dateTime = DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss");
        var oldColor = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.White;
        Console.Write($"[{dateTime}] ");
        Console.ForegroundColor = oldColor;
        return (dateTime.Length + 3, $"[{dateTime}] ");
    }

    /// <summary>
    /// Determines the current step and returns the length of the pretext.
    /// </summary>
    /// <param name="currentStep">Current step</param>
    /// <returns>The length of the pretext</returns>
    private static (int length, string pretext) DetermineCurrentStep(CurrentStep currentStep)
    {
        return currentStep switch
        {
            CurrentStep.SelectingFiles => CreateCurrentStep("Selecting Files"),
            CurrentStep.ExtractingFiles => CreateCurrentStep("Extracting Files"),
            CurrentStep.MergingFiles => CreateCurrentStep("Merging Files"),
            CurrentStep.CompressingFiles => CreateCurrentStep("Compressing Files"),
            CurrentStep.Main => CreateCurrentStep("Main"),
            _ => (0, String.Empty)
        };
    }

    /// <summary>
    /// Creates the pretext for the current step.
    /// </summary>
    /// <param name="currentStep">Current step</param>
    /// <returns>The length of the pretext</returns>
    private static (int length, string pretext) CreateCurrentStep(string currentStep)
    {
        Console.Write($"[{currentStep}] ");
        return (currentStep.Length, $"[{currentStep}] ");
    }
}
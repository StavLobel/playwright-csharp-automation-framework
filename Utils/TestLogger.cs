using Spectre.Console;

namespace PlaywrightAutomation.Utils;

public static class TestLogger
{
    private static readonly object _lock = new();
    private static int _totalTests = 0;
    private static int _passedTests = 0;
    private static int _failedTests = 0;
    private static DateTime _suiteStartTime;

    public static void InitializeSuite()
    {
        lock (_lock)
        {
            _totalTests = 0;
            _passedTests = 0;
            _failedTests = 0;
            _suiteStartTime = DateTime.Now;
        }

        AnsiConsole.Clear();
        var rule = new Rule("[bold cyan]Playwright Automation Test Suite[/]")
        {
            Justification = Justify.Center
        };
        AnsiConsole.Write(rule);
        AnsiConsole.WriteLine();
    }

    public static void Info(string message)
    {
        lock (_lock)
        {
            AnsiConsole.MarkupLine($"[grey]    ├─ {Markup.Escape(message)}[/]");
        }
    }

    public static void Success(string message)
    {
        lock (_lock)
        {
            AnsiConsole.MarkupLine($"[green]    ├─ {Markup.Escape(message)} ✓[/]");
        }
    }

    public static void Error(string message)
    {
        lock (_lock)
        {
            AnsiConsole.MarkupLine($"[red]    ├─ {Markup.Escape(message)} ✗[/]");
        }
    }

    public static void Warning(string message)
    {
        lock (_lock)
        {
            AnsiConsole.MarkupLine($"[yellow]    ├─ {Markup.Escape(message)} ⚠[/]");
        }
    }

    public static void TestStart(string testName)
    {
        lock (_lock)
        {
            _totalTests++;
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine($"[bold blue][[▶]] Running:[/] [white]{Markup.Escape(testName)}[/]");
        }
    }

    public static void TestEnd(string testName, bool passed)
    {
        lock (_lock)
        {
            if (passed)
            {
                _passedTests++;
                AnsiConsole.MarkupLine($"[bold green][[✓]] PASSED[/]");
            }
            else
            {
                _failedTests++;
                AnsiConsole.MarkupLine($"[bold red][[✗]] FAILED[/]");
            }
        }
    }

    public static void TestDetail(string detail)
    {
        lock (_lock)
        {
            AnsiConsole.MarkupLine($"[grey]    └─ {Markup.Escape(detail)}[/]");
        }
    }

    public static void DisplaySummary()
    {
        lock (_lock)
        {
            var totalDuration = (DateTime.Now - _suiteStartTime).TotalSeconds;
            var successRate = _totalTests > 0 ? (_passedTests * 100.0 / _totalTests) : 0;

            AnsiConsole.WriteLine();
            AnsiConsole.WriteLine();

            // Create summary panel
            var panel = new Panel(
                new Table()
                    .Border(TableBorder.Rounded)
                    .BorderColor(Color.Cyan1)
                    .AddColumn(new TableColumn("[bold]Metric[/]").LeftAligned())
                    .AddColumn(new TableColumn("[bold]Value[/]").RightAligned())
                    .AddRow("[white]Total Tests[/]", $"[bold]{_totalTests}[/]")
                    .AddRow("[green]Passed[/]", $"[bold green]{_passedTests} ✓[/]")
                    .AddRow("[red]Failed[/]", $"[bold red]{_failedTests}[/]")
                    .AddRow("[grey]Skipped[/]", $"[grey]0[/]")
                    .AddRow("[cyan]Success Rate[/]", $"[bold cyan]{successRate:F1}%[/]")
                    .AddRow("[yellow]Duration[/]", $"[bold]{totalDuration:F2}s[/]")
                    .AddRow("[grey]Start Time[/]", $"[grey]{_suiteStartTime:HH:mm:ss}[/]")
                    .AddRow("[grey]End Time[/]", $"[grey]{DateTime.Now:HH:mm:ss}[/]")
            )
            {
                Header = new PanelHeader("[bold cyan]Test Execution Summary[/]", Justify.Center),
                Border = BoxBorder.Double,
                BorderStyle = new Style(Color.Cyan1),
                Padding = new Padding(2, 1)
            };

            AnsiConsole.Write(panel);

            // Display status message
            if (_failedTests == 0)
            {
                AnsiConsole.WriteLine();
                AnsiConsole.MarkupLine("[bold green]✓ All tests passed successfully![/]");
            }
            else
            {
                AnsiConsole.WriteLine();
                AnsiConsole.MarkupLine($"[bold red]✗ {_failedTests} test(s) failed. Check the logs for details.[/]");
            }

            // Display report location
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[grey]Reports generated in:[/] [cyan]./Reports[/]");
            AnsiConsole.MarkupLine("[grey]HTML Report:[/] [cyan]./Reports/playwright-report/index.html[/]");
            AnsiConsole.WriteLine();
        }
    }

    /// <summary>
    /// Get current test statistics
    /// </summary>
    public static (int total, int passed, int failed, double duration) GetStatistics()
    {
        lock (_lock)
        {
            var duration = (DateTime.Now - _suiteStartTime).TotalSeconds;
            return (_totalTests, _passedTests, _failedTests, duration);
        }
    }

    /// <summary>
    /// Display a detailed breakdown table
    /// </summary>
    public static void DisplayDetailedBreakdown(Dictionary<string, (bool passed, double duration)> testResults)
    {
        lock (_lock)
        {
            AnsiConsole.WriteLine();
            var table = new Table()
                .Border(TableBorder.Rounded)
                .BorderColor(Color.Grey)
                .AddColumn(new TableColumn("[bold]Test Name[/]").LeftAligned())
                .AddColumn(new TableColumn("[bold]Status[/]").Centered())
                .AddColumn(new TableColumn("[bold]Duration[/]").RightAligned());

            foreach (var (testName, (passed, duration)) in testResults)
            {
                var status = passed ? "[green]✓ PASSED[/]" : "[red]✗ FAILED[/]";
                var durationStr = $"[grey]{duration:F2}s[/]";
                table.AddRow(Markup.Escape(testName), status, durationStr);
            }

            var panel = new Panel(table)
            {
                Header = new PanelHeader("[bold yellow]Test Results Breakdown[/]", Justify.Center),
                Border = BoxBorder.Rounded,
                BorderStyle = new Style(Color.Yellow),
                Padding = new Padding(1, 0)
            };

            AnsiConsole.Write(panel);
        }
    }

    public static void Progress(string message, Action action)
    {
        AnsiConsole.Status()
            .Spinner(Spinner.Known.Dots)
            .SpinnerStyle(Style.Parse("cyan"))
            .Start(message, ctx =>
            {
                action();
            });
    }

    public static async Task ProgressAsync(string message, Func<Task> action)
    {
        await AnsiConsole.Status()
            .Spinner(Spinner.Known.Dots)
            .SpinnerStyle(Style.Parse("cyan"))
            .StartAsync(message, async ctx =>
            {
                await action();
            });
    }

    public static void Section(string title)
    {
        lock (_lock)
        {
            AnsiConsole.WriteLine();
            AnsiConsole.Write(new Rule($"[yellow]{Markup.Escape(title)}[/]")
            {
                Justification = Justify.Left
            });
        }
    }
}

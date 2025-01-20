#nullable enable

using System;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;

namespace OpenSense.WPF {
    public static class Program {

        private const int AttachParentProcess = -1;

        [DllImport("kernel32.dll")]
        private static extern bool AttachConsole(int dwProcessId);

        public static int Main(string[] args) {
            AttachConsole(AttachParentProcess);//Do not launch a new process by default

            Log.Logger = new LoggerConfiguration()
                .WriteTo.Debug()
                .WriteTo.Console(theme: SystemConsoleTheme.Colored)
                .CreateLogger();

            var exitCode = 0;
            try {
                var runOption = new Option<FileInfo[]>(
                    ["--run", "-r"],
                    description: "The configuration files to run at start up.",
                    parseArgument: result => {
                        var files = result.Tokens.Select(token => new FileInfo(token.Value)).ToArray();
                        var anyNotExist = files.Any(file => !file.Exists);
                        if (anyNotExist) {
                            result.ErrorMessage = "One or more configuration files do not exist.";
                            return Array.Empty<FileInfo>();
                        }
                        return files;
                    }
                ) {
                };

                var rootCommand = new RootCommand(
                    "OpenSense Windows Presentation Foundation (WPF) Application."
                ) {
                    runOption,
                };

                rootCommand.SetHandler(context => {
                    var cancellationToken = context.GetCancellationToken();
                    //Nothing
                });

                var parser = new CommandLineBuilder(rootCommand)
                    .UseDefaults()
                    .Build();
                var parseResult = parser.Parse(args);
                exitCode = parseResult.Invoke();
                var helpOption = rootCommand.Options.Where(o => o.Name == "help").Single();//The HelpOption class is internal. The longest alias becomes the default name.
                var versionOption = rootCommand.Options.Where(o => o.Name == "version").Single();
                var doNotLaunchApp = parseResult.HasOption(helpOption) || parseResult.HasOption(versionOption);
                if (!doNotLaunchApp && exitCode == 0) {
                    var runConfigurations = parseResult.GetValueForOption(runOption);
                    Trace.Assert(runConfigurations is not null);
                    var staThread = new Thread(() => {
                        var app = new App(runConfigurations);
                        app.InitializeComponent();
                        exitCode = app.Run();
                    });
                    staThread.SetApartmentState(ApartmentState.STA);
                    staThread.Start();
                    staThread.Join();
                }
            } catch (Exception ex) {
                Log.Fatal(ex, "Application exited unexpectedly.");
                exitCode = 1;
            } finally {
                Log.CloseAndFlush();
            }
            return exitCode;
        }
    }

}

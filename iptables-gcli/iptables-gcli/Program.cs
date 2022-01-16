using System;
using Terminal.Gui;

namespace iptables_gcli
{
    class Program
    {
		static void Main(string[] args)
		{
            try {
                // Handle command line arguments
                if (args.Length > 0) {
                    if (args[0] == "--help" || args[0] == "-h") {
                        Console.WriteLine("Usage: iptables-gcli [OPTION]");
                        Console.WriteLine("");
                        Console.WriteLine("Options:");
                        Console.WriteLine("  --help, -h                 Show this help message and exit");
                        Console.WriteLine("  --version, -v              Show version information and exit");
                        Console.WriteLine("  --table, -t              Set the table to operate on. Default: filter");
                        return;
                    }
                    else if (args[0] == "--version" || args[0] == "-v") {
                        Console.WriteLine("iptables-gcli version 0.1");
                        Console.WriteLine("Copyright (C) 2021  iptables-gcli contributors");
                        Console.WriteLine("This is free software; see the source for copying conditions."); //TODO: Add license
                        return;
                    }
                    else if (args[0] == "--table" || args[0] == "-t") {
                        if (args.Length > 1) {
                            ProgramState.Table = args[1];
                        }
                        else {
                            Console.WriteLine("Error: --table requires an argument");
                            return;
                        }
                    }
                    else {
                        Console.WriteLine("iptables-gcli: unrecognized option '{0}'", args[0]);
                        Console.WriteLine("Try 'iptables-gcli --help' for more information.");
                        return;
                    }
                }

                // Initialize UI (UI/RulesListWindow.cs)
                Application.Init();
                var top = Application.Top;

                var win = new Window("IPTables-GCLI")
                {
                    X = 0,
                    Y = 1, // Leave one row for the toplevel menu

                    // By using Dim.Fill(), it will automatically resize without manual intervention
                    Width = Dim.Fill(),
                    Height = Dim.Fill()
                };
                var rulesListWindow = new RulesListWindow($"Rules for table \"{ProgramState.Table}\"");
                win.Add(rulesListWindow);
                top.Add(win);

                Application.Run(top);
            } finally
            {
                Application.Shutdown();
            }
		}
	}
}
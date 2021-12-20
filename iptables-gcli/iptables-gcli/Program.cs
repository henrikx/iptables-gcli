using System;
using Terminal.Gui;

namespace iptables_gcli
{
    class Program
    {
		static void Main(string[] args)
		{
            try {
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
                var rulesListWindow = new RulesListWindow("Rules");
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
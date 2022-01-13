using System;

namespace iptables_gcli
{
    /// <summary>
    /// Information that is used throughout the program such as the table to operate on.
    /// </summary>
    class ProgramState
    {
        public static string Table { get; set; } = "filter";
    }
}
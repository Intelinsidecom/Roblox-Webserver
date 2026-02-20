using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Windows;

namespace Control_Panel
{
    /// <summary>
    /// Manages multiple console window instances with reserved names
    /// </summary>
    public static class ConsoleWindowManager
    {
        private static readonly ConcurrentDictionary<string, ConsoleWindow> _reservedConsoles = new ConcurrentDictionary<string, ConsoleWindow>();
        private static readonly ConcurrentDictionary<string, SqlConsoleWindow> _reservedSqlConsoles = new ConcurrentDictionary<string, SqlConsoleWindow>();
        private static readonly object _globalLock = new object();
        private static ConsoleWindow _globalConsole;

        /// <summary>
        /// Gets the global console instance (for general use)
        /// </summary>
        public static ConsoleWindow GlobalConsole
        {
            get
            {
                lock (_globalLock)
                {
                    if (_globalConsole == null || !_globalConsole.IsLoaded)
                    {
                        _globalConsole = new ConsoleWindow("Global Console");
                    }
                    return _globalConsole;
                }
            }
        }

        /// <summary>
        /// Gets or creates a reserved console with a specific name
        /// </summary>
        /// <param name="consoleName">The unique name for this console</param>
        /// <returns>A console window reserved for this specific purpose</returns>
        public static ConsoleWindow GetReservedConsole(string consoleName)
        {
            if (consoleName == null || consoleName.Trim() == "")
                throw new ArgumentException("Console name cannot be null or empty", "consoleName");

            return _reservedConsoles.GetOrAdd(consoleName, delegate(string name) { return new ConsoleWindow(name); });
        }

        /// <summary>
        /// Gets or creates a reserved SQL console with a specific name
        /// </summary>
        /// <param name="consoleName">The unique name for this SQL console</param>
        /// <returns>A SQL console window reserved for this specific purpose</returns>
        public static SqlConsoleWindow GetReservedSqlConsole(string consoleName)
        {
            if (consoleName == null || consoleName.Trim() == "")
                throw new ArgumentException("Console name cannot be null or empty", "consoleName");
            SqlConsoleWindow existingConsole;
            if (_reservedSqlConsoles.TryGetValue(consoleName, out existingConsole))
            {
                try
                {
                    if (!existingConsole.IsLoaded)
                    {
                        _reservedSqlConsoles.TryRemove(consoleName, out existingConsole);
                    }
                }
                catch
                {
                    _reservedSqlConsoles.TryRemove(consoleName, out existingConsole);
                }
            }

            return _reservedSqlConsoles.GetOrAdd(consoleName, delegate(string name) { return new SqlConsoleWindow(name); });
        }

        /// <summary>
        /// Closes and removes a reserved console
        /// </summary>
        /// <param name="consoleName">The name of the console to close</param>
        public static void CloseReservedConsole(string consoleName)
        {
            ConsoleWindow console;
            if (_reservedConsoles.TryRemove(consoleName, out console))
            {
                console.Dispatcher.Invoke(() => console.Close());
            }
        }

        /// <summary>
        /// Closes and removes a reserved SQL console
        /// </summary>
        /// <param name="consoleName">The name of the SQL console to close</param>
        public static void CloseReservedSqlConsole(string consoleName)
        {
            SqlConsoleWindow console;
            if (_reservedSqlConsoles.TryRemove(consoleName, out console))
            {
                console.Dispatcher.Invoke(() => console.Close());
            }
        }

        /// <summary>
        /// Gets all currently active reserved console names
        /// </summary>
        public static IEnumerable<string> GetActiveReservedConsoles()
        {
            return _reservedConsoles.Keys.ToArray();
        }

        /// <summary>
        /// Gets all currently active reserved SQL console names
        /// </summary>
        public static IEnumerable<string> GetActiveReservedSqlConsoles()
        {
            return _reservedSqlConsoles.Keys.ToArray();
        }

        /// <summary>
        /// Checks if a reserved console exists and is active
        /// </summary>
        /// <param name="consoleName">The console name to check</param>
        public static bool IsReservedConsoleActive(string consoleName)
        {
            ConsoleWindow console;
            return _reservedConsoles.TryGetValue(consoleName, out console) && console.IsLoaded;
        }

        /// <summary>
        /// Checks if a reserved SQL console exists and is active
        /// </summary>
        /// <param name="consoleName">The SQL console name to check</param>
        public static bool IsReservedSqlConsoleActive(string consoleName)
        {
            SqlConsoleWindow console;
            return _reservedSqlConsoles.TryGetValue(consoleName, out console) && console.IsLoaded;
        }

        /// <summary>
        /// Shows a reserved console and brings it to front
        /// </summary>
        /// <param name="consoleName">The name of the console to show</param>
        public static void ShowReservedConsole(string consoleName)
        {
            ConsoleWindow console;
            if (_reservedConsoles.TryGetValue(consoleName, out console))
            {
                console.Dispatcher.Invoke(() =>
                {
                    console.Show();
                    console.WindowState = WindowState.Normal;
                    console.Activate();
                });
            }
        }

        /// <summary>
        /// Shows a reserved SQL console and brings it to front
        /// </summary>
        /// <param name="consoleName">The name of the SQL console to show</param>
        public static void ShowReservedSqlConsole(string consoleName)
        {
            SqlConsoleWindow console;
            if (_reservedSqlConsoles.TryGetValue(consoleName, out console))
            {
                console.Dispatcher.Invoke(() =>
                {
                    console.Show();
                    console.WindowState = WindowState.Normal;
                    console.Activate();
                });
            }
        }

        /// <summary>
        /// Clears all output from a reserved console
        /// </summary>
        /// <param name="consoleName">The name of the console to clear</param>
        public static void ClearReservedConsole(string consoleName)
        {
            ConsoleWindow console;
            if (_reservedConsoles.TryGetValue(consoleName, out console))
            {
                console.Dispatcher.Invoke(() => console.ClearOutput());
            }
        }

        /// <summary>
        /// Clears all output from a reserved SQL console
        /// </summary>
        /// <param name="consoleName">The name of the SQL console to clear</param>
        public static void ClearReservedSqlConsole(string consoleName)
        {
            SqlConsoleWindow console;
            if (_reservedSqlConsoles.TryGetValue(consoleName, out console))
            {
                console.Dispatcher.Invoke(() => console.ClearOutput());
            }
        }

        /// <summary>
        /// Closes all reserved consoles
        /// </summary>
        public static void CloseAllReservedConsoles()
        {
            var consoleNames = _reservedConsoles.Keys.ToArray();
            foreach (var name in consoleNames)
            {
                CloseReservedConsole(name);
            }
        }

        /// <summary>
        /// Closes all reserved SQL consoles
        /// </summary>
        public static void CloseAllReservedSqlConsoles()
        {
            var consoleNames = _reservedSqlConsoles.Keys.ToArray();
            foreach (var name in consoleNames)
            {
                CloseReservedSqlConsole(name);
            }
        }
    }
}

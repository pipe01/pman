using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PMan
{
    class Program
    {
        private delegate bool CommandDelegate(Arguments args);
        private static Dictionary<string, CommandDelegate> cmds = new Dictionary<string, CommandDelegate>();

        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                PrintUsage();
                return;
            }
            string cmd = args[0];
            string[] postargs = args.Skip(1).ToArray();

            cmds.Add("pkill", CmdPKill);
            cmds.Add("plist", CmdPList);
            cmds.Add("winlist", CmdWinList);
            cmds.Add("wininfo", CmdWinInfo);
            cmds.Add("win", CmdWin);
            cmds.Add("start", CmdStart);

            if (!ExecuteCmd(cmd, new Arguments(postargs)))
            {
                PrintError("Invalid command.");
                PrintUsage();
            }
        }

        private static bool CmdWin(Arguments args)
        {
            //if (args.StrCount < 1) return false;
            string cmd = args[0];

            switch (cmd)
            {
                case "close":
                    OpenWindowGetter.CloseWindow(GetWindowFromString(args[1]));
                    break;
                case "min":
                    OpenWindowGetter.ShowWindow(GetWindowFromString(args[1]), OpenWindowGetter.ShowWindowCommands.Minimize);
                    break;
                default:
                    break;
            }

            return true;
        }

        private static bool CmdStart(Arguments args)
        {
            if (args.StrCount < 1) return false;

            ProcessStartInfo psi = new ProcessStartInfo();
            psi.FileName = args[0];
            psi.WindowStyle = args.IsOptionSet("H") ? ProcessWindowStyle.Hidden : ProcessWindowStyle.Normal;

            if (args.StrCount > 1)
            {
                foreach (var item in args.Strings.Skip(1))
                {
                    psi.Arguments += item + " ";
                }
            }

            Process p = new Process();
            p.StartInfo = psi;

            try
            {
                p.Start();
            }
            catch (Win32Exception)
            {
                PrintError("No executable file found by \"{0}\".", args[0]);
            }

            return true;
        }

        #region Commands
        private static bool ExecuteCmd(string cmd, Arguments args)
        {
            bool e = cmds.ContainsKey(cmd);
            if (e)
                e = cmds[cmd].Invoke(args);
            return e;
        }

        #region Window man
        private static bool CmdWinList(Arguments args)
        {
            var wins = OpenWindowGetter.GetOpenWindows();

            foreach (var item in wins)
            {
                if (args.IsOptionSet("N"))
                {
                    Console.WriteLine(item.Value);
                }
                else if (args.IsOptionSet("H"))
                {
                    Console.WriteLine(item.Key);
                }
                else
                {
                    Console.WriteLine("{0} - {1}", item.Key, item.Value);
                }
            }

            return true;
        }

        private static bool CmdWinInfo(Arguments args)
        {
            if (args.StrCount == 1)
            {
                var wins = OpenWindowGetter.GetOpenWindows();
                var hwnd = GetWindowFromString(args[0]);
                if (hwnd != IntPtr.Zero)
                {
                    var pl = Process.GetProcesses();
                    uint pid = OpenWindowGetter.GetPIDFromWindow(hwnd);
                    Process process = null;
                    Array.ForEach(pl, (o) =>
                    {
                        if (o.Id == pid && process == null)
                        {
                            process = o;
                        }
                    });

                    if (args.IsOptionSet("B"))
                    {
                        Console.WriteLine(OpenWindowGetter.GetCaptionOfWindow(hwnd));
                        Console.WriteLine(OpenWindowGetter.GetClassNameOfWindow(hwnd));
                        Console.WriteLine(hwnd.ToString());
                        Console.WriteLine(pid);
                        Console.WriteLine(process.ProcessName);
                    }
                    else
                    {
                        Console.WriteLine("Window title: {0}", OpenWindowGetter.GetCaptionOfWindow(hwnd));
                        Console.WriteLine("Window class: {0}", OpenWindowGetter.GetClassNameOfWindow(hwnd));
                        Console.WriteLine("Window HWND: {0}", hwnd.ToString());
                        Console.WriteLine("Process ID: {0}", pid);
                        Console.WriteLine("Process name: {0}", process.ProcessName);
                    }
                }
                else
                {
                    PrintError("Couldn't find the specified window");
                }
                //}// else if (args.IsOptionSet())
            }

            return true;
        }
        #endregion

        #region Process man
        private static bool CmdPList(Arguments args)
        {
            var pl = Process.GetProcesses();
            List<Process> pocl = pl.ToList();
            if (args.IsOptionSet("I"))
                pocl.Sort((a, b) => (a.Id.CompareTo(b.Id)));
            pl = pocl.ToArray();

            List<string> names = new List<string>();
            foreach (var item in pl)
            {
                string print;
                if (args.IsOptionSet("N"))
                {
                    print = item.ProcessName;
                }
                else if (args.IsOptionSet("P"))
                {
                    print = item.Id.ToString();
                }
                else
                {
                    print = item.ProcessName + "-" + item.Id;
                }
                names.Add(print);
            }
            if (args.IsOptionSet("A")) names.Sort();
            foreach (var item in names)
            {
                Console.WriteLine(item);
            }

            return true;
        }

        private static bool CmdPKill(Arguments args)
        {
            int pid = 0;
            if (args.StrCount > 0)
            {
                if (int.TryParse(args[0], out pid)) //PID specified
                {
                    TryKillProcess(pid);
                }
                else //PID not specified
                {
                    bool all = args.IsOptionSet("A");
                    var ps = Process.GetProcessesByName(args[0]); //Try to get all processes with name args[0]
                    if (ps.Count() == 1) //If there's only one process, kill it
                    {
                        ps[0].Kill();
                        
                    }
                    else if (ps.Count() > 1) //If there are more, check for /A
                    {
                        if (all) //If it's on, kill them all
                        {
                            Array.ForEach(ps, (o) => TryKillProcess(o));
                        }
                        else //If not, error
                        {
                            PrintError("There is more than one process by the name \"{0}\"", args[0]);
                            Console.WriteLine("Run the program with the /A option to kill al processes");
                        }
                    }
                }
            }
            return false;
        }
        #endregion
        #endregion

        static IntPtr GetWindowFromString(string str)
        {
            int num = 0;
            var wins = OpenWindowGetter.GetOpenWindows();

            if (int.TryParse(str, out num))
            {
                foreach (var item in wins)
                {
                    var pid = OpenWindowGetter.GetPIDFromWindow(item.Key);
                    if (pid == num)
                    {
                        return item.Key;
                    }
                }
                return new IntPtr(num);
            }
            else
            {
                var ret = wins.Where((o) => o.Value == str);
                if (ret.Any())
                    return ret.Single().Key;
                else
                    return IntPtr.Zero;
            }
        }

        static void PrintError(string errmsg, params object[] args)
        {
            var c = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("Error: ");
            Console.ForegroundColor = c;
            Console.WriteLine(errmsg, args);
        }
        
        static bool TryKillProcess(Process p)
        {
            try
            {
                p.Kill();
            }
            catch (Win32Exception)
            {
                PrintError("Access denied.");
                return false;
            }
            return true;
        }

        static bool TryKillProcess(int pid)
        {
            try
            {
                Process p = Process.GetProcessById(pid);
                TryKillProcess(p);
            }
            catch (ArgumentException)
            {
                PrintError("No process is running with the PID {0}.", pid);
                return false;
            }
            return true;
        }

        static void PrintUsage()
        {
            Console.WriteLine("PMan by pipe01");
            Console.WriteLine("Usage: pman <command> <argument1> <argument2> ...");
            Console.WriteLine("Available commands:");
            Console.WriteLine("  pkill <pid>           Kill the process with the specified process ID");
            Console.WriteLine("  pkill <name>          Kill one or more processes with the specified name");
            Console.WriteLine("    /A                    If there are more than one, kill them all");
            Console.WriteLine("  plist                 List all running processes");
            Console.WriteLine("    /A                    Order by alphabetical order");
            Console.WriteLine("    /N                    Only print process name");
            Console.WriteLine("    /P                    Only print process ID");
            Console.WriteLine("  winlist               List all open windows");
            Console.WriteLine("    /N                    Only print the window name");
            Console.WriteLine("    /H                    Only print the window HWND");
            Console.WriteLine("  wininfo <window>      Get info from the window");
            Console.WriteLine("    /B                    Don't over-verbose (batch mode)");
            Console.WriteLine("  win close <window>    Close a window");
            Console.WriteLine("  win minimize <window> Minimize a window");
            Console.WriteLine("  start <name>          Execute the specified file");
            Console.WriteLine("    /H                    Start window hidden");
            Console.WriteLine();

            Console.Write("You can replace <window> with either any window title, any window HWND (provided by the winlist command), or any process ID");
        }
    }

    public struct Arguments
    {
        public string[] Options;
        public string[] Strings;

        public string this[int i]
        {
            get { return Strings[i]; }
            set { Strings[i] = value; }
        }

        public int StrCount { get { return Strings.Length; } }
        public int OptCount { get { return Options.Length; } }

        public Arguments(string[] args)
        {
            string[] opt = args.TakeWhile((o) => o.StartsWith("/")).ToArray();
            string[] str = args.Skip(opt.Length).ToArray();
            this.Options = opt;
            this.Strings = str;
        }

        public bool IsOptionSet(string option, bool forcecase = false)
        {
            bool c = Options.Contains("/" + option);
            if (!c && !forcecase)
            {
                c = Options.Contains("/" + option.ToLower());
            }
            return c;
        }
    }
}

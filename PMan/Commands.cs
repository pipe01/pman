using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/*namespace PMan.Commands
{
    #region Base
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

    public struct DefArgument
    {
        public string Name, Usage;

        public DefArgument(string name, string usage)
        {
            this.Name = name;
            this.Usage = usage;
        }
    }

    public struct Usage
    {
        public DefArgument[] Arguments;
        public string Description;

        public Usage(string desc, params DefArgument[] args)
        {
            this.Arguments = args;
            this.Description = desc;
        }
    }

    public abstract class Command
    {
        public static List<Command> GetCommands()
        {
            return new Command[]
            {
                new CmdPList()
            }
            .ToList();
        }

        public abstract string GetName();
        public abstract Usage[] GetUsages();
        public abstract CommandOption[] GetPossibleOptions();
        public abstract bool Execute(Arguments args);
    }

    public struct CommandOption
    {
        public string Letter;
        public string Usage;

        public CommandOption(string l, string u)
        {
            this.Letter = l;
            this.Usage = u;
        }
    }
    #endregion

    #region Commands
    public class CmdPList : Command
    {
        public override bool Execute(Arguments args)
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

        public override string GetName()
        {
            return "plist";
        }

        public override CommandOption[] GetPossibleOptions()
        {
            return new[] {
                new CommandOption("A", "Order alphabetically"),
                new CommandOption("N", "Only print process name"),
                new CommandOption("P", "Only print process ID")
            };
        }

        public override Usage[] GetUsages()
        {
            return new[] { new Usage("List all running processes") };
        }
    }

    public class CmdPKill : Command
    {
        public override bool Execute(Arguments args)
        {
            int pid = 0;
            if (args.StrCount > 0)
            {
                if (int.TryParse(args[0], out pid)) //PID specified
                {
                    Program.TryKillProcess(pid);
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
                            Array.ForEach(ps, (o) => Program.TryKillProcess(o));
                        }
                        else //If not, error
                        {
                            Program.PrintError("There is more than one process by the name \"{0}\"", args[0]);
                            Console.WriteLine("Run the program with the /A option to kill all processes");
                        }
                    }
                }
            }
            return false;
        }

        public override string GetName()
        {
            return "pkill";
        }

        public override CommandOption[] GetPossibleOptions()
        {
            return new[] { new CommandOption("A", "If there are more than one, kill them all") };
        }

        public override Usage[] GetUsages()
        {
            return new[]
            {
                new Usage("Kill the process with the specified process ID", "pid"),
                new Usage("Kill one or more processes with the specified name", "name")
            };
        }
    }

    public class CmdWinList : Command
    {
        public override bool Execute(Arguments args)
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

        public override string GetName()
        {
            return "winlist";
        }

        public override CommandOption[] GetPossibleOptions()
        {
            return new[]
            {
                new CommandOption("N", "Only print the window name"),
                new CommandOption("H", "Only print the window HWND")
            };
        }

        public override Usage[] GetUsages()
        {
            return new[]
            {
                new Usage("List all open windows")
            };
        }
    }

    public class CmdWinClose : Command
    {
        public override bool Execute(Arguments args)
        {
            if (args.StrCount == 1)
            {
                OpenWindowGetter.CloseWindow(OpenWindowGetter.GetWindowFromString(args[0]));
            }
            return true;
        }

        public override string GetName()
        {
            return "winclose";
        }

        public override CommandOption[] GetPossibleOptions()
        {
            return new CommandOption[0];
        }

        public override Usage[] GetUsages()
        {
            return new[] { new Usage("winclose", "window") };
        }
    }
    #endregion
}
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace EasyHosts_CLI
{
    class Program
    {
        static string path = System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.System),
                "drivers/etc/hosts"
            );
        static System.Collections.ArrayList hostsList;

        static void Main(string[] args)
        {
            string userInput;

            if (!HasAdministratorPrivileges())
            {
                Console.WriteLine("Access denied. You need to run this program as an Administrator.");
            }

            readHostsFile();
            printHeader();

            while ((userInput = Console.ReadLine()) != "exit")
            {
                // tokens[0] is the commmand (add or remove)
                // tokens[1] to tokebs[n] are the command arguments
                string[] tokens = userInput.Split(' ');

                if (userInput.StartsWith("add"))
                {
                    processAddCommand(tokens);
                }
                else if (userInput.StartsWith("remove"))
                {
                    processRemoveCommand(tokens);
                }

                Console.Clear();
                printHeader();
            }
        }

        private static void processAddCommand(string[] tokens)
        {
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(path, true))
            {
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.Append(tokens[1]);
                stringBuilder.Append("\t");
                stringBuilder.Append(tokens[2]);
                for (var i = 3; i < tokens.Length; i++)
                {
                    stringBuilder.Append(" ");
                    stringBuilder.Append(tokens[i]);
                }

                file.WriteLine(stringBuilder.ToString());
            }

            readHostsFile();
        }

        private static void processRemoveCommand(string[] tokens)
        {
            try
            {
                int hostId = Int32.Parse(tokens[1]);
                if (hostId >= 0 && hostId < hostsList.Count)
                {
                    hostsList.RemoveAt(hostId);
                    rewriteHostsFile();
                }
                else
                {
                    Console.WriteLine("Invalid entry number.");
                    Console.ReadKey();
                }
            }
            catch (FormatException e)
            {
                Console.WriteLine(e.Message);
                Console.ReadKey();
            }
        }

        private static void printHeader()
        {
            Console.WriteLine("EasyHosts CLI Tool v1.0 - Daniel Filipe Silva");
            Console.WriteLine();
            Console.WriteLine("Current hosts file entries:");
            printHostsList();

            Console.WriteLine();
            Console.WriteLine("Available actions:");
            Console.WriteLine("\tadd [IP address] [hostname]");
            Console.WriteLine("\tremove [entry id]");
            Console.WriteLine("\texit");

            Console.Write("$: ");
        }

        private static void printHostsList()
        {
            var counter = 0;
            foreach (string hostEntry in hostsList)
            {
                Console.WriteLine(counter + ": " + hostEntry);
                counter++;
            }
        }

        private static void readHostsFile()
        {
            hostsList = new System.Collections.ArrayList();

            using (System.IO.StreamReader file = new System.IO.StreamReader(path))
            {
                string line;
                while ((line = file.ReadLine()) != null)
                {
                    if (!line.StartsWith("#") && !line.Equals(""))
                    {
                        hostsList.Add(line);
                    }
                }
            }
        }

        private static void rewriteHostsFile()
        {
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(path))
            {
                string hostFileOriginalHeader = @"# Copyright (c) 1993-2009 Microsoft Corp.
#
# This is a sample HOSTS file used by Microsoft TCP/IP for Windows.
#
# This file contains the mappings of IP addresses to host names. Each
# entry should be kept on an individual line. The IP address should
# be placed in the first column followed by the corresponding host name.
# The IP address and the host name should be separated by at least one
# space.
#
# Additionally, comments (such as these) may be inserted on individual
# lines or following the machine name denoted by a '#' symbol.
#
# For example:
#
#      102.54.94.97     rhino.acme.com          # source server
#       38.25.63.10     x.acme.com              # x client host

# localhost name resolution is handled within DNS itself.
#	127.0.0.1       localhost
#	::1             localhost";

                file.WriteLine(hostFileOriginalHeader);
                file.WriteLine();
                foreach (string hostEntry in hostsList)
                {
                    file.WriteLine(hostEntry);
                }
            }
        }

        private static bool HasAdministratorPrivileges()
        {
            WindowsIdentity id = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(id);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
    }
}

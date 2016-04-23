﻿using Microsoft.VisualStudio.Services.Agent.Util;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.VisualStudio.Services.Agent.Listener
{
    public static class Program
    {
        private static Tracing s_trace;

        public static int Main(string[] args)
        {
            return MainAsync(args).GetAwaiter().GetResult();
        }

        // Return code definition: (this will be used by service host to determine whether it will re-launch agent.listener)
        // 0: Agent exit
        // 1: Terminate failure
        // 2: Retriable failure
        // 3: Exit for self update
        public async static Task<int> MainAsync(string[] args)
        {
            // Validate the binaries intended for one OS are not running on a different OS.
            switch (Constants.Agent.Platform)
            {
                case Constants.OSPlatform.Linux:
                    if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                    {
                        Console.WriteLine(StringUtil.Loc("NotLinux"));
                        return Constants.Agent.ReturnCode.TerminatedError;
                    }
                    break;
                case Constants.OSPlatform.OSX:
                    if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                    {
                        Console.WriteLine(StringUtil.Loc("NotOSX"));
                        return Constants.Agent.ReturnCode.TerminatedError;
                    }
                    break;
                case Constants.OSPlatform.Windows:
                    if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    {
                        Console.WriteLine(StringUtil.Loc("NotWindows"));
                        return Constants.Agent.ReturnCode.TerminatedError;
                    }
                    break;
                default:
                    Console.WriteLine(StringUtil.Loc("PlatformNotSupport", RuntimeInformation.OSDescription, Constants.Agent.Platform.ToString()));
                    return Constants.Agent.ReturnCode.TerminatedError;
            }

            using (HostContext context = new HostContext("Agent"))
            using (var term = context.GetService<ITerminal>())
            {
                int rc = Constants.Agent.ReturnCode.Success;
                try
                {
                    s_trace = context.GetTrace("AgentProcess");
                    s_trace.Info($"Version: {Constants.Agent.Version}");
                    s_trace.Info($"Commit: {BuildConstants.Source.CommitHash}");

                    //
                    // TODO (bryanmac): Need VsoAgent.exe compat shim for SCM
                    //                  That shim will also provide a compat arg parse 
                    //                  and translate / to -- etc...
                    //

                    // Parse the command line args.
                    var command = new CommandSettings(context, args);
                    s_trace.Info("Arguments parsed");

                    // Defer to the Agent class to execute the command.
                    IAgent agent = context.GetService<IAgent>();
                    using (agent.TokenSource = new CancellationTokenSource())
                    {
                        rc = await agent.ExecuteCommand(command);
                    }
                }
                catch (Exception e)
                {
                    if (!(e is OperationCanceledException))
                    {
                        Console.Error.WriteLine(StringUtil.Format("An error occured.  {0}", e.Message));
                    }

                    s_trace.Error(e);
                    rc = 1;
                }

                return rc;
            }
        }
    }
}

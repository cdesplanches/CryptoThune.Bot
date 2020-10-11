using System;
using System.IO;
using CommandLine;
using CryptoThune.Net;
using CryptoThune.Strategy;
using NLog;


namespace CryptoThune.Bot
{
    /// <summary>
    /// Main
    /// </summary>
    class Program
    {
        /// <summary>
        /// NLogger
        /// </summary>
        /// <returns></returns>
        private static readonly Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        /// <summary>
        /// Options that can be passed to the program
        /// </summary>
        public class Options
        {
            /// <summary>
            /// Set the verbosity.
            /// </summary>
            /// <value></value>
            [Option('v', "verbose", Required = false, HelpText = "Set output to verbose messages.")]
            public bool Verbose { get; set; }
            /// <summary>
            /// Do a simulation
            /// </summary>
            /// <value></value>
            [Option('s', "sim", Required = false, HelpText = "Launch a simulation.")]
            public bool Simulate { get; set; }
            /// <summary>
            /// Make a dry run. Same function, not actions
            /// </summary>
            /// <value></value>            
            [Option('d', "dry", Default = false, Required = false, HelpText = "Run.")]
            public bool DryRun { get; set; }
            /// <summary>
            /// The output directory
            /// </summary>
            /// <value></value>
            [Option('o', "output", Default = false, Required = false, HelpText = "Set the output directory.")]
            public string Output { get; set; }
        }
        /// <summary>
        /// The entry point
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        static int Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args)
                   .WithParsed<Options>(o =>
                   {
                        Console.WriteLine("CryptoThune.Bot v=1.0");
                        Console.WriteLine("by cdesplanches");
                        Console.WriteLine("kcdesplanches@gmail.com");

                        var config = new NLog.Config.LoggingConfiguration();

                        // Targets where to log to: Console
                        var logconsole = new NLog.Targets.ConsoleTarget("logconsole");
                        config.AddRule(LogLevel.Info, LogLevel.Fatal, logconsole);
                        NLog.LogManager.Configuration = config;

                        if (o.Verbose)
                        {
                            Console.WriteLine($"Verbosity: ON");
                        }

                        if ( !String.IsNullOrEmpty(o.Output) )
                        {
                            try
                            {
                                //Set the current directory.
                                Directory.SetCurrentDirectory(o.Output.Trim());
                            }
                            catch (DirectoryNotFoundException e)
                            {
                                Console.WriteLine("The specified directory does not exist. {0}", e);
                            }

                            Logger.Info("Output Directory is now set to: " + Directory.GetCurrentDirectory());
                        }

                        if ( o.Simulate )
                        {
                            var bot = new BotThune<ExchangeFake>();
                            bot.MarketExchange.Deposit(295.0);
                            var strategy = new ZOB(1.0, 7.0, 0.6);
                            bot.AddStrategy(strategy, "XTZEUR", 20.0 );
                            bot.AddStrategy(strategy, "XRPEUR", 80.0 );
                            bot.Sim(startDate: new DateTime(2020, 09, 07));
                        }
                        else
                        {
                             var bot = new BotThune<ExchangeKraken>();
                            var strategy = new ZOB(1.0, 7.0, 0.6);
                            bot.AddStrategy(strategy, "XTZEUR", 20.0);
                            bot.AddStrategy(strategy, "BTCEUR", 5.0);
                            bot.AddStrategy(strategy, "XRPEUR", 75.0 );
                            if ( o.DryRun )
                            {
                                bot.DryRun();
                            }
                            else    // Oh; real run
                            {
                                bot.Run();
                            }
                            
                        }


                   });


                   return 0;

        }
    }
}

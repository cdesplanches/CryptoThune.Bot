using System;
using System.Linq;
using System.IO;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;
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
            [Option('o', "output", Required = false, HelpText = "Set the output directory.")]
            public string Output { get; set; }
            /// <summary>
            /// The start date of the simulation
            /// </summary>
            /// <value></value>
            [Option('b', "begindate", Required = false, HelpText = "Set the begin date of the simulation.")]
            public DateTime? BeginDate { get; set; }
            /// <summary>
            /// The start date of the simulation
            /// </summary>
            /// <value></value>
            [Option('e', "enddate", Required = false, HelpText = "Set the end date of the simulation.")]
            public DateTime? EndDate { get; set; }
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



                        var builder = new ConfigurationBuilder()
                                    .AddJsonFile("portfolio", optional: false, reloadOnChange: true)
                                    .Build();
                        var assets = builder.GetSection("Assets")
                                .GetChildren()
                                .ToList()
                                .Select(x => new {
                                    Asset = x.GetValue<string>("Asset"),
                                    Weight = x.GetValue<double>("Weight"),
                                    Threshold = x.GetValue<double>("Threshold"),
                                    Ruptor = x.GetValue<double>("Ruptor"),
                                    Probability = x.GetValue<double>("Probability"),
                                });

                        if ( o.Simulate )
                        {
                           
                            var bot = new BotThune<ExchangeFake>();
                            bot.MarketExchange.Deposit(295.0);
                            foreach ( var ast in assets )
                            {
                                var str = new ZOB(ast.Threshold, ast.Ruptor, ast.Probability);
                                bot.AddStrategy(str, ast.Asset, ast.Weight );    
                            }                        

                            bot.Sim(startDate: o.BeginDate, endDate: o.EndDate);
                        }
                        else
                        {
                            var bot = new BotThune<ExchangeKraken>();
                            foreach ( var ast in assets )
                            {
                                var str = new ZOB(ast.Threshold, ast.Ruptor, ast.Probability);
                                bot.AddStrategy(str, ast.Asset, ast.Weight );    
                            }
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

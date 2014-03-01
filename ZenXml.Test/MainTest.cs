using System;
using NLog;
using NLog.Config;
using NLog.Targets;
using Xunit;
using ZenXml.Core;

namespace ZenXml.Test
{
    public class MainTest
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public MainTest()
        {
            ConfigureNLog();
        }

        private static void ConfigureNLog()
        {
            var consoleTarget = new ConsoleTarget();
            var loggingRule = new LoggingRule("*", LogLevel.Trace, consoleTarget);
            var loggingConfiguration = new LoggingConfiguration();
            loggingConfiguration.LoggingRules.Add(loggingRule);
            LogManager.Configuration = loggingConfiguration;
        }

        private const string TestXml = @"<?xml version=""1.0"" encoding=""utf-8"" ?>

        <Root>

            <Item1>My Item 1</Item1>
            <Item2 Attribute1=""Test"">My Item 1</Item2>
            
        
        </Root>
";
        [Fact]
        public void TestRoot()
        {
            var zenXml = ZenXmlObject.CreateFromXml(TestXml, StringComparison.OrdinalIgnoreCase);
            Logger.Info(zenXml.Root.GetType());
            
            Logger.Info((object) zenXml.Root.Item1);
        }
    }
}

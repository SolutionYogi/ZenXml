﻿using System;
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
            <Item3>
                <Item3_1>My Item 3.1</Item3_1>
            </Item3>    

            <Item4>4.0</Item4>
            <Item4>4.1</Item4>
            <Item4>4.2</Item4>
            <Item4>4.3</Item4>
        
        </Root>
";

        [Fact]
        public void TestRoot()
        {
            var zenXml = ZenXmlObject.CreateFromXml(TestXml, StringComparison.OrdinalIgnoreCase);
            Logger.Info(zenXml.Root.GetType());
        }

        [Fact]
        public void TestItem1()
        {
            var zenXml = ZenXmlObject.CreateFromXml(TestXml, StringComparison.OrdinalIgnoreCase);
            Logger.Info((object) zenXml.Root.Item1);
        }

        [Fact]
        public void TestItem2()
        {
            var zenXml = ZenXmlObject.CreateFromXml(TestXml, StringComparison.OrdinalIgnoreCase);
            Logger.Info((object) zenXml.Root.Item2.Attribute1);
        }

        [Fact]
        public void TestItem3()
        {
            var zenXml = ZenXmlObject.CreateFromXml(TestXml, StringComparison.OrdinalIgnoreCase);
            Logger.Info((object) zenXml.Root.Item3.Item3_1);
        }

        [Fact]
        public void TestItem4()
        {
            var zenXml = ZenXmlObject.CreateFromXml(TestXml, StringComparison.OrdinalIgnoreCase);
            foreach(var item4 in zenXml.Root.Item4)
            {
                Logger.Info(item4.InnerText);
            }
        }
    }
}
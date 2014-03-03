using System;
using System.IO;
using System.Reflection;
using System.Xml.Linq;
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

        private static XDocument GetXmlFile(string xmlFileName)
        {
            if(string.IsNullOrWhiteSpace(xmlFileName))
                throw new ArgumentNullException("xmlFileName");

            var assembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();

            var directoryName = Path.GetDirectoryName(assembly.Location);

            if(string.IsNullOrWhiteSpace(directoryName))
                throw new InvalidOperationException(string.Format("Could not identify Directory Path from Location {0}", assembly.Location));

            var finalFilePath = Path.Combine(directoryName, "XmlFiles", xmlFileName);

            if(! File.Exists(finalFilePath))
            {
                throw new InvalidOperationException(
                    string.Format(
                        "Could not find file with name [{0}] at location [{1}]. Have you verified that the xml file is included in the project and CopyOutput is set to 'CopyAlways'?",
                        xmlFileName, finalFilePath));
            }

            return XDocument.Load(finalFilePath);
        }

        private static XDocument CustomerOrder
        {
            get { return GetXmlFile("CustomerOrder.xml"); }
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
            var zenXml = ZenXmlObject.CreateFromXml(TestXml);
            Logger.Info(zenXml.Root.GetType());
        }

        [Fact]
        public void TestItem1()
        {
            var zenXml = ZenXmlObject.CreateFromXml(TestXml);
            Logger.Info(zenXml.Root.Item1);
        }

        [Fact]
        public void TestItem2()
        {
            var zenXml = ZenXmlObject.CreateFromXml(TestXml);
            Logger.Info(zenXml.Root.Item2.Attribute1);
        }

        [Fact]
        public void TestItem3()
        {
            var zenXml = ZenXmlObject.CreateFromXml(TestXml);
            Logger.Info(zenXml.Root.Item3.Item3_1);
        }


        [Fact]
        public void TestAsMethod()
        {
            var zenXml = ZenXmlObject.CreateFromXml(TestXml);
            Logger.Info(zenXml.Root.Item3.As<int>());
        }


        [Fact]
        public void TestItem4()
        {
            var zenXml = ZenXmlObject.CreateFromXml(TestXml);
            foreach(var item4 in zenXml.Root.Item4)
            {
                Logger.Info(item4.InnerText);
            }
        }

        [Fact]
        public void TestCustomers()
        {
            var zenXml = ZenXmlObject.CreateFromXContainer(CustomerOrder);
            var customers = zenXml.Root.Customers.AsEnumerable();
            foreach(var customer in customers)
            {
                Logger.Info(customer.CustomerID);
            }
        }

        [Fact]
        public void TestCustomerWithTitleManager()
        {
            var zenXml = ZenXmlObject.CreateFromXContainer(CustomerOrder);
            var customers = zenXml.Root.Customers.AsEnumerable();
            foreach(var customer in customers)
            {
                if(customer.ContactTitle.Equals("Marketing Manager", StringComparison.OrdinalIgnoreCase))
                    Logger.Info(customer.FullAddress.PostalCode);
            }
        }
    }
}
namespace NetCore2.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;

    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using NetCore2.Tests.Listeners;

    [TestClass]
    public class LoggerShould
    {
        private const string DefaultLog4NetConfigFileName = "log4net.config";

        private CustomTraceListener listener;

        [TestInitialize]
        public void Setup()
        {
            this.listener = new CustomTraceListener();
            Trace.Listeners.Add(listener);
        }

        [TestMethod]
        public void Include_ScopePropertyOnMessages_When_ScopeIsString()
        {
            var provider = new Log4NetProvider("./log4net.config");
            var logger = provider.CreateLogger("Test");

            const string message = "A message";
            using (var scope = logger.BeginScope("TEST_SCOPE"))
            {
                logger.LogCritical(message);
            }

            Assert.AreEqual(1, this.listener.Messages.Count);
            Assert.IsTrue(this.listener.Messages.Any(x => x.Contains(message)));
            Assert.IsTrue(this.listener.Messages.Any(x => x.Contains("TEST_SCOPE")));
        }

        [TestMethod]
        public void Include_ScopePropertyOnMessages_When_ScopeIsDictionaryOfObjects()
        {
            var provider = new Log4NetProvider("./log4net.config");
            var logger = provider.CreateLogger("Test");

            const string message = "A message";
            using (var scope = logger.BeginScope(new Dictionary<string, object>() { { "test", "SCOPED_VALUE" } }))
            {
                logger.LogCritical(message);
            }

            Assert.AreEqual(1, this.listener.Messages.Count);
            Assert.IsTrue(this.listener.Messages.Any(x => x.Contains(message)));
            Assert.IsTrue(this.listener.Messages.Any(x => x.Contains("SCOPED_VALUE")));
        }

        [TestMethod]
        public void Include_ScopePropertyOnMessages_When_ScopeIsDictionaryOfStrings()
        {
            var provider = new Log4NetProvider("./log4net.config");
            var logger = provider.CreateLogger("Test");

            const string message = "A message";
            using (var scope = logger.BeginScope(new Dictionary<string, string>() { { "test", "SCOPED_VALUE" } }))
            {
                logger.LogCritical(message);
            }

            Assert.AreEqual(1, this.listener.Messages.Count);
            Assert.IsTrue(this.listener.Messages.Any(x => x.Contains(message)));
            Assert.IsTrue(this.listener.Messages.Any(x => x.Contains("SCOPED_VALUE")));
        }

        [TestMethod]
        public void ProviderShouldBeCreatedWithOptions()
        {
            const string OverridOHLogFilePath = "overridOH.log";
            if (File.Exists(OverridOHLogFilePath))
            {
                File.Delete(OverridOHLogFilePath);
            }

            var options = GetLog4NetProviderOptions();
            var provider = new Log4NetProvider(options);
            var logger = provider.CreateLogger();
            logger.LogCritical("Test file creation");

            Assert.IsNotNull(provider);
            Assert.IsTrue(File.Exists(OverridOHLogFilePath));
        }

        [TestMethod]
        public void LogCriticalMessages()
        {
            var provider = new Log4NetProvider(DefaultLog4NetConfigFileName);
            var logger = provider.CreateLogger("Test");

            const string message = "A message";
            logger.LogCritical(message);

            Assert.AreEqual(1, this.listener.Messages.Count);
            Assert.IsTrue(this.listener.Messages.Any(x => x.Contains(message)));
        }

        [TestMethod]
        public void ProviderShouldCreateLoggerUsingConfigurationFileRelativePath()
        {
            var provider = new Log4NetProvider("./log4net.config");

            var logger = provider.CreateLogger("Test");

            const string message = "A message";
            logger.LogCritical(message);

            Assert.AreEqual(1, this.listener.Messages.Count);
            Assert.IsTrue(this.listener.Messages.Any(x => x.Contains(message)));
        }

        [TestMethod]
        public void UsePatternLayoutOnExceptions()
        {
            var provider = new Log4NetProvider(DefaultLog4NetConfigFileName);
            var logger = provider.CreateLogger("Test");

            try
            {
                ThrowException();
            }
            catch (Exception ex)
            {
                logger.LogCritical(10, ex, "Catched message");
            }

            Assert.AreEqual(1, this.listener.Messages.Count);
            Assert.IsTrue(this.listener.Messages.Any(x => x.Contains("Catched message")));
        }

        [TestMethod]
        public void ProviderShouldBeCreatedWithConfigurationSectionOverrides()
        {
            if (File.Exists("overrided.log"))
            {
                File.Delete("overrided.log");
            }

            var configuration = GetNetCoreConfiguration();
            var provider = new Log4NetProvider(DefaultLog4NetConfigFileName, configuration.GetSection("Logging"));
            var logger = provider.CreateLogger("test");
            logger.LogCritical("Test file creation");

            Assert.IsNotNull(provider);
            Assert.IsTrue(File.Exists("overrided.log"));
        }

        [TestMethod]
        public void ProviderShouldBeCreatedWithoutCoreConfigOverridesIfConfigSectionDoesNotContainData()
        {
            if (File.Exists("example.log"))
            {
                File.Delete("example.log");
            }

            var configuration = GetNetCoreConfiguration();
            var provider = new Log4NetProvider(DefaultLog4NetConfigFileName, configuration.GetSection("LoggingEmpty"));
            var logger = provider.CreateLogger("test");
            logger.LogCritical("Test file creation");

            Assert.IsNotNull(provider);
            Assert.IsTrue(File.Exists("example.log"));
        }

        /// <summary>
        /// Throws the exception, and have stacktrace to be tested by the ExceptionLayoutPattern.
        /// </summary>
        /// <exception cref="InvalidOperationException">A message</exception>
        private static void ThrowException()
            => throw new InvalidOperationException("A message");

        private static IConfigurationRoot GetNetCoreConfiguration()
            => new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory())
                                         .AddJsonFile("appsettings.json")
                                         .Build();

        /// <summary>
        /// Gets the log4net provider options.
        /// </summary>
        /// <returns></returns>
        private static Log4NetProviderOptions GetLog4NetProviderOptions()
            => new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory())
                                         .AddJsonFile("appsettings.json")
                                         .Build()
                                         .GetSection("Log4NetCore")
                                         .Get<Log4NetProviderOptions>();
    }
}
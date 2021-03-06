namespace FullFramework.Tests
{
	using System;
	using System.Diagnostics;
	using System.Linq;

	using Microsoft.Extensions.Logging;
	using Microsoft.VisualStudio.TestTools.UnitTesting;

	using FullFramework.Tests.Listeners;
	using System.IO;
	using System.Collections.Generic;
	using Microsoft.Extensions.Logging.Log4Net.AspNetCore.Entities;

	[TestClass]
	public class LoggerShould
	{
		private CustomTraceListener listener;

		[TestInitialize]
		public void Setup()
		{
			this.listener = new CustomTraceListener();
			Trace.Listeners.Add(listener);
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
			var provider = new Log4NetProvider("log4net.config");
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
			var provider = new Log4NetProvider("log4net.config");
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

		/// <summary>
		/// Throws the exception, and have stacktrace to be tested by the ExceptionLayoutPattern.
		/// </summary>
		/// <exception cref="InvalidOperationException">A message</exception>
		private static void ThrowException() => throw new InvalidOperationException("A message");

		/// <summary>
		/// Gets the log4net provider options.
		/// </summary>
		/// <returns></returns>
		private static Log4NetProviderOptions GetLog4NetProviderOptions()
		{
			var attributes = new Dictionary<string, string>();
			attributes.Add("Value", "overridOH.log");
			var nodeInfo = new NodeInfo { XPath = "" };
			var builder = new Log4NetProviderOptions()
			{
				PropertyOverrides = new List<NodeInfo>
				{
					new NodeInfo {
						XPath = "/log4net/appender[@name='RollingFile']/file",
						Attributes = attributes
					}
				}
			};
			return builder;
		}
	}
}
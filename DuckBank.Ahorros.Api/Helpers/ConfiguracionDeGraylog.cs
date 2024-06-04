using Serilog;
using Serilog.Events;
using Serilog.Sinks.Graylog;
using Serilog.Sinks.Graylog.Core.Transport;

namespace DuckBank.Ahorros.Api.Helpers
{
    public class ConsoleLoggerConfiguration
    {
        public bool Enabled { get; set; } = false;
        public LogEventLevel MinimumLevel { get; set; }
    }

    public class GraylogLoggerConfiguration
    {
        public bool Enabled { get; set; } = false;
        public string Host { get; set; } = "";
        public int Port { get; set; }
        public LogEventLevel MinimumLevel { get; set; }
    }

    public static class LoggerConfigurationExtensions
    {
        public static LoggerConfiguration AddConsoleLogger(this LoggerConfiguration loggerConfiguration,
            ConsoleLoggerConfiguration consoleLoggerConfiguration)
        {
            return consoleLoggerConfiguration.Enabled
                ? loggerConfiguration.WriteTo.Console(consoleLoggerConfiguration.MinimumLevel)
                : loggerConfiguration;
        }

        public static LoggerConfiguration AddGraylogLogger(this LoggerConfiguration loggerConfiguration,
        GraylogLoggerConfiguration graylogLoggerConfiguration)
        {
            return graylogLoggerConfiguration.Enabled
                ? loggerConfiguration.WriteTo.Graylog(graylogLoggerConfiguration.Host, graylogLoggerConfiguration.Port,
                    TransportType.Udp,true, graylogLoggerConfiguration.MinimumLevel)
                : loggerConfiguration;
        }
    }

    public static class ConfigureLogger
    {

        public static IHostBuilder ConfigureSerilog(this IHostBuilder builder)
            => builder.UseSerilog((context, loggerConfiguration)
                => ConfigureSerilogLogger(loggerConfiguration, context.Configuration));

        private static LoggerConfiguration ConfigureSerilogLogger(LoggerConfiguration loggerConfiguration,
            IConfiguration configuration)
        {

            GraylogLoggerConfiguration graylogLogger = new GraylogLoggerConfiguration();
            configuration.GetSection("Logging:Graylog").Bind(graylogLogger);
            ConsoleLoggerConfiguration consoleLogger = new ConsoleLoggerConfiguration();
            configuration.GetSection("Logging:Console").Bind(consoleLogger);

            return loggerConfiguration.AddConsoleLogger(consoleLogger).AddGraylogLogger(graylogLogger);
        }

    }
}

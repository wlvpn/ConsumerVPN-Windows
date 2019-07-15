using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Serilog
{
    /// <summary>
    /// Adds methods for enriching exception stack traces.
    /// </summary>
    public static class LoggerEnrichmentConfigurationExtensions
    {
        /// <summary>
        /// Enriches exception stack traces using Demystifier.
        /// </summary>
        /// <param name="enrichmentConfiguration">The logger enrichment configuration (`Enrich`).</param>
        /// <returns>Logger configuration to allow method chaining.</returns>
        public static LoggerConfiguration WithDemystifiedStackTraces(this LoggerEnrichmentConfiguration enrichmentConfiguration)
        {
            return enrichmentConfiguration.With(new DemystifiedStackTraceEnricher());
        }
    }

    public class DemystifiedStackTraceEnricher : ILogEventEnricher
    {
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            logEvent.Exception?.Demystify();
        }
    }
}
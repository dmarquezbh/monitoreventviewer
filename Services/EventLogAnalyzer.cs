#pragma warning disable CA1416 // Validate platform compatibility

using ScottPlot;
using System.Diagnostics;

namespace MonitorEventViewer.Services
{
    public class EventDetails
    {
        public DateTime TimeCreated { get; set; }
        public int EventId { get; set; }
        public string LevelText { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }

    public class EventLogAnalyzer
    {
        private readonly string _machineName;
        private readonly string _logName;

        public EventLogAnalyzer(string machineName, string logName)
        {
            _machineName = machineName;
            _logName = logName;
        }

        public Dictionary<string, int> GetEventCountsByTimeRange(DateTime startTime, DateTime endTime)
        {
            try
            {
                Console.WriteLine($"Buscando eventos de {startTime} até {endTime}");
                Console.WriteLine($"Máquina: {_machineName}, Log: {_logName}");

                var eventLog = new EventLog(_logName, _machineName);
                Console.WriteLine($"Log aberto, total de entradas: {eventLog.Entries.Count}");

                var counts = new Dictionary<string, int>();
                int processedEntries = 0;
                int matchingEntries = 0;

                foreach (EventLogEntry entry in eventLog.Entries)
                {
                    processedEntries++;
                    if (entry.TimeWritten >= startTime && entry.TimeWritten <= endTime)
                    {
                        matchingEntries++;
                        string source = entry.Source;
                        if (!counts.ContainsKey(source))
                            counts[source] = 0;
                        counts[source]++;
                    }
                }

                Console.WriteLine($"Processadas {processedEntries} entradas");
                Console.WriteLine($"Encontradas {matchingEntries} entradas no período");
                Console.WriteLine($"Total de fontes diferentes: {counts.Count}");

                return counts;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao buscar eventos: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                throw;
            }
        }

        public byte[] GenerateChartImage(Dictionary<string, int> eventCounts)
        {
            try
            {
                Console.WriteLine($"Iniciando geração do gráfico com {eventCounts.Count} eventos");

                if (eventCounts.Count == 0)
                {
                    Console.WriteLine("Nenhum evento para gerar gráfico");
                    return Array.Empty<byte>();
                }

                var plt = new Plot(1200, 800);
                Console.WriteLine("Plot criado");

                double[] values = eventCounts.Values.Select(x => (double)x).ToArray();
                string[] labels = eventCounts.Keys.ToArray();
                Console.WriteLine($"Arrays criados: {values.Length} valores, {labels.Length} rótulos");

                plt.AddBar(values);
                Console.WriteLine("Barras adicionadas");

                plt.XTicks(Enumerable.Range(0, labels.Length).Select(x => (double)x).ToArray(), labels);
                plt.XAxis.TickLabelStyle(rotation: 45);
                Console.WriteLine("Rótulos configurados");

                string tempFile = Path.Combine(Path.GetTempPath(), $"chart_{Guid.NewGuid()}.png");
                Console.WriteLine($"Arquivo temporário: {tempFile}");

                try
                {
                    plt.SaveFig(tempFile);
                    Console.WriteLine("Gráfico salvo no arquivo temporário");

                    var bytes = File.ReadAllBytes(tempFile);
                    Console.WriteLine($"Arquivo lido: {bytes.Length} bytes");
                    return bytes;
                }
                finally
                {
                    if (File.Exists(tempFile))
                    {
                        File.Delete(tempFile);
                        Console.WriteLine("Arquivo temporário deletado");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao gerar gráfico: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                throw;
            }
        }

        public byte[] TestChart()
        {
            try
            {
                var plt = new Plot(600, 400);
                double[] values = { 1, 2, 3, 4, 5 };
                plt.AddBar(values);

                string tempFile = Path.Combine(Path.GetTempPath(), $"test_chart_{Guid.NewGuid()}.png");
                plt.SaveFig(tempFile);

                return File.ReadAllBytes(tempFile);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro no teste do ScottPlot: {ex.Message}");
                throw;
            }
        }

        public List<EventDetails> GetEventDetails(string source, DateTime startTime, DateTime endTime)
        {
            var eventLog = new EventLog(_logName, _machineName);
            var details = new List<EventDetails>();

            foreach (EventLogEntry entry in eventLog.Entries)
            {
                if (entry.TimeWritten >= startTime &&
                    entry.TimeWritten <= endTime &&
                    entry.Source == source)
                {
                    details.Add(new EventDetails
                    {
                        TimeCreated = entry.TimeWritten,
                        EventId = (int)(entry.InstanceId & int.MaxValue), // Conversão segura de long para int
                        LevelText = entry.EntryType.ToString(),
                        Message = entry.Message
                    });
                }
            }

            return details;
        }
    }
}
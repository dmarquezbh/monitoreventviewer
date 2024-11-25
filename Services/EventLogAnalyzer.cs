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
            var counts = new Dictionary<string, int>();
            try
            {
                Console.WriteLine($"Tentando acessar log '{_logName}' na máquina '{_machineName}'");
                var eventLog = new EventLog(_logName, _machineName);
                Console.WriteLine($"Total de entradas no log: {eventLog.Entries.Count}");

                foreach (EventLogEntry entry in eventLog.Entries)
                {
                    if (entry.TimeWritten >= startTime && entry.TimeWritten <= endTime)
                    {
                        string source = entry.Source;
                        if (!counts.ContainsKey(source))
                            counts[source] = 0;
                        counts[source]++;
                    }
                }
                Console.WriteLine($"Eventos encontrados no período: {counts.Sum(x => x.Value)}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao acessar log de eventos: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
            }

            return counts;
        }

        public byte[] GenerateChartImage(Dictionary<string, int> eventCounts)
        {
            try
            {
                Console.WriteLine("Iniciando geração do gráfico");
                var plt = new Plot(1200, 800);

                double[] values = eventCounts.Values.Select(x => (double)x).ToArray();
                string[] labels = eventCounts.Keys.ToArray();

                Console.WriteLine($"Dados para o gráfico: {values.Length} valores, {labels.Length} rótulos");

                plt.AddBar(values);
                plt.XTicks(Enumerable.Range(0, labels.Length).Select(x => (double)x).ToArray(), labels);
                plt.XAxis.TickLabelStyle(rotation: 45);

                string tempFile = Path.Combine(Path.GetTempPath(), $"chart_{Guid.NewGuid()}.png");
                Console.WriteLine($"Salvando gráfico em: {tempFile}");

                plt.SaveFig(tempFile);
                Console.WriteLine("Gráfico salvo com sucesso");

                var bytes = File.ReadAllBytes(tempFile);
                Console.WriteLine($"Arquivo lido: {bytes.Length} bytes");

                if (File.Exists(tempFile))
                {
                    File.Delete(tempFile);
                    Console.WriteLine("Arquivo temporário deletado");
                }

                return bytes;
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
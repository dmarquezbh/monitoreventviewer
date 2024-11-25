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
            var eventLog = new EventLog(_logName, _machineName);
            var counts = new Dictionary<string, int>();

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

            return counts;
        }

        public byte[] GenerateChartImage(Dictionary<string, int> eventCounts)
        {
            var plt = new Plot(1200, 800);

            double[] values = eventCounts.Values.Select(x => (double)x).ToArray();
            string[] labels = eventCounts.Keys.ToArray();

            plt.AddBar(values);
            plt.XTicks(Enumerable.Range(0, labels.Length).Select(x => (double)x).ToArray(), labels);
            plt.XAxis.TickLabelStyle(rotation: 45);

            // Criar um arquivo temporário
            string tempFile = Path.Combine(Path.GetTempPath(), $"chart_{Guid.NewGuid()}.png");
            try
            {
                plt.SaveFig(tempFile);
                // Ler o arquivo e retornar como byte array
                return File.ReadAllBytes(tempFile);
            }
            finally
            {
                // Garantir que o arquivo temporário seja deletado
                if (File.Exists(tempFile))
                {
                    File.Delete(tempFile);
                }
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
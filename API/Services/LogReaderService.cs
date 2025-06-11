namespace API.Services
{
    public class LogEntry
    {
        public DateTime Timestamp { get; set; }
        public string Level { get; set; }
        public string Message { get; set; }
        public string User { get; set; }
        public string JsonData { get; set; }
    }

    public class LogReaderService
    {
        public List<LogEntry> GetLogs()
        {
            var logFile = $"Logs/actions-{DateTime.Today:yyyy-MM-dd}.json";
            if (!File.Exists(logFile)) return new();

            var lines = File.ReadAllLines(logFile);
            var logs = new List<LogEntry>();

            foreach (var line in lines)
            {
                if (line.Contains("Kullanıcı"))
                {
                    logs.Add(new LogEntry
                    {
                        Timestamp = ExtractTimestamp(line),
                        Message = ExtractMessage(line),
                        JsonData = ExtractJson(line)
                    });
                }
            }

            return logs;
        }

        private DateTime ExtractTimestamp(string line)
        {
            var timestamp = line.Substring(0, 19);
            return DateTime.ParseExact(timestamp, "yyyy-MM-dd HH:mm:ss", null);
        }

        private string ExtractMessage(string line)
        {
            var start = line.IndexOf("Kullanıcı");
            return line.Substring(start);
        }

        private string ExtractJson(string line)
        {
            var jsonStart = line.IndexOf('{');
            return jsonStart > 0 ? line.Substring(jsonStart) : "";
        }
    }

}

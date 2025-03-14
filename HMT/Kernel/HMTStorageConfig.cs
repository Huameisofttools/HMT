using HMT.Copilot;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace HMT.Kernel
{
    /// <summary>
    /// History data storage config class
    /// Willie Yao - 03/14/2025
    /// </summary>
    public static class HMTStorageConfig
    {
        private static readonly string AppDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "HMCopilotExtension");

        public static readonly string HistoryPath = Path.Combine(AppDataPath, "chat_history.json");

        /// <summary>
        /// Constructor
        /// Willie Yao - 03/14/2025
        /// </summary>
        static HMTStorageConfig()
        {
            if (!Directory.Exists(AppDataPath))
            {
                Directory.CreateDirectory(AppDataPath);
            }
        }
    }

    /// <summary>
    /// HMTChatHistory contract class
    /// Willie Yao - 03/14/2025
    /// </summary>
    [DataContract]
    public class HMTChatHistory
    {
        [DataMember(Order = 1)]
        public List<HMTChatMessage> Messages { get; set; } = new List<HMTChatMessage>();

        [DataMember(Order = 2)]
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

        [DataMember(Order = 3)]
        public int Version { get; set; } = 1;
    }

    /// <summary>
    /// History management service class
    /// Willie Yao - 03/14/2025
    /// </summary>
    public class HMTHistoryService
    {
        private const int MaxHistoryItems = 200;
        private static readonly JsonSerializerSettings Settings = new JsonSerializerSettings()
        {
            Formatting = Formatting.Indented,
            TypeNameHandling = TypeNameHandling.Auto
        };

        /// <summary>
        /// Load chat history message
        /// Willie Yao - 03/14/2025
        /// </summary>
        /// <returns>HMTChatHistory</returns>
        public static HMTChatHistory LoadHistory()
        {
            try
            {
                if (!File.Exists(HMTStorageConfig.HistoryPath))
                    return new HMTChatHistory();

                var content = File.ReadAllText(HMTStorageConfig.HistoryPath);
                return JsonConvert.DeserializeObject<HMTChatHistory>(content, Settings)
                       ?? new HMTChatHistory();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Load history failed​: {ex.Message}");
                return new HMTChatHistory();
            }
        }

        /// <summary>
        /// Save chat history message
        /// </summary>
        /// <param name="history">HMTChatHistory</param>
        public static void SaveHistory(HMTChatHistory history)
        {
            try
            {
                if (history.Messages.Count > MaxHistoryItems)
                {
                    history.Messages = history.Messages
                        .Skip(history.Messages.Count - MaxHistoryItems)
                        .ToList();
                }

                var content = JsonConvert.SerializeObject(history, Settings);
                File.WriteAllText(HMTStorageConfig.HistoryPath, content);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Save history failed​: {ex.Message}");
            }
        }
    }
}

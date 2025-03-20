using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using System.ComponentModel;
using HMT.Copilot;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;
using System.Runtime.CompilerServices;
using MdXaml;
using Newtonsoft.Json;
using RestSharp;
using suiren.Utilities;
using suiren.Services;
using suiren.Models;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace HMT.Views.Global
{
    public class HMTChatViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<HMTChatMessage> _messages = new ObservableCollection<HMTChatMessage>();
        private string _inputText;
        public event PropertyChangedEventHandler PropertyChanged;

        public ICommand SendCommand => new RelayCommand(SendMessageAsync);
        public ICommand ClearCommand => new RelayCommand(ClearMessages);

        public ObservableCollection<HMTChatMessage> Messages
        {
            get => _messages;
            set => SetField(ref _messages, value);
        }

        public string InputText
        {
            get => _inputText;
            set => SetField(ref _inputText, value);
        }

        public StyleInfo _selectedStyleInfo;
        public StyleInfo SelectedStyleInfo
        {
            get => _selectedStyleInfo;
            set
            {
                if (_selectedStyleInfo == value) return;
                _selectedStyleInfo = value;
            }
        }

        public HMTChatViewModel()
        {
            SelectedStyleInfo = new StyleInfo("SasabuneCompact", MarkdownStyle.SasabuneStandard);
        }

        private async Task ProcessStreamResponseAsync(HttpResponseMessage response, HMTChatMessage assistantMessage)
        {
            using (var stream = await response.Content.ReadAsStreamAsync())
            using (var reader = new StreamReader(stream))
            {
                StringBuilder buffer = new StringBuilder();
                char[] readBuffer = new char[4096];

                while (true)
                {
                    int bytesRead = await reader.ReadAsync(readBuffer, 0, readBuffer.Length);
                    if (bytesRead == 0) break;

                    string chunk = new string(readBuffer, 0, bytesRead);
                    buffer.Append(chunk);

                    while (true)
                    {
                        int dataStart = buffer.ToString().IndexOf("data: ");
                        if (dataStart == -1) break;

                        int dataEnd = buffer.ToString().IndexOf("\n\n", dataStart, StringComparison.Ordinal);
                        if (dataEnd == -1) break;

                        string dataLine = buffer.ToString().Substring(dataStart, dataEnd - dataStart);
                        buffer.Remove(0, dataEnd + 2);

                        ProcessSingleDataLine(dataLine, assistantMessage);
                    }
                }
            }            
        }

        private void ProcessSingleDataLine(string dataLine, HMTChatMessage assistantMessage)
        {
            if (!dataLine.StartsWith("data: ")) return;

            var json = dataLine.Substring("data: ".Length);
            if (json == "[DONE]") return;

            try
            {
                var chunk = JsonConvert.DeserializeObject<DskApiResponseChunk>(json);
                var content = chunk?.Choices?.FirstOrDefault()?.Delta?.Content;

                if (!string.IsNullOrEmpty(content))
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        assistantMessage.AppendContent(content);
                    });
                }
            }
            catch (JsonException) { }
        }

        private List<dynamic> BuildMessageHistory()
        {
            var messages = new List<dynamic>
            {
                new { content = "You are a helpful assistant", role = "system" }
            };

            foreach (var msg in Messages)
            {
                messages.Add(new
                {
                    content = msg.Content,
                    role = msg.IsUser ? "user" : "assistant"
                });
            }

            return messages;
        }

        private async void SendMessageAsync()
        {
            if (string.IsNullOrWhiteSpace(InputText)) return;

            var userMessage = new HMTChatMessage(InputText, true);
            Messages.Add(userMessage);

            var assistantMessage = new HMTChatMessage("", false);
            Messages.Add(assistantMessage);

            try
            {
                var requestBody = new
                {
                    messages = BuildMessageHistory(),
                    model = "deepseek-chat",
                    stream = true,
                    temperature = 1,
                    max_tokens = 2048
                };

                var json = JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "sk-613975d20507407399cb67b54a313504");

                var response = await httpClient.PostAsync("https://api.deepseek.com/chat/completions", content);
                await ProcessStreamResponseAsync(response, assistantMessage);
            }
            catch (Exception ex)
            {
                // Handle exception
            }
            finally
            {
                InputText = string.Empty;
            }
        }

        private void ClearMessages()
        {
            Messages.Clear();
        }

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class DskApiResponseChunk
    {
        [JsonProperty("choices")]
        public List<DskApiStreamChoice> Choices { get; set; }
    }

    public class DskApiStreamChoice
    {
        [JsonProperty("delta")]
        public DskApiStreamDelta Delta { get; set; }
    }

    public class DskApiStreamDelta
    {
        [JsonProperty("content")]
        public string Content { get; set; }
    }

    public class StyleInfo
    {
        public string Name { get; set; }
        public Style Style { get; set; }

        public StyleInfo(string name, Style style)
        {
            Name = name;
            Style = style;
        }

        public override int GetHashCode() => Name.GetHashCode();

        public override bool Equals(object val) => val is StyleInfo sf && Name == sf.Name;

        public static bool operator ==(StyleInfo left, StyleInfo right) => Equals(left, right);
        public static bool operator !=(StyleInfo left, StyleInfo right) => !Equals(left, right);
    }

    public class HMTAlignmentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => (bool)value ? HorizontalAlignment.Right : HorizontalAlignment.Left;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

    public class HMTBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => (bool)value ? new SolidColorBrush(Color.FromRgb(0, 120, 215)) : new SolidColorBrush(Colors.White);

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

    public class HMTForegroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => (bool)value ? Brushes.White : Brushes.Black;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

    public class HMTPercentageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double width && parameter is string percentageStr &&
                double.TryParse(percentageStr, NumberStyles.Any, culture, out double percentage))
            {
                return width * percentage;
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

    public partial class HAiMainChatWindowControl : UserControl
    {
        public HAiMainChatWindowControl()
        {
            var _ = new MdXaml.MarkdownScrollViewer();
            this.InitializeComponent();
            DataContext = new HMTChatViewModel();
        }

        private void InputBox_KeyDown(object sender, KeyEventArgs e) { }
    }
}
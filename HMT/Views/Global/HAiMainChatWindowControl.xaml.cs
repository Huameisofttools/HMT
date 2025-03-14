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
using System.Threading.Tasks;
using HMT.Kernel;
using System.Linq;
using System.IO;
using System.Collections.Specialized;

namespace HMT.Views.Global
{
    /// <summary>
    /// The model view for HAi main chat window control.
    /// Willie Yao - 03/14/2025
    /// </summary>
    public class HMTChatViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<HMTChatMessage> _messages = new ObservableCollection<HMTChatMessage>();
        private string _inputText;

        public event PropertyChangedEventHandler PropertyChanged;
        public ICommand SendCommand => new RelayCommand(SendMessage);
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

        private async void SendMessage()
        {
            if (string.IsNullOrWhiteSpace(InputText)) return;

            var newMessage = new HMTChatMessage(InputText, true);
            Messages.Add(newMessage);

            try
            {
                // var response = await ChatService.GetResponseAsync(InputText);
                Messages.Add(new HMTChatMessage("response", false));
            }
            catch (Exception ex)
            {
                Messages.Add(new HMTChatMessage($"Error: {ex.Message}", false));
            }

            InputText = string.Empty;
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

    public class HMTAlignmentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? HorizontalAlignment.Right : HorizontalAlignment.Left;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class HMTBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ?
                new SolidColorBrush(Color.FromRgb(0, 120, 215)) :
                new SolidColorBrush(Colors.White);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class HMTForegroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isUser = (bool)value;
            return isUser
                ? Brushes.White
                : Brushes.Black;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Interaction logic for HAiMainChatWindowControl.
    /// </summary>
    public partial class HAiMainChatWindowControl : UserControl
    {
        private List<dynamic> messages;

        /// <summary>
        /// Initializes a new instance of the <see cref="HAiMainChatWindowControl"/> class.
        /// </summary>
        public HAiMainChatWindowControl()
        {
            this.InitializeComponent();
            //messages = new List<dynamic>();
            //messages.Add(new
            //{
            //    content = "You are a helpful assistant",
            //    role = "system"
            //});
            DataContext = new HMTChatViewModel();

        }

        private void InputBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {

        }

        //private void AddMessageToPanel(string message, string sender)
        //{
        //    StackPanel messagePanel = new StackPanel { Orientation = Orientation.Horizontal };
        //    TextBlock messageBlock = new TextBlock
        //    {
        //        Text = message,
        //        Margin = new Thickness(5),
        //        TextWrapping = TextWrapping.Wrap
        //    };

        //    messagePanel.Children.Add(messageBlock);
        //    chatPanel.Children.Add(messagePanel);
        //    scrollViewer.ScrollToEnd();
        //}

        ///// <summary>
        ///// Handles click on the button by displaying a message box.
        ///// </summary>
        ///// <param name="sender">The event sender.</param>
        ///// <param name="e">The event args.</param>        
        //private async void button1_Click(object sender, RoutedEventArgs e)  
        //{           
        //    string responseString = string.Empty;
        //    string input = inputBox.Text;
        //    RestClient restClient = ClientFactory.CreateDskHttpClient();
        //    OpenaiApiService openaiApiService = new OpenaiApiService(restClient);

        //    AddMessageToPanel(input, "User");
        //    clearInputInfo();

        //    messages.Add(new
        //    {
        //        content = input,
        //        role = "user"
        //    });

        //    var request = new
        //    {
        //        messages,
        //        model = "deepseek-chat",
        //        frequency_penalty = 0,
        //        max_tokens = 2048,
        //        presence_penalty = 0,
        //        response_format = new
        //        {
        //            type = "text"
        //        },
        //        stop = (string)null,
        //        stream = false,
        //        stream_options = (string)null,
        //        temperature = 1,
        //        top_p = 1,
        //        tools = (string)null,
        //        tool_choice = "none",
        //        logprobs = false,
        //        top_logprobs = (string)null
        //    };

        //    var body = JsonConvert.SerializeObject(request);

        //    var response = await openaiApiService.GetDskDataAsync(body).ConfigureAwait(false);            

        //    if (response.IsSuccessful)
        //    {
        //        DskApiResponse dskApiResponse = JsonConvert.DeserializeObject<DskApiResponse>(response.Content);
        //        dskApiResponse.Choices.ForEach(choice =>
        //        {
        //            if (choice.Message.Role == "assistant")
        //            {
        //                responseString = choice.Message.Content;
        //            }
        //        });
        //    }
        //    else
        //    {
        //        responseString = "**TIME OUT**";
        //    }

        //    AddMessageToChat(responseString, false);
        //}

        //private void clearInputInfo()
        //{
        //    Dispatcher.Invoke(() =>
        //    {
        //        sendBtn.IsEnabled = false;
        //        inputBox.Text = string.Empty;
        //    });
        //}

        //private void AddMessageToChat(string message, bool isUser)
        //{           
        //    Dispatcher.Invoke((Action)(() =>
        //    {
        //        RichTextBox resultRichTextBox = new RichTextBox();
        //        Markdown engine = new Markdown();
        //        if (string.IsNullOrEmpty(message))
        //        {
        //            StackPanel messagePanel = new StackPanel { Orientation = Orientation.Horizontal };
        //            TextBlock messageBlock = new TextBlock
        //            {
        //                Text = "No result",
        //                Margin = new Thickness(5),
        //                TextWrapping = TextWrapping.Wrap
        //            };

        //            messagePanel.Children.Add(messageBlock);
        //            chatPanel.Children.Add(messagePanel);
        //        }
        //        else
        //        {
        //            //MarkdownScrollViewer markdownScrollViewer = new MarkdownScrollViewer()
        //            //{
        //            //    Engine = engine
        //            //};
        //            FlowDocument document = engine.Transform(message);
        //            //resultRichTextBox.Document = document;
        //            FlowDocumentReader flowDocumentReader = new FlowDocumentReader();
        //            flowDocumentReader.Document = document;
        //            chatPanel.Children.Add(flowDocumentReader);
        //        }

        //        sendBtn.IsEnabled = true;


        //    }));
        //}
    }
}
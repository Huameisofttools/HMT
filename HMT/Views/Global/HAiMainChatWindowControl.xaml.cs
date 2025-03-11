using Newtonsoft.Json;
using RestSharp;
using suiren.Services;
using suiren.Utilities;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace suiren
{
    /// <summary>
    /// Interaction logic for HAiMainChatWindowControl.
    /// </summary>
    public partial class HAiMainChatWindowControl : UserControl
    {
        // private DispatcherTimer typingTimer;
        private string currentTypingMessage;
        private int typingIndex;
        private StackPanel currentMessagePanel;
        private List<dynamic> messages;

        /// <summary>
        /// Initializes a new instance of the <see cref="HAiMainChatWindowControl"/> class.
        /// </summary>
        public HAiMainChatWindowControl()
        {
            this.InitializeComponent();
            messages = new List<dynamic>();
            messages.Add(new
            {
                content = "You are a helpful assistant",
                role = "system"
            });
        }

        private void AddMessageToPanel(string message, string sender)
        {
            StackPanel messagePanel = new StackPanel { Orientation = Orientation.Horizontal };
            TextBlock messageBlock = new TextBlock
            {
                Text = message,
                Margin = new Thickness(5),
                TextWrapping = TextWrapping.Wrap
            };

            messagePanel.Children.Add(messageBlock);
            chatPanel.Children.Add(messagePanel);
            scrollViewer.ScrollToEnd();
        }

        /// <summary>
        /// Handles click on the button by displaying a message box.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event args.</param>        
        private async void button1_Click(object sender, RoutedEventArgs e)  
        {           
            string responseString = string.Empty;
            string input = inputBox.Text;
            RestClient restClient = ClientFactory.CreateDskHttpClient();
            OpenaiApiService openaiApiService = new OpenaiApiService(restClient);

            AddMessageToPanel(input, "User");
            clearInputInfo();

            messages.Add(new
            {
                content = input,
                role = "user"
            });

            var request = new
            {
                messages,
                model = "deepseek-chat",
                frequency_penalty = 0,
                max_tokens = 2048,
                presence_penalty = 0,
                response_format = new
                {
                    type = "text"
                },
                stop = (string)null,
                stream = false,
                stream_options = (string)null,
                temperature = 1,
                top_p = 1,
                tools = (string)null,
                tool_choice = "none",
                logprobs = false,
                top_logprobs = (string)null
            };

            var body = JsonConvert.SerializeObject(request);

            var response = await openaiApiService.GetDskDataAsync(body).ConfigureAwait(false);            

            response.Choices.ForEach(choice =>
            {
                if (choice.Message.Role == "assistant")
                {
                    responseString = choice.Message.Content;
                }                    
            });

            AddMessageToChat(responseString, false);
        }

        private void clearInputInfo()
        {
            Dispatcher.Invoke(() =>
            {
                sendBtn.IsEnabled = false;
                inputBox.Text = string.Empty;
            });
        }

        private void AddMessageToChat(string message, bool isUser)
        {
            Dispatcher.Invoke(() =>
            {
                TextBlock messageBlock = new TextBlock
                {
                    TextWrapping = TextWrapping.Wrap,
                    Margin = new Thickness(5),
                    Text = ""
                };
                chatPanel.Children.Add(messageBlock);

                DispatcherTimer timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(5) };
                int charIndex = 0;
                timer.Tick += (s, args) =>
                {
                    if (charIndex < message.Length)
                    {
                        messageBlock.Text += message[charIndex];
                        charIndex++;
                    }
                    else
                    {
                        timer.Stop();
                        sendBtn.IsEnabled = true;
                    }
                };
                timer.Start();
            });
        }

    }
}
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace HMT.Copilot
{
    public class HMTChatMessage : INotifyPropertyChanged
    {
        private string _content;

        public bool IsUser { get; }

        public HMTChatMessage(string _content, bool _isUser)
        {
            Content = _content;
            IsUser = _isUser;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private readonly StringBuilder _rawBuffer = new StringBuilder();
        private string _renderedContent;

        // 渲染帧率控制（60 FPS）
        private const int RenderIntervalMs = 16;
        private DateTime _lastRenderTime = DateTime.MinValue;

        // 在 HMTChatMessage 中添加
        private readonly StringBuilder _renderBuffer = new StringBuilder();
        private DateTime _lastUpdateTime = DateTime.MinValue;

        public string Content
        {
            get => _renderedContent;
            private set
            {
                _renderedContent = value;
                OnPropertyChanged();
            }
        }

        public void AppendHighFrequencyContent(string text)
        {
            lock (_rawBuffer)
            {
                _rawBuffer.Append(text);
            }

            // 节流渲染
            if ((DateTime.Now - _lastRenderTime).TotalMilliseconds >= RenderIntervalMs)
            {
                ForceRender();
            }
        }

        private void ForceRender()
        {
            lock (_rawBuffer)
            {
                if (_rawBuffer.Length == 0) return;

                // 分段更新而非全量替换
                string newContent = _rawBuffer.ToString();
                _rawBuffer.Clear();

                // 触发逐字符动画
                StartIncrementalRender(newContent);
            }
        }

        public void FinalFlush()
        {
            lock (_rawBuffer)
            {
                if (_rawBuffer.Length > 0)
                {
                    Content += _rawBuffer.ToString();
                    _rawBuffer.Clear();
                }
            }
        }

        private async void StartIncrementalRender(string content)
        {
            for (int i = 0; i < content.Length; i++)
            {
                Content += content[i];
                await Task.Delay(30); // 每个字符间隔 30ms
            }
        }

        public void AppendContent(string text)
        {
            lock (_renderBuffer)
            {
                _renderBuffer.Append(text);

                // 每30ms或缓冲区超过5字符时刷新
                if (DateTime.Now - _lastUpdateTime > TimeSpan.FromMilliseconds(30) ||
                    _renderBuffer.Length >= 5)
                {
                    string flushContent = _renderBuffer.ToString();
                    _renderBuffer.Clear();

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        Content += flushContent;
                    });

                    _lastUpdateTime = DateTime.Now;
                }
            }
        }
    }
}

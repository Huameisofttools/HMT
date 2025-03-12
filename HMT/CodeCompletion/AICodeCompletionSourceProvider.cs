using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;

namespace HMT.CodeCompletion
{
    [Export(typeof(IInlineCompletionSourceProvider))]
    [ContentType("text")]
    public class AICodeCompletionSourceProvider : IInlineCompletionSourceProvider
    {
        public IInlineCompletionSource TryCreateInlineCompletionSource(ITextView textView)
        {
            return new AICodeCompletionSource();
        }
    }

    public class AICodeCompletionSource : IInlineCompletionSource
    {
        public async Task<InlineCompletionItem?> GetInlineCompletionItemAsync(IInlineCompletionSession session, CancellationToken token)
        {
            string contextCode = GetCodeContext(session.TextView);
            string suggestion = await CallAIModel(contextCode); // 调用AI服务
            return new InlineCompletionItem(suggestion);
        }

        public void ShowGhostText(ITextView view, string suggestion)
        {
            var caretPosition = view.Caret.Position.BufferPosition;
            var span = new SnapshotSpan(caretPosition, suggestion.Length);
            var brush = new SolidColorBrush(Color.FromArgb(0x80, 0xCC, 0xCC, 0xCC)); // 半透明灰色
            var textAdornment = new TextAdornment(view, span, suggestion, brush);
            view.GetAdornmentLayer("GhostTextLayer").Add(textAdornment);
        }
    }
}

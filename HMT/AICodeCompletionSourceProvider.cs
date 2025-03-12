using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion;
using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion.Data;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;
using System;
using System.ComponentModel.Composition;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;

namespace HMT.CodeCompletion
{
    [Export(typeof(IAsyncCompletionSourceProvider))]
    [ContentType("text")]
    public class AICodeCompletionSourceProvider : IAsyncCompletionSourceProvider
    {
        public IAsyncCompletionSource GetOrCreate(ITextView textView)
        {
            throw new NotImplementedException();
        }

        public IAsyncCompletionSource TryCreateInlineCompletionSource(ITextView textView)
        {
            return new AICodeCompletionSource();
        }
    }

    public class AICodeCompletionSource : IAsyncCompletionSource
    {
        public Task<CompletionContext> GetCompletionContextAsync(IAsyncCompletionSession session, CompletionTrigger trigger, SnapshotPoint triggerLocation, SnapshotSpan applicableToSpan, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public Task<object> GetDescriptionAsync(IAsyncCompletionSession session, CompletionItem item, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        //public async Task<InlineCompletionItem?> GetInlineCompletionItemAsync(IInlineCompletionSession session, CancellationToken token)
        //{
        //    string contextCode = GetCodeContext(session.TextView);
        //    string suggestion = await CallAIModel(contextCode); // 调用AI服务
        //    return new InlineCompletionItem(suggestion);
        //}

        public CompletionStartData InitializeCompletion(CompletionTrigger trigger, SnapshotPoint triggerLocation, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        //public void ShowGhostText(ITextView view, string suggestion)
        //{
        //    var caretPosition = view.Caret.Position.BufferPosition;
        //    var span = new SnapshotSpan(caretPosition, suggestion.Length);
        //    var brush = new SolidColorBrush(Color.FromArgb(0x80, 0xCC, 0xCC, 0xCC)); // 半透明灰色
        //    var textAdornment = new TextAdornment(view, span, suggestion, brush);
        //    view.GetAdornmentLayer("GhostTextLayer").Add(textAdornment);
        //}
    }
}

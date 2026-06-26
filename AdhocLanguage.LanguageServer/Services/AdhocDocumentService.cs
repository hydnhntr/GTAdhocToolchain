using EmmyLua.LanguageServer.Framework.Protocol.Model.TextDocument;

using Esprima;
using Esprima.Ast;

using GTAdhocToolchain.Analyzer;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace AdhocLanguage.LanguageServer.Services;

public class AdhocDocumentService
{
    private readonly ConcurrentDictionary<string, DocumentState> _documents = new();

    public static DateTime LastParsed { get; set; }
    private static readonly Lock thisLock = new Lock();

    public void ScanFile(string uri, string code, int version, bool force = false)
    {
        var now = DateTime.Now;
        if (!force && (now - LastParsed).TotalSeconds < 0.5)
            return;

        LastParsed = now;

        lock (thisLock)
        {
            var errHandler = new CollectingErrorHandler();
            AdhocAbstractSyntaxTree tree = new AdhocAbstractSyntaxTree(new ParserOptions()
            {
                ErrorHandler = errHandler
            });

            Script script = tree.ParseScript(code);

            var analyser = new AdhocScriptAnalyzer();
            analyser.ErrorHandler = errHandler;
            analyser.ParseScript(script);

            _documents[uri] = new DocumentState(uri, code, tree, analyser, version);
        }
    }

    public DocumentState? GetDocument(string uri)
    {
        return _documents.GetValueOrDefault(uri);
    }

    public void ApplyFileChanges(List<TextDocumentContentChangeEvent> changes)
    {
        var now = DateTime.Now;
        if ((now - LastParsed).TotalSeconds < 0.5)
            return;

        LastParsed = now;
    }

    public void Remove(string uri)
    {
        _documents.TryRemove(uri, out _);
    }
}

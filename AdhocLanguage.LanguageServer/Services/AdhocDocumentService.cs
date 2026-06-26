using EmmyLua.LanguageServer.Framework.Protocol.Model.Diagnostic;
using EmmyLua.LanguageServer.Framework.Protocol.Model.TextDocument;
using EmmyLua.LanguageServer.Framework.Protocol.Model;
using LSPPos = EmmyLua.LanguageServer.Framework.Protocol.Model.Position;

using Esprima;
using Esprima.Ast;

using GTAdhocToolchain.Analyzer;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Threading;

namespace AdhocLanguage.LanguageServer.Services;

public class AdhocDocumentService
{
    private readonly ConcurrentDictionary<string, DocumentState> _documents = new();

    public static DateTime LastParsed { get; set; }
    private static readonly Lock thisLock = new Lock();
    private CollectingErrorHandler _errorHandler;

    public void ScanFile(string uri, string code, int version, bool force = false)
    {
        var now = DateTime.Now;
        if (!force && (now - LastParsed).TotalSeconds < 0.5)
            return;

        LastParsed = now;

        lock (thisLock)
        {
            _errorHandler = new CollectingErrorHandler();
            AdhocAbstractSyntaxTree tree = new AdhocAbstractSyntaxTree(new ParserOptions()
            {
                ErrorHandler = _errorHandler
            });

            Script script = tree.ParseScript(code);

            var analyser = new AdhocScriptAnalyzer();
            analyser.ErrorHandler = _errorHandler;
            analyser.ParseScript(script);

            var docState = new DocumentState(uri, code, tree, analyser, version);
            _documents[uri] = docState;

            foreach (var err in _errorHandler.Errors)
            {
                docState.Diagnostics.Add(new Diagnostic()
                {
                    Message = err.Description,
                    Source = err.Source,
                    Range = DocumentRange.From(new LSPPos(err.LineNumber - 1, err.Column), new LSPPos(err.LineNumber - 1, err.Column)),
                    Severity = DiagnosticSeverity.Error,
                });
            }
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

public class DocumentState
{
    public string Uri { get; set; }
    public string Text { get; set; }
    public AdhocAbstractSyntaxTree Ast { get; set; }
    public AdhocScriptAnalyzer Analyser { get; set; }
    public List<Diagnostic>? Diagnostics { get; set; } = [];
    public int Version { get; set; }

    public DocumentState(string uri, string text, AdhocAbstractSyntaxTree ast, AdhocScriptAnalyzer analyser, int version)
    {
        Uri = uri;
        Text = text;
        Ast = ast;
        Analyser = analyser;
        Version = version;
    }
}


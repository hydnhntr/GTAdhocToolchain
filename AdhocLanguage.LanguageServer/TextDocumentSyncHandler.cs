using AdhocLanguage.LanguageServer.Services;

using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Client.ClientCapabilities;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Server;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Server.Options;
using EmmyLua.LanguageServer.Framework.Protocol.Message.TextDocument;
using EmmyLua.LanguageServer.Framework.Protocol.Model.Diagnostic;
using EmmyLua.LanguageServer.Framework.Protocol.Model.TextEdit;
using EmmyLua.LanguageServer.Framework.Server.Handler;

using Esprima;
using Esprima.Ast;

using GTAdhocToolchain.Analyzer;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AdhocLanguage.LanguageServer;

class TextDocumentSyncHandler : TextDocumentHandlerBase
{
    private readonly ServerContext _context;

    public TextDocumentSyncHandler(ServerContext context)
    {
        _context = context;
    }

    public TextDocumentSyncKind Change { get; } = TextDocumentSyncKind.Full;

    protected override Task Handle(DidOpenTextDocumentParams request, CancellationToken token)
    {
        Console.Error.WriteLine($"TextDocumentHandler: DidOpenTextDocumentParams {request.TextDocument.Uri}");

        _context.Documents.ScanFile(request.TextDocument.Uri.FileSystemPath, request.TextDocument.Text, request.TextDocument.Version, force: true);
        //PublishDiagnostics(uri, ast);

        return Task.CompletedTask;
    }

    protected override Task Handle(DidChangeTextDocumentParams request, CancellationToken token)
    {
        var newText = request.ContentChanges.LastOrDefault()?.Text;
        if (newText is not null)
            _context.Documents.ScanFile(request.TextDocument.Uri.FileSystemPath, newText, request.TextDocument.Version);
        
        Console.Error.WriteLine($"TextDocumentHandler: DidChangeTextDocumentParams {request.TextDocument.Uri}");
        return Task.CompletedTask;
    }

    protected override Task Handle(DidCloseTextDocumentParams request, CancellationToken token)
    {
        Console.Error.WriteLine($"TextDocumentHandler: DidCloseTextDocument {request.TextDocument.Uri}");

        _context.Documents.Remove(request.TextDocument.Uri.FileSystemPath);

        return Task.CompletedTask;
    }

    protected override Task Handle(WillSaveTextDocumentParams request, CancellationToken token)
    {
        Console.Error.WriteLine($"TextDocumentHandler: WillSaveTextDocument {request.TextDocument.Uri}");
        return Task.CompletedTask;
    }

    protected override Task<List<TextEdit>?> HandleRequest(WillSaveTextDocumentParams request, CancellationToken token)
    {
        Console.Error.WriteLine($"TextDocumentHandler: WillSaveTextDocumentRequest {request.TextDocument.Uri}");
        return Task.FromResult<List<TextEdit>?>(null);
    }

    public override void RegisterCapability(ServerCapabilities serverCapabilities, ClientCapabilities clientCapabilities)
    {
        serverCapabilities.TextDocumentSync = new TextDocumentSyncOptions
        {
            Change = TextDocumentSyncKind.Full,
            OpenClose = true,
            WillSave = true,
            WillSaveWaitUntil = true,
            Save = true
        };
    }
}

public class DocumentState
{
    public string Uri { get; set; }
    public string Text { get; set; }
    public AdhocAbstractSyntaxTree Ast { get; set; }
    public AdhocScriptAnalyzer Analyser { get; set; }
    public List<Diagnostic>? Diagnostics { get; set; }
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

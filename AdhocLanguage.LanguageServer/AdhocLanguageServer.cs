using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using AdhocLanguage.LanguageServer.Services;

namespace AdhocLanguage.LanguageServer;

public class AdhocLanguageServer
{
    public AdhocLanguageServer()
    {

    }

    public async Task StartAsync()
    {
        try
        { 
            var server = EmmyLua.LanguageServer.Framework.Server.LanguageServer.From(Console.OpenStandardInput(), Console.OpenStandardOutput());
            server.OnInitialize((request, serverInfo) =>
            {
                serverInfo.Name = "Adhoc Language Server";
                serverInfo.Version = "1.0.0";
                return Task.CompletedTask;
            });

            server.OnInitialized(async (request) =>
            {
                await server.Client.LogInfo("Server initialized!");
            });

            var context = new ServerContext();
            server.AddHandler(new TextDocumentSyncHandler(context));
            server.AddHandler(new CompletionHandler(context));
            server.AddHandler(new DocumentSymbolHandler(context));
            await server.Run();
        }
        catch (Exception ex)
        {
            await File.WriteAllTextAsync(
                Path.Combine(Path.GetTempPath(), "adhoc-lsp-crash.log"),
                ex.ToString());
        }
    }
}

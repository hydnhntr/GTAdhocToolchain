using System;
using System.Collections.Generic;
using System.Text;

namespace AdhocLanguage.LanguageServer.Services;

public class ServerContext
{
    public AdhocDocumentService Documents { get; set; } = new();
}

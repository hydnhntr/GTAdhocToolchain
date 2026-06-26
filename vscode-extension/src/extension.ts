'use strict';
import * as path from 'path';
import * as vscode from 'vscode';
import {
  LanguageClient,
  LanguageClientOptions,
  ServerOptions,
  TransportKind
} from 'vscode-languageclient/node';

let client: LanguageClient;

export function activate(context: vscode.ExtensionContext) 
{
    var type = "build_script";
    vscode.tasks.registerTaskProvider(type, 
    {
        provideTasks(token?: vscode.CancellationToken) 
        {
            if (vscode.workspace.workspaceFolders === undefined)
            {
                return;
            }

            var currentlyOpenTabfilePath = vscode.window.activeTextEditor?.document.uri.fsPath;
            if (currentlyOpenTabfilePath === undefined)
            {
                return undefined;
            }

            var execution = new vscode.ShellExecution(`adhoc build -i ${currentlyOpenTabfilePath}`);
            return [
                new vscode.Task({type: type}, vscode.TaskScope.Workspace,
                    "Build Adhoc Script", "Adhoc", execution, undefined)
            ];
        },
        resolveTask(task: vscode.Task, token?: vscode.CancellationToken) 
        {
            return task;
        }
    });

    const serverOptions: ServerOptions = 
    {
      run: 
      {
        command: 'adhoc',
        args: ["language-server"],
        transport: TransportKind.stdio,
        
      },
      debug: 
      {
        command: 'adhoc',
        args: ["language-server", "--wait-for-debugger"],
        transport: TransportKind.stdio
      }
    };
  
    const clientOptions: LanguageClientOptions = 
    {
      documentSelector: [{ scheme: 'file', language: 'adhoc' }] // match package.json's language id, or 'plaintext' for quick testing
    };
  
    client = new LanguageClient(
      'adhoc',
      'Adhoc Language Server',
      serverOptions,
      clientOptions
    );
  
    client.start();
}
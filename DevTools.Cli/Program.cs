using Cocona;
using DevTools.Cli;

var builder = CoconaApp.CreateBuilder();

var app = builder.Build();

app.AddCommands<DevToolsCommands>();

app.Run();
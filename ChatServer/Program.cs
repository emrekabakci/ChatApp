using ChatUygulamasi.ChatServer;

ChatServer server = new ChatServer();

EventHandler? e = new EventHandler(OnExit);
AppDomain.CurrentDomain.ProcessExit += e;

server.Run();

void OnExit(object? sender, EventArgs? e)
{
    server.Cleanup();
}


using Fleck;
using System;
using WitcherScriptCompanion.Events;
using WitcherScriptCompanion.Events.Debug;

namespace WitcherScriptCompanion.Features
{
    public abstract class Feature
    {
        public void Execute()
        {
        }
    }

    public class CookFeature : Feature
    {
        public CookFeature()
        {
            // recebe aqui os parametros necessarios para executar uma ação.

            // Cook command pode usar vários features....
        }
    }

    public class DebugFeature
    {
        private WebSocketServer server;

        public DebugFeature()
        {
            server = new WebSocketServer("ws://127.0.0.1:8181");

            server.Start(client =>
            {
                client.OnOpen = () => StartDebug();

                client.OnClose = () => OnClientDisconnect();

                client.OnMessage = message => HandleMessages(message);
            });
        }

        private void OnBreakpointSetSucessfull()
        {
            EventManager.Send(new DebugBreakpointResponseEvent(15, true));
        }

        private void OnClientDisconnect()
        {
            Console.WriteLine("Client disconnected");

            server.Dispose();
        }

        public void HandleMessages(string message)
        {
            Console.WriteLine("Message: " + message);
        }

        private void StartDebug()
        {
            Console.WriteLine("Client connected");
        }
    }
}
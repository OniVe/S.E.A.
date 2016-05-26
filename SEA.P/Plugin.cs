using System;
using Sandbox;
using VRage.Plugins;
using SEA.P.Web;

namespace SEA.P
{
    public sealed class Plugin : IPlugin
    {
        private Server webServer;

        public void Init( object gameInstance )
        {
            MySandboxGame.Log.WriteLineAndConsole("S.E.A: Initializing Web Server");
            var port = Models.Settings.ServerPort;
            webServer = new Server(port);
            if (webServer != null && webServer.Start())
            {
                MySandboxGame.Log.WriteLineAndConsole("S.E.A: Web Server is running");
                if (Models.Settings.launchBrowserOnStartup)
                {
                    MySandboxGame.Log.WriteLineAndConsole("S.E.A: Opening the browser page...");
                    System.Diagnostics.Process.Start($"http://localhost:{port.ToString()}/");
                }
            }
            else
            {
                MySandboxGame.Log.WriteLineAndConsole("S.E.A: Web Server start fail");
            }
        }

        public void Update()
        {
            //throw new NotImplementedException();
        }
        public void Dispose()
        {
            if (webServer != null)
                webServer.Stop();
        }
    }
}

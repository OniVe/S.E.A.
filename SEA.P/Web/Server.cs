using Microsoft.AspNet.SignalR;
using Microsoft.Owin.Hosting;
using System;
using System.Text;

namespace SEA.P.Web
{
    public class Server
    {
        private IDisposable Host { get; set; }
        private StartOptions startOptions = null;
        private bool isRun = false;
        public bool IsRun => isRun;

        public Server(int port)
        {
            startOptions = new StartOptions();
            startOptions.AppStartup = "http://+";
            startOptions.Port = port;

            /*var hostName = System.Net.Dns.GetHostName();
            startOptions.Urls.Add(string.Format("http://{0}:{1}", "localhost", port));
            startOptions.Urls.Add(string.Format("http://{0}:{1}", "127.0.0.1", port));
            startOptions.Urls.Add(string.Format("http://{0}:{1}", hostName, port));

            foreach (System.Net.IPAddress ip in System.Net.Dns.GetHostAddresses(hostName))
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    startOptions.Urls.Add(string.Format("http://{0}:{1}", ip.ToString(), port));*/
        }
        public bool Start()
        {
            Sandbox.MySandboxGame.Log.WriteLineAndConsole("S.E.A: Web server starting...");

            if (startOptions == null)
                return false;

            try
            {
                Sandbox.MySandboxGame.Log.WriteLineAndConsole("S.E.A: Web server url: " + startOptions.AppStartup + ":" + startOptions.Port.ToString());
                Host = WebApp.Start<Startup>(string.Format("http://{0}:{1}/", "+", startOptions.Port));
                Hubs.seaHub.context = new HUBContext();
                isRun = true;
                return true;
            }
            catch (Exception ex)
            {
                Sandbox.MySandboxGame.Log.WriteLineAndConsole("S.E.A: Web server start error...");
                Sandbox.MySandboxGame.Log.WriteLineAndConsole(Utilities.GetExceptionString(ex));
                return false;
            }
        }
        public void Stop()
        {
            Sandbox.MySandboxGame.Log.WriteLineAndConsole("S.E.A: Web server Stop");
            isRun = false;
            Hubs.seaHub.context?.Dispose();
            Host?.Dispose();
            GlobalHost.DependencyResolver?.Dispose();
        }
        ~Server()
        {
            Stop();
        }
    }
}

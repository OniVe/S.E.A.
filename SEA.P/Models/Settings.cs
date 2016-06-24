using SEA.P.Web.Models;
using System;
using System.Text;
using System.Xml;

namespace SEA.P.Models
{
    public static class Settings
    {
        private static AsyncLock settingsRead = new AsyncLock();
        private static string GetAttributeValue( string key )
        {
            using (settingsRead.Lock())
            {
                try
                {
                    XmlDocument xDoc = new XmlDocument();
                    xDoc.Load("SEA.P.dll.config");
                    var portNode = xDoc.SelectSingleNode("//appSettings/add[@key='" + key + "']");
                    if (portNode == null)
                        Sandbox.MySandboxGame.Log.WriteLineAndConsole("S.E.A: Attribute <add key=\"" + key + "\"> not found in file \"SEA.P.dll.config\". Set on the default value.");
                    else
                    {
                        var portAttribute = portNode.Attributes["value"];
                        return portAttribute == null ? null : portAttribute.Value;
                    }
                }
                catch (Exception ex)
                {
                    Sandbox.MySandboxGame.Log.WriteLineAndConsole(Utilities.GetExceptionString(ex));
                }
            }
            return null;
        }
        public static int ServerPort
        {
            get
            {
                int value = 80;
                int.TryParse(GetAttributeValue("port"), out value);
                return value == 0 ? 80 : value;
            }
        }

        public static bool launchBrowserOnStartup
        {
            get
            {
                var value = GetAttributeValue("launchBrowserOnStartup");
                return string.IsNullOrEmpty(value) ? false : value.ToLower() == "true";
            }
        }
    }
}

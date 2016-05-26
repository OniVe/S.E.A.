using Microsoft.AspNet.SignalR;
using Microsoft.Owin;
using Nancy;
using Nancy.Bootstrapper;
using Nancy.Owin;
using Nancy.TinyIoc;
using Owin;

[assembly: OwinStartup(typeof(SEA.P.Web.Startup))]
namespace SEA.P.Web
{
    public class Startup
    {
        public void Configuration( IAppBuilder app )
        {
            GlobalHost.Configuration.DisconnectTimeout = System.TimeSpan.FromSeconds(8);
            GlobalHost.Configuration.ConnectionTimeout = System.TimeSpan.FromSeconds(24);
            GlobalHost.Configuration.MaxIncomingWebSocketMessageSize = 4194304;// 4MB

            //SignalR

            var hubConfiguration = new HubConfiguration();
            hubConfiguration.EnableJSONP = true;
            hubConfiguration.EnableDetailedErrors = false;
            app.MapSignalR(hubConfiguration);

            //Nancy

            app.UseNancy(options =>
              options.PerformPassThrough = context =>
                  context.Response.StatusCode == HttpStatusCode.NotFound);
        }
    }

    public class Bootstrapper : DefaultNancyBootstrapper
    {
        private byte[] favicon;
        protected override byte[] FavIcon => this.favicon ?? (this.favicon = LoadFavIcon());
        private byte[] LoadFavIcon()
        {
            using (var resourceStream = GetType().Assembly.GetManifestResourceStream("SEA.P.Web.favicon.ico"))
            {
                var tempFavicon = new byte[resourceStream.Length];
                resourceStream.Read(tempFavicon, 0, (int)resourceStream.Length);
                return tempFavicon;
            }
        }

        protected override void ApplicationStartup( TinyIoCContainer container, IPipelines pipelines )
        {
            // Enable Compression with Settings
            var settings = new GzipCompressionSettings();
            settings.MinimumBytes = 1024;
            //settings.MimeTypes.Add("application/vnd.myexample");
            //pipelines.EnableGzipCompression(settings);

            // Enable Compression with Default Settings
            pipelines.EnableGzipCompression(settings);

            base.ApplicationStartup(container, pipelines);
        }
    }
}

using Nancy;

namespace SEA.P.Web.Modules
{
    public class IndexModule : NancyModule
    {
        public IndexModule()
        {
            Get["/"] = _ => new ContentManager.Content("index.html", "html", true);
        }
    }
    public class ContentModule : NancyModule
    {
        public ContentModule()
        {
            Get["/res/"] = p => new ContentManager.DirectoryContent("json", this.Request.Query["pattern"], this.Request.Query["command"]);
            Get["/res/{name}.{ext}"] = p => new ContentManager.Content(p.name + "." + p.ext, p.ext, true);
        }
    }
}

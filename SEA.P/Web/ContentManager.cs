using Nancy;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SEA.P.Web
{
    public class ContentManager
    {
        private static readonly char[] invalidPathChars = Path.GetInvalidPathChars();
        private const string resourcePathTemplate = @"SEA.P.Web.Content.{0}.{1}";
        private const string resourceDirectoryPathTemplate = @"SEA.P.Web.Content.{0}.";
        private const string filePathTemplate = @"\web\{0}\{1}";
        private const string directoryPathTemplate = @"\web\{0}\";
        private readonly string fullFilePathTemplate;
        private readonly string fullDirectoryPathTemplate;
        private static ContentManager self;
        private Assembly assembly;

        public static ContentManager Static
        {
            get
            {
                if (self == null)
                    self = new ContentManager();

                return self;
            }
        }
        public ContentManager()
        {
            assembly = Assembly.GetExecutingAssembly();
            fullFilePathTemplate = Environment.CurrentDirectory + filePathTemplate;
            fullDirectoryPathTemplate = Environment.CurrentDirectory + directoryPathTemplate;
        }
        private void GetTypeFromExtension( string ext, out FileType fileType, out string subDirectory )
        {
            ext = ext.ToLower();
            switch (ext)
            {
                case "html": fileType = FileType.HTML; subDirectory = "html"; break;
                case "js": fileType = FileType.JS; subDirectory = "script"; break;
                case "css": fileType = FileType.CSS; subDirectory = "css"; break;
                case "json": fileType = FileType.JSON; subDirectory = "data"; break;
                case "png": fileType = FileType.IMAGE; subDirectory = "img"; break;
                case "jpg": fileType = FileType.IMAGE; subDirectory = "img"; break;
                case "jpeg": fileType = FileType.IMAGE; subDirectory = "img"; break;
                case "gif": fileType = FileType.IMAGE; subDirectory = "img"; break;
                case "svg": fileType = FileType.IMAGE; subDirectory = "img"; break;
                case "ttf": fileType = FileType.FONT; subDirectory = "font"; break;
                case "otf": fileType = FileType.FONT; subDirectory = "font"; break;
                case "woff": fileType = FileType.FONT; subDirectory = "font"; break;
                default: fileType = FileType.VOID; subDirectory = string.Empty; break;
            }
        }
        private string GetContentTypeFromExtension( string ext )
        {
            switch (ext.ToLower())
            {
                case "html": return "text/html";
                case "xml": return "text/xml";
                case "css": return "text/css";
                case "js": return "application/javascript";
                case "json": return "application/json";
                case "png": return "image/png";
                case "jpg": return "image/jpeg";
                case "jpeg": return "image/jpeg";
                case "gif": return "image/gif";
                case "svg": return "image/svg+xml";
                case "ttf": return "application/x-font-ttf";
                case "otf": return "application/x-font-opentype";
                case "woff": return "application/font-woff";
                default: return "text/html";
            }
        }
        private string GetExtension( ref string name )
        {
            int x = name.Length -1;
            int i = name.LastIndexOf('.') +1;
            if (i > 0 && i < x)
                return name.Substring(i);

            return string.Empty;
        }
        private bool TryGetResourceStream( string path, out Stream stream )
        {
            stream = assembly.GetManifestResourceStream(path);
            return stream != null;
        }
        private List<string> GetResourceNames( string path, string pattern )
        {
            var list_1 = assembly.GetManifestResourceNames().ToList();
            var list_2 = new List<string>();
            int i = 0, l = list_1.Count, L = path.Length;
            for (; i < l; ++i)
                if (list_1[i].StartsWith(path))
                    list_2.Add(list_1[i].Substring(L));

            if (string.IsNullOrEmpty(pattern))
                return list_2;

            System.Text.RegularExpressions.Regex mask = new System.Text.RegularExpressions.Regex(pattern.Replace(".", "[.]").Replace("*", ".*").Replace("?", "."));

            list_1.Clear();
            for (i = 0, l = list_2.Count; i < l; ++i)
                if (mask.IsMatch(list_2[i]))
                    list_1.Add(list_2[i]);

            return list_1;
        }

        private enum FileType : byte
        {
            VOID = 0,
            HTML = 1,
            JS = 2,
            CSS = 3,
            IMAGE = 4,
            FONT = 5,
            JSON = 6
        }
        private static bool fileNameIsValid( string name )
        {
            if (string.IsNullOrEmpty(name))
                return true;

            int i = name.Length;
            while (i-- > 0)
                if (invalidPathChars.Contains(name[i]))
                    return false;

            return true;
        }

        public class Content : Response
        {
            public Content( string name, string ext, bool createIfFileNotExist = false )
            {
                ContentType = Static.GetContentTypeFromExtension(ext);
                StatusCode = HttpStatusCode.OK;
                Contents = stream =>
                {
                    if (fileNameIsValid(name))
                    {
                        FileType fileType;
                        string subDirectory;
                        Static.GetTypeFromExtension(ext, out fileType, out subDirectory);
                        if (fileType != FileType.VOID)
                            try
                            {
                                FileInfo file = new FileInfo(string.Format(Static.fullFilePathTemplate, subDirectory, name));
                                if (file.Exists)
                                    using (var fileStream = file.OpenRead())
                                        fileStream.CopyTo(stream);

                                else
                                {
                                    Stream resStream;
                                    if (Static.TryGetResourceStream(string.Format(resourcePathTemplate, subDirectory, name), out resStream))
                                    {
                                        if (createIfFileNotExist)
                                        {
                                            file.Directory.Create();
                                            using (FileStream fileStream = file.Create())
                                            {
                                                resStream.Seek(0, SeekOrigin.Begin);
                                                resStream.CopyTo(fileStream);
                                                resStream.Seek(0, SeekOrigin.Begin);
                                            }
                                        }
                                        resStream.CopyTo(stream);
                                    }
                                }
                            }
                            catch
                            {
                                this.StatusCode = HttpStatusCode.InternalServerError;
                            }
                    }
                    else
                        this.StatusCode = HttpStatusCode.BadRequest;
                };
            }
        }
        public class DirectoryContent : Response
        {
            public DirectoryContent( string ext, string pattern, string command )
            {
                bool createIfFileNotExist = command == "createifnotexists";
                ContentType = Static.GetContentTypeFromExtension("json");
                StatusCode = HttpStatusCode.OK;
                Contents = stream =>
                {
                    if (fileNameIsValid(pattern))
                    {
                        FileType fileType;
                        string subDirectory;
                        Static.GetTypeFromExtension(ext, out fileType, out subDirectory);
                        DirectoryInfo directory = new DirectoryInfo(string.Format(Static.fullDirectoryPathTemplate, subDirectory));
                        StringBuilder responseText = new StringBuilder();
                        try
                        {
                            if (createIfFileNotExist)
                            {
                                if (!directory.Exists)
                                    directory.Create();

                                Stream resStream;
                                FileInfo file;
                                var resourceNames = Static.GetResourceNames(string.Format(resourceDirectoryPathTemplate, subDirectory), pattern);
                                for (var i = 0; i < resourceNames.Count; ++i)
                                {
                                    file = new FileInfo(string.Format(Static.fullFilePathTemplate, subDirectory, resourceNames[i]));
                                    if (file.Exists) continue;

                                    if (Static.TryGetResourceStream(string.Format(resourcePathTemplate, subDirectory, resourceNames[i]), out resStream))
                                        using (FileStream fileStream = file.Create())
                                        {
                                            resStream.Seek(0, SeekOrigin.Begin);
                                            resStream.CopyTo(fileStream);
                                        }
                                }
                                responseText.Append("true");
                            }
                            else
                            {
                                responseText.Append("[");
                                if (directory.Exists)
                                {
                                    FileInfo[] files = string.IsNullOrWhiteSpace(pattern) ? directory.GetFiles() : directory.GetFiles(pattern);
                                    for (var i = 0; i < files.Length; ++i)
                                        responseText
                                            .Append(i == 0 ? "\"" : ",\"")
                                            .Append(files[i].Name)
                                            .Append("\"");
                                }
                                else
                                {
                                    var resourceNames = Static.GetResourceNames(string.Format(resourceDirectoryPathTemplate, subDirectory), pattern);
                                    for (var i = 0; i < resourceNames.Count; ++i)
                                        responseText
                                            .Append(i == 0 ? "\"" : ",\"")
                                            .Append(resourceNames[i])
                                            .Append("\"");
                                }
                                responseText.Append("]");
                            }
                        }
                        catch
                        {
                            responseText.Clear();
                            responseText.Append("false");
                            StatusCode = HttpStatusCode.InternalServerError;
                        }
                        byte[] byteArray = Encoding.UTF8.GetBytes(responseText.ToString());
                        stream.Write(byteArray, 0, byteArray.Length);
                    }
                    else
                        StatusCode = HttpStatusCode.BadRequest;
                };
            }
        }
    }
}

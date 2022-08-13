using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Blish_HUD;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Nekres.Special_Forces.Core.Services.Persistance
{
    internal class TemplateReader
    {
        private readonly IsoDateTimeConverter _dateFormat;

        public TemplateReader()
        {
            _dateFormat = new IsoDateTimeConverter {DateTimeFormat = "dd/MM/yyyy"};
        }

        private bool IsLocalPath(string p)
        {
            return new Uri(p).IsFile;
        }

        internal List<RawTemplate> LoadMultiple(string uri)
        {
            StreamReader reader;
            string objText;

            if (IsLocalPath(uri))
                using (reader = new StreamReader(uri))
                {
                    objText = reader.ReadToEnd();
                    return JsonConvert.DeserializeObject<List<RawTemplate>>(objText, _dateFormat);
                }

            var request = (HttpWebRequest) WebRequest.Create(uri);
            using (var response = (HttpWebResponse) request.GetResponse())
            {
                using (reader = new StreamReader(response.GetResponseStream()))
                {
                    objText = reader.ReadToEnd();
                    return JsonConvert.DeserializeObject<List<RawTemplate>>(objText, _dateFormat);
                }
            }
        }

        internal RawTemplate LoadSingle(string uri)
        {
            StreamReader reader;
            string objText;

            if (IsLocalPath(uri))
                using (reader = new StreamReader(uri))
                {
                    objText = reader.ReadToEnd();

                    return JsonConvert.DeserializeObject<RawTemplate>(objText, _dateFormat);
                }

            var request = (HttpWebRequest) WebRequest.Create(uri);
            using (var response = (HttpWebResponse) request.GetResponse())
            {
                using (reader = new StreamReader(response.GetResponseStream()))
                {
                    objText = reader.ReadToEnd();
                    return JsonConvert.DeserializeObject<RawTemplate>(objText, _dateFormat);
                }
            }
        }

        internal List<RawTemplate> LoadDirectory(string path)
        {
            var loaded = Directory.GetFiles(path, "*.json", SearchOption.TopDirectoryOnly);
            var files = new List<RawTemplate>();
            foreach (var file in loaded)
            {
                files.Add(LoadSingle(file));
            }
            return files;
        }
    }
}
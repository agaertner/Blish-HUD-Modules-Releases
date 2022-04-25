using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Nekres.Musician.Core.Models;
using Nekres.Musician.UI;

namespace Nekres.Musician
{
    internal class MusicSheetImporter : IDisposable
    {
        private readonly FileSystemWatcher _xmlWatcher;

        private readonly MusicSheetService _sheetService;

        private readonly IProgress<string> _loadingIndicator;

        public bool IsLoading { get; private set; }

        public string Log { get; private set; }

        public MusicSheetImporter(MusicSheetService sheetService, IProgress<string> loadingIndicator)
        {
            _sheetService = sheetService;
            _loadingIndicator = loadingIndicator;
            _xmlWatcher = new FileSystemWatcher(sheetService.CacheDir)
            {
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName,
                Filter = "*.xml",
                EnableRaisingEvents = true
            };
            _xmlWatcher.Created += OnXmlCreated;
        }

        private async void OnXmlCreated(object sender, FileSystemEventArgs e) => await ConvertXml(e.FullPath);

        public void Init()
        {
            var thread = new Thread(LoadSheetsInBackground)
            {
                IsBackground = true
            };

            thread.Start();
        }

        private async void LoadSheetsInBackground()
        {
            this.IsLoading = true;
            var initialFiles = Directory.EnumerateFiles(_sheetService.CacheDir).Where(s => Path.GetExtension(s).Equals(".xml"));
            foreach (var filePath in initialFiles) await ConvertXml(filePath, true);
            this.IsLoading = false;
            this.Log = null;
            _loadingIndicator.Report(null);
        }

        private async Task ConvertXml(string filePath, bool silent = false)
        {
            var log = $"Importing {Path.GetFileName(filePath)}..";
            System.Diagnostics.Debug.WriteLine(log);
            MusicianModule.Logger.Info(log);
            this.Log = log;
            _loadingIndicator.Report(log);
            var musicSheet = MusicSheet.FromXml(filePath);
            if (musicSheet == null) return;
            await FileUtil.DeleteAsync(filePath);
            await _sheetService.AddOrUpdate(musicSheet, silent);
        }

        public void Dispose()
        {
            _xmlWatcher.Created -= OnXmlCreated;
            _xmlWatcher.Changed -= OnXmlCreated;
            _xmlWatcher.Dispose();
            _xmlWatcher?.Dispose();
            _sheetService?.Dispose();
        }
    }
}

using Blish_HUD;
using Blish_HUD.Content;
using Nekres.Screenshot_Manager.Properties;
using Nekres.Screenshot_Manager.UI.Controls;
using Nekres.Screenshot_Manager.UI.Models;
using Nekres.Screenshot_Manager.UI.Views;
using Nekres.Screenshot_Manager_Module.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Nekres.Screenshot_Manager.Core
{
    public class FileWatcherFactory : IDisposable
    {
        public event EventHandler<ValueEventArgs<string>> FileAdded;
        public event EventHandler<ValueEventArgs<string>> FileDeleted;
        public event EventHandler<ValueChangedEventArgs<string>> FileRenamed;

        public const int NewFileNotificationDelay = 300;

        private string[] _imageFilters;

        private List<FileSystemWatcher> _screensPathWatchers;

        private readonly List<string> _index;
        public IReadOnlyList<string> Index => new List<string>(_index);

        public FileWatcherFactory()
        {
            _index = new List<string>();
            _screensPathWatchers = new List<FileSystemWatcher>();
            _imageFilters = new[] { "*.bmp", "*.jpg", "*.png" };

            foreach (var filter in _imageFilters)
            {
                var watcher = new FileSystemWatcher
                {
                    Path = DirectoryUtil.ScreensPath,
                    NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName,
                    Filter = filter,
                    EnableRaisingEvents = true
                };
                watcher.Created += OnScreenShotCreated;
                watcher.Deleted += OnScreenShotDeleted;
                watcher.Renamed += OnScreenShotRenamed;
                watcher.EnableRaisingEvents = true;
                _screensPathWatchers.Add(watcher);
            }

            var initialFiles = Directory.EnumerateFiles(DirectoryUtil.ScreensPath)
                                                        .Where(s => Array.Exists(_imageFilters, filter => filter.Equals('*' + Path.GetExtension(s), StringComparison.InvariantCultureIgnoreCase)))
                                                        .Select(x => Path.Combine(DirectoryUtil.ScreensPath, x));
            _index.AddRange(initialFiles);
        }

        private async void OnScreenShotCreated(object sender, FileSystemEventArgs e)
        {
            _index.Add(e.FullPath);
            await ScreenShotNotify(e.FullPath);
            FileAdded?.Invoke(this, new ValueEventArgs<string>(e.FullPath));

        }

        private void OnScreenShotDeleted(object sender, FileSystemEventArgs e)
        {
            _index.Remove(e.FullPath);
            FileDeleted?.Invoke(this, new ValueEventArgs<string>(e.FullPath));
        }

        private void OnScreenShotRenamed(object sender, RenamedEventArgs e)
        {
            _index.Remove(e.OldFullPath);
            _index.Add(e.FullPath);
            FileRenamed?.Invoke(this, new ValueChangedEventArgs<string>(e.OldFullPath, e.FullPath));
        }

        private async Task ScreenShotNotify(string filePath)
        {
            if (!ScreenshotManagerModule.ModuleInstance.MuteSound.Value) ScreenshotManagerModule.ModuleInstance.ScreenShotSfx.Play(GameService.GameIntegration.Audio.Volume,0,0);
            if (ScreenshotManagerModule.ModuleInstance.DisableNotification.Value) return;
            // Delaying so created file handle is closed (write completed) before we look at the directory for its newest file.
            await Task.Delay(NewFileNotificationDelay).ContinueWith(async delegate
            {
                var timeout = DateTime.UtcNow.AddMilliseconds(ScreenshotManagerModule.FileTimeOutMilliseconds);
                while (DateTime.UtcNow < timeout)
                {
                    try
                    {
                        var texture = new AsyncTexture2D();
                        ScreenshotNotification.ShowNotification(texture, filePath, Resources.Screenshot_Created_, 5.0f, () => OpenInspectionPanel(filePath));
                        await TextureUtil.GetThumbnail(filePath).ContinueWith(t => texture.SwapTexture(t.Result));
                        break;
                    }
                    catch (InvalidOperationException ex)
                    {
                        if (DateTime.UtcNow < timeout) continue;
                        ScreenshotManagerModule.Logger.Error(ex.Message);
                        return;
                    }
                }

            });
        }

        private async void OpenInspectionPanel(string filePath)
        {
            await CreateInspectionPanel(filePath);
            GameService.Overlay.BlishHudWindow.Show();
            GameService.Overlay.BlishHudWindow.Navigate(new ScreenshotManagerView(new ScreenshotManagerModel(this)));
        }

        public async Task CreateInspectionPanel(string filePath)
        {
            if (!File.Exists(filePath)) return;
            var texture = new AsyncTexture2D();
            var inspect = new InspectPanel(texture, Path.GetFileNameWithoutExtension(filePath));
            await TextureUtil.GetScreenShot(filePath).ContinueWith(t => texture.SwapTexture(t.Result));
        }

        public void Dispose()
        {
            foreach (var watcher in _screensPathWatchers)
            {
                watcher.Created -= OnScreenShotCreated;
                watcher.Deleted -= OnScreenShotDeleted;
                watcher.Renamed -= OnScreenShotRenamed;
                watcher.Dispose();
            }
        }
    }
}

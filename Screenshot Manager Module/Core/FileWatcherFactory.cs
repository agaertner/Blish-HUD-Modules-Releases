using Blish_HUD;
using Microsoft.Xna.Framework.Graphics;
using Nekres.Screenshot_Manager.Properties;
using Nekres.Screenshot_Manager_Module.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using Microsoft.Xna.Framework;
using Nekres.Screenshot_Manager.UI.Models;
using Nekres.Screenshot_Manager.UI.Views;

namespace Nekres.Screenshot_Manager.Core
{
    internal class FileWatcherFactory : IDisposable
    {
        public const int NewFileNotificationDelay = 300;

        private string[] _imageFilters;

        private List<FileSystemWatcher> _screensPathWatchers;

        private AsyncCache<string, Texture2D> _cache;
        private List<string> _index;

        public FileWatcherFactory()
        {
            _cache = new AsyncCache<string, Texture2D>(GetScreenShot);
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
        }

        private async void OnScreenShotCreated(object sender, FileSystemEventArgs e)
        {
            _index.Add(e.FullPath);
            await ScreenShotNotify(e.FullPath);

        }

        private void OnScreenShotDeleted(object sender, FileSystemEventArgs e)
        {
            _index.Remove(e.FullPath);
        }

        private void OnScreenShotRenamed(object sender, RenamedEventArgs e)
        {
            _index.Remove(e.OldFullPath);
            _index.Add(e.FullPath);
        }

        private async Task ScreenShotNotify(string filePath)
        {
            // Delaying so created file handle is closed (write completed) before we look at the directory for its newest file.
            await Task.Delay(NewFileNotificationDelay).ContinueWith(async delegate
            {
                var timeout = DateTime.UtcNow.AddMilliseconds(ScreenshotManagerModule.FileTimeOutMilliseconds);
                while (DateTime.UtcNow < timeout)
                {
                    try
                    {
                        var thumb = new AsyncTexture2D();
                        await TextureUtil.GetThumbnail(filePath, 270, 170).ContinueWith(x => thumb.SwapTexture(x.Result));
                        ScreenshotNotification.ShowNotification(thumb, Resources.Screenshot_Created_, 5.0f, () => OpenInspectionPanel(filePath));
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

        private async Task<Texture2D> GetScreenShot(string filePath)
        {
            return null;
        }

        private async void OpenInspectionPanel(string filePath)
        {
            var texture = await _cache.GetItem(filePath);
            CreateInspectionPanel(texture);
            GameService.Overlay.BlishHudWindow.Show();
            GameService.Overlay.BlishHudWindow.Navigate(new ScreenshotManagerView(new ScreenshotManagerModel()));
        }

        public void CreateInspectionPanel(Texture2D texture)
        {
            if (texture == null) return;
            var panel = new Panel
            {
                Parent = GameService.Graphics.SpriteScreen,
                Size = new Point(texture.Width + 10, texture.Height + 10),
                Location = new Point(GameService.Graphics.SpriteScreen.Width / 2 - texture.Width / 2, GameService.Graphics.SpriteScreen.Height / 2 - texture.Height / 2),
                BackgroundColor = Color.Black,
                ZIndex = 9999,
                ShowTint = true,
                Opacity = 0.0f
            };
            var image = new Image
            {
                Parent = panel,
                Location = new Point(5, 5),
                Size = new Point(texture.Width, texture.Height),
                Texture = texture
            };
            GameService.Animation.Tweener.Tween(panel, new { Opacity = 1.0f }, 0.35f);
            image.Click += (o, e) => GameService.Animation.Tweener.Tween(panel, new { Opacity = 0.0f }, 0.15f).OnComplete(() => panel?.Dispose());
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

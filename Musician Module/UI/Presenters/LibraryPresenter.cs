using Blish_HUD;
using Blish_HUD.Graphics.UI;
using Nekres.Musician.Controls;
using Nekres.Musician.UI.Models;
using Nekres.Musician.UI.Views;
using System;
using System.Linq;
using Blish_HUD.Controls;
using Nekres.Musician.Core.Models;

namespace Nekres.Musician.UI.Presenters
{
    internal class LibraryPresenter : Presenter<LibraryView, LibraryModel>
    {
        public LibraryPresenter(LibraryView view, LibraryModel model) : base(view, model)
        {
            view.OnSelectedSortChanged += View_SelectedSortChanged;
            view.OnImportFromClipboardClick += View_ImportFromClipboardClicked;
            model.MusicSheetService.OnSheetUpdated += OnSheetUpdated;
        }

        protected override void Unload()
        {
            this.View.OnSelectedSortChanged -= View_SelectedSortChanged;
            this.View.OnImportFromClipboardClick -= View_ImportFromClipboardClicked;
            this.Model.MusicSheetService.OnSheetUpdated -= OnSheetUpdated;
            base.Unload();
        }

        private void OnSheetUpdated(object o, ValueEventArgs<MusicSheetModel> e)
        {
            if (!TryGetSheetButtonById(e.Value.Id, out var button))
            {
                this.View.CreateSheetButton(e.Value);
                return;
            }
            button.Artist = e.Value.Artist;
            button.Title = e.Value.Title;
            button.User = e.Value.User;
        }
        
        private bool TryGetSheetButtonById(Guid id, out SheetButton button)
        {
            button = this.View.MelodyFlowPanel.Children.Where(x => x.GetType() == typeof(SheetButton)).Cast<SheetButton>().FirstOrDefault(y => y.Id.Equals(id));
            if (button == null) return false;
            return true;
        }

        private void View_SelectedSortChanged(object o, ValueEventArgs<string> e)
        {
            this.View.MelodyFlowPanel?.SortChildren<SheetButton>((x, y) =>
            {
                var isInstrument = Enum.TryParse<Instrument>(e.Value, true, out _);
                x.Visible = !isInstrument || x.Instrument.ToString().Equals(e.Value, StringComparison.InvariantCultureIgnoreCase);
                y.Visible = !isInstrument || y.Instrument.ToString().Equals(e.Value, StringComparison.InvariantCultureIgnoreCase);

                if (!x.Visible || !y.Visible) return 0;

                if (this.Model.DD_TITLE.Equals(e.Value))
                    return string.Compare(x.Title, y.Title, StringComparison.InvariantCulture);
                if (this.Model.DD_ARTIST.Equals(e.Value))
                    return string.Compare(x.Artist, y.Artist, StringComparison.InvariantCulture);
                if (this.Model.DD_USER.Equals(e.Value))
                    return string.Compare(x.User, y.User, StringComparison.InvariantCulture);
                return 0;
            });
        }

        private async void View_ImportFromClipboardClicked(object o, EventArgs e)
        {
            var xml = await ClipboardUtil.WindowsClipboardService.GetTextAsync();
            if (!MusicSheet.TryParseXml(xml, out var sheet))
            {
               GameService.Content.PlaySoundEffectByName("error");
               ScreenNotification.ShowNotification("Your clipboard does not contain a valid music sheet.", ScreenNotification.NotificationType.Error);
               return;
            }
            await MusicianModule.ModuleInstance.MusicSheetService.AddOrUpdate(sheet);
            this.View.CreateSheetButton(sheet.ToModel());
        }
    }
}

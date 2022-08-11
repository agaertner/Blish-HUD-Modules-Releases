namespace Nekres.Music_Mixer.Core.UI.Models
{
    internal class ConfigModel : MainModel
    {
        public readonly MusicContextModel MusicContextModel;

        public ConfigModel(MusicContextModel model) : base(model.State)
        {
            this.MusicContextModel = model;
        }
    }
}

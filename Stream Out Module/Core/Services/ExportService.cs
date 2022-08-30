using System;
using System.Threading.Tasks;

namespace Nekres.Stream_Out.Core.Services
{
    internal abstract class ExportService : IDisposable
    {
        private DateTime _prevApiRequestTime;

        protected ExportService()
        {
            _prevApiRequestTime = DateTime.UtcNow;
        }

        public async Task DoUpdate()
        {
            await DoResetDaily().ContinueWith(async _ =>
            {
                if (DateTime.UtcNow.Subtract(_prevApiRequestTime).TotalSeconds < 300) return;
                _prevApiRequestTime = DateTime.UtcNow;
                await this.Update();
            });
        }

        private async Task DoResetDaily()
        {
            if (DateTime.UtcNow < StreamOutModule.Instance.ResetTimeDaily.Value) return;
            StreamOutModule.Instance.ResetTimeDaily.Value = Gw2Util.GetDailyResetTime();
            await this.ResetDaily();
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public virtual async Task Initialize() { /* NOOP */ }

        protected virtual async Task Update() { /* NOOP */ }

        protected virtual async Task ResetDaily() { /* NOOP */ }
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously

        public abstract Task Clear();

        public abstract void Dispose();
    }
}

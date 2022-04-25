using System;
using System.Threading.Tasks;

namespace Nekres.Stream_Out.Core.Services
{
    internal interface IExportService : IDisposable
    {
        Task Update();

        Task Initialize();

        Task ResetDaily();
    }
}

using System;
using System.Threading.Tasks;
using Dapr.Actors;

namespace demo.interfaces
{
    public interface IMyActor : IActor
    {
        Task<string> SetDataAsync(MyData data);
        Task<MyData> GetDataAsync();
        Task RegisterReminder();
        Task UnregisterReminder();
        Task RegisterTimer();
        Task UnregisterTimer();
    }
}

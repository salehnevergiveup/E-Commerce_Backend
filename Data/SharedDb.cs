using PototoTrade.Models;
//using SignalR.Models;
using System.Collections.Concurrent;

namespace PototoTrade.Models
{
    public class SharedDb
    {
        public readonly ConcurrentDictionary<string, UserAccount> connections = new ConcurrentDictionary<string, UserAccount>();
    }
}

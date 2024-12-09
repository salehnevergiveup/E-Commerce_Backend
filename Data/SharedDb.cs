using PototoTrade.Models;
using PototoTrade.Models.User;

//using SignalR.Models;
using System.Collections.Concurrent;

namespace PototoTrade.Data
{
    public class SharedDb
    {
        public readonly ConcurrentDictionary<string, UserAccount> connections = new ConcurrentDictionary<string, UserAccount>();
    }
}

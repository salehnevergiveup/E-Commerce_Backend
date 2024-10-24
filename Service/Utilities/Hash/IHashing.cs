using System;

namespace PototoTrade.Service.Utilites.Hash;

public interface IHashing
{
    public string Hash (string input);  

    public bool Verify(string hash, string input);

}

using System;
using PototoTrade.Service.Utilites.Hash;

namespace PototoTrade.Data.Seeders;

public abstract class Seeder
{
   protected DBC _dataContext;
   protected IHashing?  _hash;  

    protected Seeder(DBC _dataContext , IHashing hash) 
    {
        this._dataContext = _dataContext;
        this._hash  = hash; 
    }

    protected Seeder(DBC _dataContext) 
    {
        this._dataContext = _dataContext;
    }

    public abstract void seed();
} 

//sudo sysctl -w fs.inotify.max_user_instances=1024

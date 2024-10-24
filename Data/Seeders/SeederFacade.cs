using PototoTrade.Service.Utilites.Hash;


namespace PototoTrade.Data.Seeders;

public class SeederFacade
{
   private readonly DBC _dataContext;

    public SeederFacade(DBC dataContext) {  
     this._dataContext = dataContext; 
    }
    public void SeedInitialData() {  
        IHashing hash = new Hashing ();  
        new SystemInti(_dataContext, hash).seed(); 
    }

}

namespace API.Models
{
    [Flags]
    public enum SalesType
    {
        None = 0,
        Store = 1,       // Mağaza
        Mobile = 2,      // Seyyar
        Online = 4       // İnternet
    }
}

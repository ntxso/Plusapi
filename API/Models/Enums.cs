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

    public enum OrderStatus
    {
        Pending = 0,      // Beklemede (örneğin ödeme bekleniyor)
        Completed = 1,    // Tamamlandı
        Cancelled = 2     // İptal edildi
    }
}

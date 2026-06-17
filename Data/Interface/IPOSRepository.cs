using Data.Entities;

namespace Data.Interface;

public interface IPOSRepository
{
    public Task<List<Sale>> GetSales();
    public Task<bool> CreateSale(Sale? sale);
    public Task<bool> UpdateSale(Sale? sale);
    public Task<bool> DeleteSale(long id);
    public Task<Sale?> GetSaleById(long Id);
    public Task<bool> ArchiveSale(long? id);
}

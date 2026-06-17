using Data.Entities;
using Data.Interface;
using Microsoft.EntityFrameworkCore;

namespace Data.Repository;

public class POSRepository : IPOSRepository
{
    private readonly AppDbContext _db;

    public POSRepository(AppDbContext db) => _db = db;

    public Task<List<Sale>> GetSales() => _db.Sales.ToListAsync();

    public async Task<bool> CreateSale(Sale? sale)
    {
        if (sale == null)
        {
            return false;
        }
        _db.Sales.Add(sale);
        int recordsCreated = await _db.SaveChangesAsync();
        if (recordsCreated > 0)
            return true;
        return false;
    }

    public async Task<bool> UpdateSale(Sale? sale)
    {
        if (sale == null)
        {
            return false;
        }
        sale.LastModifiedBy = 1;
        sale.LastModifiedOn = DateTime.UtcNow;
        _db.Sales.Update(sale);
        int recordsUpdated = await _db.SaveChangesAsync();
        if (recordsUpdated > 0)
            return true;
        return false;
    }

    public async Task<bool> DeleteSale(long id)
    {
        var sale = await _db.Sales.FindAsync(id);
        if (sale is not null)
        {
            _db.Sales.Remove(sale);
            int recordsDeleted = await _db.SaveChangesAsync();
            if (recordsDeleted > 0)
                return true;
            return false;
        }
        return false;
    }

    public async Task<bool> ArchiveSale(long? id)
    {
        if (id == null)
            return false;
        var sale = await _db.Sales.FindAsync(id);
        if (sale is not null)
        {
            sale.IsActive = false;
            sale.LastModifiedBy = 1;
            sale.LastModifiedOn = DateTime.UtcNow;
            _db.Sales.Update(sale);
            int recordsDeleted = await _db.SaveChangesAsync();
            if (recordsDeleted > 0)
                return true;
            return false;
        }
        return false;
    }

    public async Task<Sale?> GetSaleById(long Id)
    {
        Sale? sale = await _db.Sales.FindAsync(Id);
        return sale;
    }
}

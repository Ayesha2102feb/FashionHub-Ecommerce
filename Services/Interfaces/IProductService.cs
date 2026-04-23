using EcomWebsite.Models;

namespace EcomWebsite.Services.Interfaces
{
    public interface IProductService
    {
        List<Product> GetAll();
        Product GetById(int id);
        void Add(Product product);
        void Update(Product product);
        void Delete(int id);
    }
}

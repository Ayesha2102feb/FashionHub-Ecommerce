using EcomWebsite.Models;
using EcomWebsite.Repositories.Interfaces;
using EcomWebsite.Services.Interfaces;

namespace EcomWebsite.Services.Implementations
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _repo;

        public ProductService(IProductRepository repo)
        {
            _repo = repo;
        }

        public List<Product> GetAll() => _repo.GetAll();

        public Product GetById(int id) => _repo.GetById(id);

        public void Add(Product product) => _repo.Add(product);

        public void Update(Product product) => _repo.Update(product);

        public void Delete(int id) => _repo.Delete(id);
    }
}

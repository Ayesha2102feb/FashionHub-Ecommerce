using System.Collections.Generic;
using System.Linq;
using EcomWebsite.Models;
using EcomWebsite.Repositories.Interfaces;

namespace EcomWebsite.Repositories.Implementations
{
    public class ProductRepository : IProductRepository
    {
        private readonly myContext _context;

        public ProductRepository(myContext context)
        {
            _context = context;
        }

        public List<Product> GetAll()
        {
            return _context.tbl_product.ToList();
        }

        public Product? GetById(int id)
        {
            return _context.tbl_product.FirstOrDefault(x => x.product_id == id);
        }
        public void Add(Product product)
        {
            _context.tbl_product.Add(product);
            _context.SaveChanges();
        }

        public void Update(Product product)
        {
            _context.tbl_product.Update(product);
            _context.SaveChanges();
        }

        public void Delete(int id)
        {
            var product = _context.tbl_product.FirstOrDefault(x => x.product_id == id);
            if (product != null)
            {
                _context.tbl_product.Remove(product);
                _context.SaveChanges();
            }
        }
    }
}

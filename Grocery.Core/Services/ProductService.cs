using Grocery.Core.Interfaces.Repositories;
using Grocery.Core.Interfaces.Services;
using Grocery.Core.Models;
using System;
using System.Linq;

namespace Grocery.Core.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;

        public ProductService(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public List<Product> GetAll()
        {
            return _productRepository.GetAll();
        }

        public Product Add(Product item)
        {
            ArgumentNullException.ThrowIfNull(item);

            if (string.IsNullOrWhiteSpace(item.Name))
            {
                throw new ArgumentException("Product name is required.", nameof(item));
            }

            if (item.Stock < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(item.Stock), "Stock cannot be negative.");
            }

            if (item.Price < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(item.Price), "Price cannot be negative.");
            }

            bool productAlreadyExists = _productRepository
                .GetAll()
                .Any(p => string.Equals(p.Name.Trim(), item.Name.Trim(), StringComparison.OrdinalIgnoreCase));

            if (productAlreadyExists)
            {
                throw new InvalidOperationException($"A product with the name '{item.Name}' already exists.");
            }

            return _productRepository.Add(item);
        }

        public Product? Delete(Product item)
        {
            throw new NotImplementedException();
        }

        public Product? Get(int id)
        {
            throw new NotImplementedException();
        }

        public Product? Update(Product item)
        {
            return _productRepository.Update(item);
        }
    }
}

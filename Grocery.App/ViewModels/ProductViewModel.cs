using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using Grocery.App.Views;
using Grocery.Core.Interfaces.Services;
using Grocery.Core.Models;

namespace Grocery.App.ViewModels
{
    public class ProductViewModel : BaseViewModel
    {
        private readonly IProductService _productService;
        private readonly GlobalViewModel _global;
        private bool _canCreateProduct;
        public bool CanCreateProduct
        {
            get => _canCreateProduct;
            private set
            {
                if (SetProperty(ref _canCreateProduct, value))
                {
                    CreateProductCommand?.NotifyCanExecuteChanged();
                }
            }
        }

        public ObservableCollection<Product> Products { get; } 

        public IAsyncRelayCommand CreateProductCommand { get; }

        public ProductViewModel(IProductService productService, GlobalViewModel global)
        {
            _productService = productService;
            _global = global;
            Products = new ObservableCollection<Product>(); 
            foreach (Product p in _productService.GetAll()) Products.Add(p);

            CreateProductCommand = new AsyncRelayCommand(NavigateToNewProductAsync, () => CanCreateProduct);
            UpdateAccess();
        }

        public override void OnAppearing()
        {
            base.OnAppearing();
            UpdateAccess();
        }

        private void LoadProducts()
        {
            Products.Clear();

            foreach (Product product in _productService.GetAll())
            {
                Products.Add(product);
            }
        }

        private void UpdateAccess()
        {
            CanCreateProduct = _global.Client?.Role == Role.Admin;
        }

        private async Task NavigateToNewProductAsync()
        {
            if (!CanCreateProduct)
            {
                return;
            }

            await Shell.Current.GoToAsync(nameof(NewProductView), true);
        }
    }
}
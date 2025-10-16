
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Grocery.Core.Interfaces.Services;
using Grocery.Core.Models;
using System;
using System.Globalization;


namespace Grocery.App.ViewModels
{
    public partial class NewProductViewModel : BaseViewModel
    {
        private readonly IProductService _productService;
        private readonly GlobalViewModel _global;

        [ObservableProperty]
        private string name = string.Empty;

        [ObservableProperty]
        private string stock = "0";

        [ObservableProperty]
        private string price = "0";

        [ObservableProperty]
        private DateTime shelfLife = DateTime.Today;

        [ObservableProperty]
        private bool shelfLifeEnabled;

        [ObservableProperty]
        private string? errorMessage;

        [ObservableProperty]
        private bool hasError;

        [ObservableProperty]
        private Product? createdProduct;

        [ObservableProperty]
        private bool isAdmin;

        public NewProductViewModel(IProductService productService, GlobalViewModel global)
        {
            Title = "Nieuw product";
            _productService = productService;
            _global = global;
        }

        public override void OnAppearing()
        {
            base.OnAppearing();
            UpdateAccess();
            ResetForm();
        }

        [RelayCommand(CanExecute = nameof(IsAdmin))]
        private void Save()
        {
            if (!IsAdmin)
            {
                SetError("Alleen beheerders kunnen nieuwe producten aanmaken.");
                return;
            }

            string trimmedName = Name.Trim();
            if (string.IsNullOrWhiteSpace(trimmedName))
            {
                SetError("Naam is verplicht.");
                return;
            }

            if (!int.TryParse(Stock?.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int stockValue) || stockValue < 0)
            {
                SetError("Voorraad moet een positief geheel getal zijn.");
                return;
            }

            string priceInput = Price?.Trim() ?? string.Empty;
            if (!decimal.TryParse(priceInput, NumberStyles.Number, CultureInfo.CurrentCulture, out decimal priceValue) || priceValue < 0)
            {
                if (!decimal.TryParse(priceInput, NumberStyles.Number, CultureInfo.InvariantCulture, out priceValue) || priceValue < 0)
                {
                    SetError("Prijs moet een positief getal zijn.");
                    return;
                }
            }

            DateOnly shelfLifeValue = ShelfLifeEnabled
                ? DateOnly.FromDateTime(ShelfLife)
                : default;

            Product newProduct = new(0, trimmedName, stockValue, shelfLifeValue, priceValue);

            try
            {
                CreatedProduct = _productService.Add(newProduct);
                ClearError();
                ResetForm();
            }
            catch (Exception ex)
            {
                SetError(ex.Message);
                CreatedProduct = null;
            }
        }

        [RelayCommand]
        private void Reset()
        {
            ResetForm();
            ClearError();
        }

        private void UpdateAccess()
        {
            IsAdmin = _global.Client?.Role == Role.Admin;
        }

        partial void OnIsAdminChanged(bool value)
        {
            SaveCommand.NotifyCanExecuteChanged();
        }

        partial void OnErrorMessageChanged(string? value)
        {
            HasError = !string.IsNullOrWhiteSpace(value);
        }

        private void ResetForm()
        {
            Name = string.Empty;
            Stock = "0";
            Price = "0";
            ShelfLife = DateTime.Today;
            ShelfLifeEnabled = false;
        }

        private void SetError(string message)
        {
            ErrorMessage = message;
        }

        private void ClearError()
        {
            ErrorMessage = null;
        }
    }
}
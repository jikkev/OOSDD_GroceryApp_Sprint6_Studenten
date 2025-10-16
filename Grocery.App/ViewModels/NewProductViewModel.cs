using System;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Grocery.Core.Interfaces.Services;
using Grocery.Core.Models;

namespace Grocery.App.ViewModels;

public class NewProductViewModel : BaseViewModel
{
    private readonly IProductService _productService;
    private readonly GlobalViewModel _global;

    private string _name = string.Empty;
    private string _stock = "0";
    private string _price = "0";
    private DateTime _shelfLife = DateTime.Today;
    private bool _shelfLifeEnabled;
    private string? _errorMessage;
    private bool _hasError;
    private Product? _createdProduct;
    private bool _isAdmin;

    public NewProductViewModel(IProductService productService, GlobalViewModel global)
    {
        Title = "Nieuw product";
        _productService = productService;
        _global = global;

        SaveCommand = new RelayCommand(Save, () => IsAdmin);
        ResetCommand = new RelayCommand(Reset);
    }

    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    public string Stock
    {
        get => _stock;
        set => SetProperty(ref _stock, value);
    }

    public string Price
    {
        get => _price;
        set => SetProperty(ref _price, value);
    }

    public DateTime ShelfLife
    {
        get => _shelfLife;
        set => SetProperty(ref _shelfLife, value);
    }

    public bool ShelfLifeEnabled
    {
        get => _shelfLifeEnabled;
        set => SetProperty(ref _shelfLifeEnabled, value);
    }

    public string? ErrorMessage
    {
        get => _errorMessage;
        private set
        {
            if (SetProperty(ref _errorMessage, value))
            {
                HasError = !string.IsNullOrWhiteSpace(value);
            }
        }
    }

    public bool HasError
    {
        get => _hasError;
        private set => SetProperty(ref _hasError, value);
    }

    public Product? CreatedProduct
    {
        get => _createdProduct;
        private set => SetProperty(ref _createdProduct, value);
    }

    public bool IsAdmin
    {
        get => _isAdmin;
        private set
        {
            if (SetProperty(ref _isAdmin, value))
            {
                SaveCommand.NotifyCanExecuteChanged();
            }
        }
    }

    public IRelayCommand SaveCommand { get; }

    public IRelayCommand ResetCommand { get; }

    public override void OnAppearing()
    {
        base.OnAppearing();
        UpdateAccess();
        ResetForm();
    }

    private void Save()
    {
        if (!IsAdmin)
        {
            SetError("Alleen beheerders kunnen nieuwe producten aanmaken.");
            return;
        }

        string trimmedName = (Name ?? string.Empty).Trim();
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

    private void Reset()
    {
        ResetForm();
        ClearError();
    }

    private void UpdateAccess()
    {
        IsAdmin = _global.Client?.Role == Role.Admin;
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
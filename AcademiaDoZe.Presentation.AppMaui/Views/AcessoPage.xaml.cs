// João Gabriel Hensen dos Santos
using AcademiaDoZe.Presentation.AppMaui.ViewModels;

namespace AcademiaDoZe.Presentation.AppMaui.Views;

public partial class AcessoPage : ContentPage
{
    private readonly AcessoViewModel _viewModel;

    public AcessoPage(AcessoViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.InitializeAsync();
    }
}
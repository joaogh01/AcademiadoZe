using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Presentation.AppMaui.ViewModels;

namespace AcademiaDoZe.Presentation.AppMaui.Views;

public partial class LogradouroListPage : ContentPage
{
    public LogradouroListPage(LogradouroListViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is LogradouroListViewModel viewModel)
        {
            await viewModel.LoadLogradourosCommand.ExecuteAsync(null);
        }
    }

    private async void OnEditButtonClicked(object? sender, EventArgs e)
    {
        if (sender is Button button && button.BindingContext is LogradouroDto logradouro && BindingContext is LogradouroListViewModel viewModel)
        {
            await viewModel.EditLogradouroCommand.ExecuteAsync(logradouro);
        }
    }

    private async void OnDeleteButtonClicked(object? sender, EventArgs e)
    {
        if (sender is Button button && button.BindingContext is LogradouroDto logradouro && BindingContext is LogradouroListViewModel viewModel)
        {
            await viewModel.DeleteLogradouroCommand.ExecuteAsync(logradouro);
        }
    }
}
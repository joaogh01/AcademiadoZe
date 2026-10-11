using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Presentation.AppMaui.ViewModels;

namespace AcademiaDoZe.Presentation.AppMaui.Views;

public partial class ColaboradorListPage : ContentPage
{
    public ColaboradorListPage(ColaboradorListViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is ColaboradorListViewModel viewModel)
        {
            await viewModel.LoadColaboradoresAsync();
        }
    }

    private async void OnEditButtonClicked(object? sender, EventArgs e)
    {
        try
        {
            if (sender is Button button && button.BindingContext is ColaboradorDto colaborador && BindingContext is ColaboradorListViewModel viewModel)
            {
                await viewModel.EditColaboradorAsync(colaborador);
            }
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Erro", $"Erro ao editar colaborador: {ex.Message}", "OK");
        }
    }

    private async void OnDeleteButtonClicked(object? sender, EventArgs e)
    {
        try
        {
            if (sender is Button button && button.BindingContext is ColaboradorDto colaborador && BindingContext is ColaboradorListViewModel viewModel)
            {
                await viewModel.DeleteColaboradorAsync(colaborador);
            }
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Erro", $"Erro ao excluir colaborador: {ex.Message}", "OK");
        }
    }
}
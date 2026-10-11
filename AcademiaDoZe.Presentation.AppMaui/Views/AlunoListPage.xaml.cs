using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Presentation.AppMaui.ViewModels;

namespace AcademiaDoZe.Presentation.AppMaui.Views;

public partial class AlunoListPage : ContentPage
{
    public AlunoListPage(AlunoListViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
{
    base.OnAppearing();
    try
    {
        if (BindingContext is AlunoListViewModel viewModel)
        {
            await viewModel.LoadAlunosAsync();
        }
    }
    catch (Exception ex)
    {
        await Shell.Current.DisplayAlertAsync("Erro ao carregar alunos", ex.ToString(), "OK");
    }
}

    private async void OnEditButtonClicked(object? sender, EventArgs e)
    {
        try
        {
            if (sender is Button button && button.BindingContext is AlunoDto aluno && BindingContext is AlunoListViewModel viewModel)
            {
                await viewModel.EditAlunoAsync(aluno);
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Erro", $"Erro ao navegar para edição: {ex.Message}", "OK");
        }
    }

    private async void OnDeleteButtonClicked(object? sender, EventArgs e)
    {
        try
        {
            if (sender is Button button && button.BindingContext is AlunoDto aluno && BindingContext is AlunoListViewModel viewModel)
            {
                await viewModel.DeleteAlunoAsync(aluno);
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Erro", $"Erro ao excluir aluno: {ex.Message}", "OK");
        }
    }
}
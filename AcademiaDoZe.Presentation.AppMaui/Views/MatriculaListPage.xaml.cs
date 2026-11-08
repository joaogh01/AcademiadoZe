using AcademiaDoZe.Presentation.AppMaui.ViewModels;

namespace AcademiaDoZe.Presentation.AppMaui.Views;

public partial class MatriculaListPage : ContentPage
{
    private readonly MatriculaListViewModel _viewModel;

    public MatriculaListPage(MatriculaListViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    public MatriculaListPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is MatriculaListViewModel vm)
        {
            await vm.LoadMatriculasAsync();
        }
        else if (_viewModel != null)
        {
            await _viewModel.LoadMatriculasAsync();
        }
    }

    private async void OnNovaMatriculaClicked(object? sender, EventArgs e)
    {
        try
        {
            await Shell.Current.GoToAsync("matricula");
        }
        catch
        {
            await Navigation.PushAsync(new MatriculaPage());
        }
    }
}
using AcademiaDoZe.Presentation.AppMaui.ViewModels;

namespace AcademiaDoZe.Presentation.AppMaui.Views;

public partial class MatriculaPage : ContentPage
{
    public MatriculaPage()
    {
        InitializeComponent();
        var vm = IPlatformApplication.Current?.Services.GetService<MatriculaViewModel>();
        if (vm != null)
        {
            BindingContext = vm;
        }
    }

    public MatriculaPage(MatriculaViewModel viewModel) : this()
    {
        if (viewModel != null)
        {
            BindingContext = viewModel;
        }
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is MatriculaViewModel vm)
        {
            try
            {
                await vm.InitializeAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ERRO INITIALIZE VM]: {ex.Message}");
            }
        }
    }
}
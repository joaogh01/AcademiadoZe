using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Enums;
using AcademiaDoZe.Application.Interfaces;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace AcademiaDoZe.Presentation.AppMaui.ViewModels;

public partial class ColaboradorListViewModel : BaseViewModel
{
    private readonly IColaboradorService _colaboradorService;

    public ObservableCollection<string> FilterTypes { get; } = ["Nome", "CPF", "Id", "Email", "Tipo", "Vinculo"];

    private ObservableCollection<ColaboradorDto> _colaboradores = [];
    public ObservableCollection<ColaboradorDto> Colaboradores
    {
        get => _colaboradores;
        set => SetProperty(ref _colaboradores, value);
    }

    private ColaboradorDto? _selectedColaborador;
    public ColaboradorDto? SelectedColaborador
    {
        get => _selectedColaborador;
        set => SetProperty(ref _selectedColaborador, value);
    }

    private string _searchText = string.Empty;
    public string SearchText
    {
        get => _searchText;
        set => SetProperty(ref _searchText, value);
    }

    private string _selectedFilterType = "Nome";
    public string SelectedFilterType
    {
        get => _selectedFilterType;
        set => SetProperty(ref _selectedFilterType, value);
    }

    public ColaboradorListViewModel(IColaboradorService colaboradorService)
    {
        _colaboradorService = colaboradorService;
        Title = "Colaboradores";
    }

    [RelayCommand]
    private async Task AddColaboradorAsync()
    {
        try
        {
            await Shell.Current.GoToAsync("colaborador");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Erro", $"Erro ao navegar para tela de cadastro: {ex.Message}", "OK");
        }
    }

    [RelayCommand]
    public async Task EditColaboradorAsync(ColaboradorDto colaborador)
    {
        if (colaborador == null) return;

        try
        {
            await Shell.Current.GoToAsync($"colaborador?Id={colaborador.Id}");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Erro", $"Erro ao navegar para tela de edição: {ex.Message}", "OK");
        }
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        IsRefreshing = true;
        await LoadColaboradoresAsync();
    }

    [RelayCommand]
    public async Task LoadColaboradoresAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                Colaboradores.Clear();
                OnPropertyChanged(nameof(Colaboradores));
            });

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            var colaboradoresList = await _colaboradorService.ObterTodosAsync(cts.Token);

            if (colaboradoresList != null)
            {
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    foreach (var colaborador in colaboradoresList)
                    {
                        Colaboradores.Add(colaborador);
                    }
                    OnPropertyChanged(nameof(Colaboradores));
                });
            }
        }
        catch (OperationCanceledException)
        {
            await Shell.Current.DisplayAlertAsync("Tempo Esgotado", "O carregamento dos colaboradores expirou. Verifique a conexão.", "OK");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Erro", $"Erro ao carregar colaboradores: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
            IsRefreshing = false;
        }
    }

    [RelayCommand]
    public async Task SearchColaboradoresAsync()
    {
        if (string.IsNullOrWhiteSpace(SearchText))
        {
            await LoadColaboradoresAsync();
            return;
        }

        try
        {
            IsBusy = true;
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            var todos = await _colaboradorService.ObterTodosAsync(cts.Token);

            if (todos == null) return;

            IEnumerable<ColaboradorDto> filtrados = SelectedFilterType switch
            {
                "CPF" => todos.Where(c => c.Cpf.Contains(new string([.. SearchText.Where(char.IsDigit)]))),
                "Id" => int.TryParse(SearchText, out var id) ? todos.Where(c => c.Id == id) : [],
                "Email" => todos.Where(c => c.Email != null && c.Email.Contains(SearchText.Trim(), StringComparison.OrdinalIgnoreCase)),
                "Tipo" => todos.Where(c => c.Tipo.ToString().Contains(SearchText.Trim(), StringComparison.OrdinalIgnoreCase)),
                "Vinculo" => todos.Where(c => c.Vinculo.ToString().Contains(SearchText.Trim(), StringComparison.OrdinalIgnoreCase)),
                _ => todos.Where(c => c.Nome.Contains(SearchText.Trim(), StringComparison.OrdinalIgnoreCase))
            };

            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                Colaboradores.Clear();
                foreach (var colab in filtrados)
                {
                    Colaboradores.Add(colab);
                }
                OnPropertyChanged(nameof(Colaboradores));
            });
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Erro", $"Erro ao buscar colaboradores: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public async Task DeleteColaboradorAsync(ColaboradorDto colaborador)
    {
        if (colaborador == null) return;

        bool confirm = await Shell.Current.DisplayAlertAsync(
            "Confirmar Exclusão",
            $"Deseja realmente excluir o colaborador {colaborador.Nome}?",
            "Sim", "Não");

        if (!confirm) return;

        try
        {
            IsBusy = true;
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            bool success = await _colaboradorService.RemoverAsync(colaborador.Id, cts.Token);

            if (success)
            {
                Colaboradores.Remove(colaborador);
                await Shell.Current.DisplayAlertAsync("Sucesso", "Colaborador excluído com sucesso!", "OK");
            }
            else
            {
                await Shell.Current.DisplayAlertAsync("Erro", "Não foi possível excluir o colaborador.", "OK");
            }
        }
        catch (OperationCanceledException)
        {
            await Shell.Current.DisplayAlertAsync("Tempo Esgotado", "A exclusão do colaborador expirou. Verifique a conexão.", "OK");
        }
        catch (Exception ex)
        {
            if (ex.Message.Contains("FOREIGN KEY", StringComparison.OrdinalIgnoreCase) ||
                ex.Message.Contains("constraint", StringComparison.OrdinalIgnoreCase) ||
                ex.Message.Contains("REFERENCE", StringComparison.OrdinalIgnoreCase))
            {
                await Shell.Current.DisplayAlertAsync("Não Permitido",
                    "Este colaborador não pode ser excluído pois possui registros vinculados no sistema (ex: acessos, turmas ou matrículas).", "OK");
            }
            else
            {
                await Shell.Current.DisplayAlertAsync("Erro", $"Erro ao excluir colaborador: {ex.Message}", "OK");
            }
        }
        finally
        {
            IsBusy = false;
        }
    }
}
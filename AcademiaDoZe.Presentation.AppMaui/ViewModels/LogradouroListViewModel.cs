using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Interfaces;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace AcademiaDoZe.Presentation.AppMaui.ViewModels;

public partial class LogradouroListViewModel : BaseViewModel
{
    private readonly ILogradouroService _logradouroService;

    public ObservableCollection<string> FilterTypes { get; } = ["Cidade", "Id", "Cep"];
    public ObservableCollection<LogradouroDto> Logradouros { get; set; } = [];

    private string _searchText = string.Empty;
    public string SearchText
    {
        get => _searchText;
        set => SetProperty(ref _searchText, value);
    }

    private string _selectedFilterType = "Cidade";
    public string SelectedFilterType
    {
        get => _selectedFilterType;
        set => SetProperty(ref _selectedFilterType, value);
    }

    public LogradouroListViewModel(ILogradouroService logradouroService)
    {
        _logradouroService = logradouroService;
        Title = "Logradouros";
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        IsRefreshing = true;
        await LoadLogradourosAsync();
    }

    [RelayCommand]
    private async Task AddLogradouroAsync()
    {
        try
        {
            await Shell.Current.GoToAsync("logradouro");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Erro", $"Erro ao navegar: {ex.Message}", "OK");
        }
    }

    [RelayCommand]
    private async Task EditLogradouroAsync(LogradouroDto logradouro)
    {
        if (logradouro == null) return;
        try
        {
            await Shell.Current.GoToAsync($"logradouro?Id={logradouro.Id}");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Erro", $"Erro ao navegar para edição: {ex.Message}", "OK");
        }
    }

    [RelayCommand]
    private async Task LoadLogradourosAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                Logradouros.Clear();
                OnPropertyChanged(nameof(Logradouros));
            });

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            var lista = await _logradouroService.ObterTodosAsync(cts.Token);

            if (lista != null)
            {
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    foreach (var log in lista)
                        Logradouros.Add(log);

                    OnPropertyChanged(nameof(Logradouros));
                });
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Erro", $"Erro ao carregar logradouros: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
            IsRefreshing = false;
        }
    }

    [RelayCommand]
    private async Task SearchLogradourosAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            await MainThread.InvokeOnMainThreadAsync(() => Logradouros.Clear());

            IEnumerable<LogradouroDto> resultados = [];
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

            if (string.IsNullOrWhiteSpace(SearchText))
            {
                resultados = await _logradouroService.ObterTodosAsync(cts.Token) ?? [];
            }
            else if (SelectedFilterType == "Cidade")
            {
                resultados = await _logradouroService.ObterPorCidadeAsync(SearchText.Trim(), cts.Token) ?? [];
            }
            else if (SelectedFilterType == "Id")
            {
                if (!int.TryParse(SearchText.Trim(), out int id) || id <= 0)
                {
                    await Shell.Current.DisplayAlertAsync("Validação", "Informe um ID numérico válido.", "OK");
                    return;
                }
                var item = await _logradouroService.ObterPorIdAsync(id, cts.Token);
                if (item != null) resultados = [item];
            }
            else if (SelectedFilterType == "Cep")
            {
                var cepLimpo = new string([.. SearchText.Where(char.IsDigit)]);
                if (cepLimpo.Length != 8)
                {
                    await Shell.Current.DisplayAlertAsync("Validação", "O CEP deve ter 8 dígitos numéricos.", "OK");
                    return;
                }
                var item = await _logradouroService.ObterPorCepAsync(cepLimpo, cts.Token);
                if (item != null) resultados = [item];
            }

            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                foreach (var log in resultados)
                    Logradouros.Add(log);

                OnPropertyChanged(nameof(Logradouros));
            });
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Erro", $"Erro na busca: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task DeleteLogradouroAsync(LogradouroDto logradouro)
    {
        if (logradouro == null) return;
        bool confirm = await Shell.Current.DisplayAlertAsync("Confirmar Exclusão", $"Deseja realmente excluir o logradouro {logradouro.Nome}?", "Sim", "Não");
        if (!confirm) return;

        try
        {
            IsBusy = true;
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            bool success = await _logradouroService.RemoverAsync(logradouro.Id, cts.Token);
            if (success)
            {
                Logradouros.Remove(logradouro);
                await Shell.Current.DisplayAlertAsync("Sucesso", "Logradouro excluído com sucesso!", "OK");
            }
            else
            {
                await Shell.Current.DisplayAlertAsync("Erro", "Não foi possível excluir o logradouro.", "OK");
            }
        }
        catch (Exception ex)
        {
            if (ex.Message.Contains("FOREIGN KEY", StringComparison.OrdinalIgnoreCase) ||
                ex.Message.Contains("constraint", StringComparison.OrdinalIgnoreCase) ||
                ex.Message.Contains("REFERENCE", StringComparison.OrdinalIgnoreCase))
            {
                await Shell.Current.DisplayAlertAsync("Não Permitido", "Este logradouro não pode ser excluído pois possui vínculos ativos.", "OK");
            }
            else
            {
                await Shell.Current.DisplayAlertAsync("Erro", $"Erro ao excluir: {ex.Message}", "OK");
            }
        }
        finally
        {
            IsBusy = false;
        }
    }
}
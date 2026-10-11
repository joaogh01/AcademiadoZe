using System.Collections.ObjectModel;
using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AcademiaDoZe.Presentation.AppMaui.ViewModels;

public partial class AlunoListViewModel : BaseViewModel
{
    private readonly IAlunoService _alunoService;

    [ObservableProperty]
    private ObservableCollection<AlunoDto> _alunos = new();

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private string _selectedFilterType = "Nome";

    [ObservableProperty]
    private bool _isRefreshing;

    public List<string> FilterTypes { get; } = new() { "Nome", "CPF", "Telefone", "Email" };

    public AlunoListViewModel(IAlunoService alunoService)
    {
        Title = "Alunos";
        _alunoService = alunoService;
    }

    [RelayCommand]
    public async Task AddAlunoAsync()
    {
        try
        {
            await Shell.Current.GoToAsync("aluno");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Erro", $"Erro ao navegar para tela de cadastro: {ex.Message}", "OK");
        }
    }

    [RelayCommand]
    public async Task EditAlunoAsync(AlunoDto aluno)
    {
        if (aluno == null) return;

        try
        {
            await Shell.Current.GoToAsync($"aluno?Id={aluno.Id}");
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
        await LoadAlunosAsync();
    }

    [RelayCommand]
    public async Task LoadAlunosAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                Alunos.Clear();
                OnPropertyChanged(nameof(Alunos));
            });

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            var alunosList = await _alunoService.ObterTodosAsync(cts.Token);

            if (alunosList != null)
            {
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    foreach (var aluno in alunosList)
                    {
                        Alunos.Add(aluno);
                    }
                    OnPropertyChanged(nameof(Alunos));
                });
            }
        }
        catch (OperationCanceledException)
        {
            await Shell.Current.DisplayAlertAsync("Tempo Esgotado", "O carregamento dos alunos expirou. Verifique a conexão.", "OK");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Erro", $"Erro ao carregar alunos: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
            IsRefreshing = false;
        }
    }

    [RelayCommand]
    public async Task SearchAlunosAsync()
    {
        if (string.IsNullOrWhiteSpace(SearchText))
        {
            await LoadAlunosAsync();
            return;
        }

        try
        {
            IsBusy = true;
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            var alunosList = await _alunoService.ObterTodosAsync(cts.Token);

            if (alunosList != null)
            {
                var query = SearchText.Trim().ToLowerInvariant();
                var filtered = alunosList.Where(a => SelectedFilterType switch
                {
                    "CPF" => (a.Cpf ?? string.Empty).ToLowerInvariant().Contains(query),
                    "Telefone" => (a.Telefone ?? string.Empty).ToLowerInvariant().Contains(query),
                    "Email" => (a.Email ?? string.Empty).ToLowerInvariant().Contains(query),
                    _ => (a.Nome ?? string.Empty).ToLowerInvariant().Contains(query)
                }).ToList();

                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    Alunos.Clear();
                    foreach (var aluno in filtered)
                    {
                        Alunos.Add(aluno);
                    }
                    OnPropertyChanged(nameof(Alunos));
                });
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Erro", $"Falha ao filtrar alunos: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public async Task DeleteAlunoAsync(AlunoDto aluno)
    {
        if (aluno == null) return;

        bool confirm = await Shell.Current.DisplayAlertAsync(
            "Confirmação",
            $"Deseja realmente eliminar o aluno {aluno.Nome}?",
            "Sim",
            "Não");

        if (!confirm) return;

        try
        {
            IsBusy = true;
            var removido = await _alunoService.RemoverAsync(aluno.Id);

            if (removido)
            {
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    Alunos.Remove(aluno);
                });
                await Shell.Current.DisplayAlertAsync("Sucesso", "Aluno eliminado com sucesso!", "OK");
            }
            else
            {
                await Shell.Current.DisplayAlertAsync("Erro", "Não foi possível eliminar o aluno.", "OK");
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Erro", $"Falha ao eliminar aluno: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }
}
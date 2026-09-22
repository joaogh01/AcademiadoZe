using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Interfaces;
using CommunityToolkit.Mvvm.Input;

namespace AcademiaDoZe.Presentation.AppMaui.ViewModels;

[QueryProperty(nameof(LogradouroId), "Id")]
public partial class LogradouroViewModel : BaseViewModel
{
    private readonly ILogradouroService _logradouroService;

    private LogradouroDto _logradouro = new()
    {
        Cep = string.Empty,
        Nome = string.Empty,
        Bairro = string.Empty,
        Cidade = string.Empty,
        Estado = string.Empty,
        Pais = "Brasil"
    };

    public LogradouroDto Logradouro
    {
        get => _logradouro;
        set => SetProperty(ref _logradouro, value);
    }

    private int _logradouroId;
    public int LogradouroId
    {
        get => _logradouroId;
        set => SetProperty(ref _logradouroId, value);
    }

    private bool _isEditMode;
    public bool IsEditMode
    {
        get => _isEditMode;
        set => SetProperty(ref _isEditMode, value);
    }

    public LogradouroViewModel(ILogradouroService logradouroService)
    {
        _logradouroService = logradouroService;
        Title = "Detalhes do Logradouro";
    }

    public async Task InitializeAsync()
    {
        if (LogradouroId > 0)
        {
            IsEditMode = true;
            Title = "Editar Logradouro";
            await LoadLogradouroAsync();
        }
        else
        {
            IsEditMode = false;
            Title = "Novo Logradouro";
            Logradouro = new LogradouroDto
            {
                Cep = string.Empty,
                Nome = string.Empty,
                Bairro = string.Empty,
                Cidade = string.Empty,
                Estado = string.Empty,
                Pais = "Brasil"
            };
        }
    }

    [RelayCommand]
    private async Task CancelAsync() => await Shell.Current.GoToAsync("..");

    [RelayCommand]
    private async Task LoadLogradouroAsync()
    {
        if (LogradouroId <= 0) return;
        try
        {
            IsBusy = true;
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            var logradouroData = await _logradouroService.ObterPorIdAsync(LogradouroId, cts.Token);
            if (logradouroData != null)
                Logradouro = logradouroData;
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Erro", $"Erro ao carregar dados: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task SearchByCepAsync()
    {
        if (string.IsNullOrWhiteSpace(Logradouro.Cep))
        {
            await Shell.Current.DisplayAlertAsync("Aviso", "Informe o CEP para realizar a busca.", "OK");
            return;
        }

        var apenasDigitos = new string([.. Logradouro.Cep.Where(char.IsDigit)]);
        if (apenasDigitos.Length != 8)
        {
            await Shell.Current.DisplayAlertAsync("Validação", "O CEP deve conter 8 dígitos numéricos.", "OK");
            return;
        }

        try
        {
            IsBusy = true;
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            var logradouroData = await _logradouroService.ObterPorCepAsync(apenasDigitos, cts.Token);

            if (logradouroData != null)
            {
                Logradouro = logradouroData;
                LogradouroId = logradouroData.Id;
                IsEditMode = true;
                Title = "Editar Logradouro";
                await Shell.Current.DisplayAlertAsync("Aviso", "CEP já cadastrado! Dados carregados para edição.", "OK");
            }
            else
            {
                await Shell.Current.DisplayAlertAsync("Aviso", "CEP não encontrado.", "OK");
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Erro", $"Erro ao buscar CEP: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task SaveLogradouroAsync()
    {
        if (IsBusy) return;
        if (!await ValidateLogradouroAsync(Logradouro)) return;

        try
        {
            IsBusy = true;
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

            Logradouro.Cep = new string([.. Logradouro.Cep.Where(char.IsDigit)]);
            Logradouro.Estado = Logradouro.Estado.Trim().ToUpperInvariant();
            Logradouro.Nome = Logradouro.Nome.Trim();
            Logradouro.Bairro = Logradouro.Bairro.Trim();
            Logradouro.Cidade = Logradouro.Cidade.Trim();
            Logradouro.Pais = string.IsNullOrWhiteSpace(Logradouro.Pais) ? "Brasil" : Logradouro.Pais.Trim();

            if (IsEditMode)
            {
                await _logradouroService.AtualizarAsync(Logradouro, cts.Token);
                await Shell.Current.DisplayAlertAsync("Sucesso", "Logradouro atualizado com sucesso!", "OK");
            }
            else
            {
                await _logradouroService.AdicionarAsync(Logradouro, cts.Token);
                await Shell.Current.DisplayAlertAsync("Sucesso", "Logradouro criado com sucesso!", "OK");
            }

            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Erro", $"Erro ao salvar: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private static async Task<bool> ValidateLogradouroAsync(LogradouroDto logradouro)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(logradouro.Cep))
            errors.Add("• CEP é obrigatório.");
        else if (new string([.. logradouro.Cep.Where(char.IsDigit)]).Length != 8)
            errors.Add("• O CEP deve conter 8 dígitos numéricos.");

        if (string.IsNullOrWhiteSpace(logradouro.Nome))
            errors.Add("• Logradouro / Rua é obrigatório.");

        if (string.IsNullOrWhiteSpace(logradouro.Bairro))
            errors.Add("• Bairro é obrigatório.");

        if (string.IsNullOrWhiteSpace(logradouro.Cidade))
            errors.Add("• Cidade é obrigatória.");

        if (string.IsNullOrWhiteSpace(logradouro.Estado))
            errors.Add("• Estado (UF) é obrigatório.");
        else if (logradouro.Estado.Trim().Length != 2 || !logradouro.Estado.Trim().All(char.IsLetter))
            errors.Add("• O Estado deve conter a sigla UF de 2 letras (ex: SC, SP).");

        if (string.IsNullOrWhiteSpace(logradouro.Pais))
            errors.Add("• País é obrigatório.");

        if (errors.Count > 0)
        {
            await Shell.Current.DisplayAlertAsync("Erros de Validação", "Por favor, corrija os seguintes campos:\n\n" + string.Join("\n", errors), "OK");
            return false;
        }

        return true;
    }
}
using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Enums;
using AcademiaDoZe.Application.Interfaces;
using CommunityToolkit.Mvvm.Input;

namespace AcademiaDoZe.Presentation.AppMaui.ViewModels;

[QueryProperty(nameof(ColaboradorId), "Id")]
public partial class ColaboradorViewModel : BaseViewModel
{
    public IEnumerable<AppColaboradorTipo> ColaboradorTipos { get; } = Enum.GetValues<AppColaboradorTipo>().Cast<AppColaboradorTipo>();
    public IEnumerable<AppColaboradorVinculo> ColaboradorVinculos { get; } = Enum.GetValues<AppColaboradorVinculo>().Cast<AppColaboradorVinculo>();

    private readonly IColaboradorService _colaboradorService;
    private readonly ILogradouroService _logradouroService;

    private ColaboradorDto _colaborador = CriarColaboradorPadrao();
    public ColaboradorDto Colaborador
    {
        get => _colaborador;
        set => SetProperty(ref _colaborador, value);
    }

    private int _colaboradorId;
    public int ColaboradorId
    {
        get => _colaboradorId;
        set => SetProperty(ref _colaboradorId, value);
    }

    private bool _isEditMode;
    public bool IsEditMode
    {
        get => _isEditMode;
        set => SetProperty(ref _isEditMode, value);
    }

    private string _confirmarSenha = string.Empty;
    public string ConfirmarSenha
    {
        get => _confirmarSenha;
        set => SetProperty(ref _confirmarSenha, value);
    }

    public bool HasEnderecoVinculado => Colaborador?.Endereco != null && Colaborador.Endereco.Id > 0;
    public bool HasNoEnderecoVinculado => !HasEnderecoVinculado;

    public ColaboradorViewModel(IColaboradorService colaboradorService, ILogradouroService logradouroService)
    {
        _colaboradorService = colaboradorService;
        _logradouroService = logradouroService;
        Title = "Detalhes do Colaborador";
    }

    public async Task InitializeAsync()
    {
        ConfirmarSenha = string.Empty;
        if (ColaboradorId > 0)
        {
            IsEditMode = true;
            Title = "Editar Colaborador";
            await LoadColaboradorAsync();
        }
        else
        {
            IsEditMode = false;
            Title = "Novo Colaborador";
            Colaborador = CriarColaboradorPadrao();
            NotificarStatusEndereco();
        }
    }

    private void NotificarStatusEndereco()
    {
        OnPropertyChanged(nameof(HasEnderecoVinculado));
        OnPropertyChanged(nameof(HasNoEnderecoVinculado));
    }

    private static ColaboradorDto CriarColaboradorPadrao() => new()
    {
        Nome = string.Empty,
        Cpf = string.Empty,
        DataNascimento = DateOnly.FromDateTime(DateTime.Today.AddYears(-18)),
        Telefone = string.Empty,
        Email = string.Empty,
        Senha = string.Empty,
        Endereco = new LogradouroDto
        {
            Cep = string.Empty,
            Nome = string.Empty,
            Bairro = string.Empty,
            Cidade = string.Empty,
            Estado = string.Empty,
            Pais = "Brasil"
        },
        Numero = string.Empty,
        Complemento = string.Empty,
        DataAdmissao = DateOnly.FromDateTime(DateTime.Today),
        Tipo = AppColaboradorTipo.Atendente,
        Vinculo = AppColaboradorVinculo.CLT
    };

    [RelayCommand]
    private async Task CancelAsync() => await Shell.Current.GoToAsync("..");

    [RelayCommand]
    public async Task LoadColaboradorAsync()
    {
        if (ColaboradorId <= 0) return;

        try
        {
            IsBusy = true;
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            var colaboradorData = await _colaboradorService.ObterPorIdAsync(ColaboradorId, cts.Token);
            if (colaboradorData != null)
            {
                colaboradorData.Endereco ??= new LogradouroDto
                {
                    Cep = string.Empty,
                    Nome = string.Empty,
                    Bairro = string.Empty,
                    Cidade = string.Empty,
                    Estado = string.Empty,
                    Pais = "Brasil"
                };

                colaboradorData.Senha = string.Empty;
                ConfirmarSenha = string.Empty;
                Colaborador = colaboradorData;
                NotificarStatusEndereco();
            }
        }
        catch (OperationCanceledException)
        {
            await Shell.Current.DisplayAlertAsync("Tempo Esgotado", "O carregamento do colaborador expirou. Verifique a conexão.", "OK");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Erro", $"Erro ao carregar colaborador: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public async Task SearchByCpfAsync()
    {
        if (string.IsNullOrWhiteSpace(Colaborador.Cpf))
        {
            await Shell.Current.DisplayAlertAsync("Aviso", "Informe o CPF para realizar a busca.", "OK");
            return;
        }

        var apenasDigitos = new string([.. Colaborador.Cpf.Where(char.IsDigit)]);
        if (apenasDigitos.Length != 11)
        {
            await Shell.Current.DisplayAlertAsync("Validação", "O CPF deve conter exatamente 11 dígitos numéricos.", "OK");
            return;
        }

        try
        {
            IsBusy = true;
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            var colaboradorData = await _colaboradorService.ObterPorCpfAsync(apenasDigitos, cts.Token);
            if (colaboradorData != null)
            {
                colaboradorData.Endereco ??= new LogradouroDto
                {
                    Cep = string.Empty,
                    Nome = string.Empty,
                    Bairro = string.Empty,
                    Cidade = string.Empty,
                    Estado = string.Empty,
                    Pais = "Brasil"
                };

                colaboradorData.Senha = string.Empty;
                ConfirmarSenha = string.Empty;
                Colaborador = colaboradorData;
                ColaboradorId = colaboradorData.Id;
                IsEditMode = true;
                Title = "Editar Colaborador";
                NotificarStatusEndereco();
                await Shell.Current.DisplayAlertAsync("Aviso", "Colaborador já cadastrado! Dados carregados para edição.", "OK");
            }
            else
            {
                await Shell.Current.DisplayAlertAsync("Aviso", "CPF não encontrado.", "OK");
            }
        }
        catch (OperationCanceledException)
        {
            await Shell.Current.DisplayAlertAsync("Tempo Esgotado", "A busca por CPF expirou. Verifique a conexão.", "OK");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Erro", $"Erro ao buscar CPF: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public async Task SearchByCepAsync()
    {
        if (Colaborador.Endereco == null || string.IsNullOrWhiteSpace(Colaborador.Endereco.Cep))
        {
            await Shell.Current.DisplayAlertAsync("Aviso", "Informe o CEP para realizar a busca.", "OK");
            return;
        }

        var apenasDigitos = new string([.. Colaborador.Endereco.Cep.Where(char.IsDigit)]);
        if (apenasDigitos.Length != 8)
        {
            await Shell.Current.DisplayAlertAsync("Validação", "O CEP deve conter exatamente 8 dígitos numéricos.", "OK");
            return;
        }

        try
        {
            IsBusy = true;
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            var logradouroData = await _logradouroService.ObterPorCepAsync(apenasDigitos, cts.Token);
            if (logradouroData != null)
            {
                Colaborador.Endereco = logradouroData;
                OnPropertyChanged(nameof(Colaborador));
                NotificarStatusEndereco();
                await Shell.Current.DisplayAlertAsync("Aviso", "CEP encontrado! Endereço preenchido automaticamente.", "OK");
            }
            else
            {
                await Shell.Current.DisplayAlertAsync("Aviso", "CEP não encontrado no cadastro de logradouros. Cadastre o logradouro primeiro.", "OK");
            }
        }
        catch (OperationCanceledException)
        {
            await Shell.Current.DisplayAlertAsync("Tempo Esgotado", "A busca de CEP expirou. Verifique a conexão com o banco.", "OK");
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
    public async Task SaveColaboradorAsync()
    {
        if (IsBusy) return;

        if (!await ValidarColaboradorAsync(Colaborador, IsEditMode, ConfirmarSenha))
            return;

        IsBusy = true;
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

        if (Colaborador.Endereco != null && Colaborador.Endereco.Id <= 0 && !string.IsNullOrWhiteSpace(Colaborador.Endereco.Cep))
        {
            var cepLimpo = new string([.. Colaborador.Endereco.Cep.Where(char.IsDigit)]);
            if (cepLimpo.Length == 8)
            {
                var logradouroData = await _logradouroService.ObterPorCepAsync(cepLimpo, cts.Token);
                if (logradouroData != null)
                {
                    Colaborador.Endereco = logradouroData;
                }
                else
                {
                    await Shell.Current.DisplayAlertAsync("Endereço Obrigatório", "O CEP informado não está cadastrado como Logradouro. Cadastre-o previamente.", "OK");
                    IsBusy = false;
                    return;
                }
            }
            else
            {
                await Shell.Current.DisplayAlertAsync("Validação", "Informe um CEP válido com 8 dígitos para o endereço.", "OK");
                IsBusy = false;
                return;
            }
        }

        Colaborador.Nome = Colaborador.Nome.Trim();
        Colaborador.Cpf = new string([.. Colaborador.Cpf.Where(char.IsDigit)]);
        Colaborador.Telefone = new string([.. Colaborador.Telefone.Where(char.IsDigit)]);
        Colaborador.Numero = Colaborador.Numero.Trim();
        Colaborador.Complemento = string.IsNullOrWhiteSpace(Colaborador.Complemento) ? null : Colaborador.Complemento.Trim();
        Colaborador.Email = string.IsNullOrWhiteSpace(Colaborador.Email) ? null : Colaborador.Email.Trim();
        Colaborador.Senha = string.IsNullOrWhiteSpace(Colaborador.Senha) ? null : Colaborador.Senha.Trim();

        try
        {
            if (IsEditMode)
            {
                await _colaboradorService.AtualizarAsync(Colaborador, cts.Token);
                await Shell.Current.DisplayAlertAsync("Sucesso", "Colaborador atualizado com sucesso!", "OK");
            }
            else
            {
                await _colaboradorService.AdicionarAsync(Colaborador, cts.Token);
                await Shell.Current.DisplayAlertAsync("Sucesso", "Colaborador criado com sucesso!", "OK");
            }

            await Shell.Current.GoToAsync("..");
        }
        catch (OperationCanceledException)
        {
            await Shell.Current.DisplayAlertAsync("Tempo Esgotado", "A gravação do colaborador expirou. Verifique a conexão.", "OK");
        }
        catch (InvalidOperationException ex)
        {
            await Shell.Current.DisplayAlertAsync("Regra de Negócio", ex.Message, "OK");
        }
        catch (ArgumentException ex)
        {
            await Shell.Current.DisplayAlertAsync("Validação", ex.Message, "OK");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Erro", $"Erro ao salvar colaborador: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public async Task SelecionarFotoAsync()
    {
        try
        {
            string escolha = await Shell.Current.DisplayActionSheetAsync("Origem da Imagem", "Cancelar", "Remover Foto", "Galeria", "Câmera");
            if (escolha == "Remover Foto")
            {
                Colaborador.Foto = null;
                OnPropertyChanged(nameof(Colaborador));
                return;
            }

            FileResult? result = null;
            if (escolha == "Galeria")
            {
                result = await FilePicker.Default.PickAsync(new PickOptions
                {
                    PickerTitle = "Selecione uma imagem",
                    FileTypes = FilePickerFileType.Images
                });
            }
            else if (escolha == "Câmera")
            {
                if (MediaPicker.Default.IsCaptureSupported)
                {
                    result = await MediaPicker.Default.CapturePhotoAsync();
                }
                else
                {
                    await Shell.Current.DisplayAlertAsync("Erro", "Captura de foto não suportada neste dispositivo.", "OK");
                    return;
                }
            }

            if (result != null)
            {
                using var stream = await result.OpenReadAsync();
                using var ms = new MemoryStream();
                await stream.CopyToAsync(ms);
                Colaborador.Foto = new ArquivoDto { Conteudo = ms.ToArray() };
                OnPropertyChanged(nameof(Colaborador));
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Erro", $"Erro ao selecionar imagem: {ex.Message}", "OK");
        }
    }

    private static async Task<bool> ValidarColaboradorAsync(ColaboradorDto colab, bool isEdit, string confirmarSenha)
    {
        var erros = new List<string>();

        if (string.IsNullOrWhiteSpace(colab.Nome)) erros.Add("Nome é obrigatório.");
        var cpfLimpo = new string([.. colab.Cpf.Where(char.IsDigit)]);
        if (cpfLimpo.Length != 11) erros.Add("CPF deve conter 11 dígitos.");

        if (colab.DataNascimento > DateOnly.FromDateTime(DateTime.Today))
            erros.Add("Data de nascimento não pode ser futura.");
        else if (colab.DataNascimento > DateOnly.FromDateTime(DateTime.Today.AddYears(-18)))
            erros.Add("Colaborador deve ter no mínimo 18 anos de idade.");

        var telLimpo = new string([.. colab.Telefone.Where(char.IsDigit)]);
        if (telLimpo.Length < 10 || telLimpo.Length > 11)
            erros.Add("Telefone deve conter DDD + número (10 ou 11 dígitos).");

        if (string.IsNullOrWhiteSpace(colab.Email) || !ValidarFormatoEmail(colab.Email))
            erros.Add("Informe um e-mail válido.");

        if (!isEdit && string.IsNullOrWhiteSpace(colab.Senha))
            erros.Add("Senha é obrigatória no cadastro.");
        else if (!string.IsNullOrWhiteSpace(colab.Senha))
        {
            if (colab.Senha.Length < 6) erros.Add("A senha deve ter no mínimo 6 caracteres.");
            if (colab.Senha != confirmarSenha) erros.Add("A confirmação de senha não confere.");
        }

        if (colab.Endereco == null || string.IsNullOrWhiteSpace(colab.Endereco.Cep))
            erros.Add("Endereço é obrigatório.");
        if (string.IsNullOrWhiteSpace(colab.Numero))
            erros.Add("Número do endereço é obrigatório.");

        if (colab.DataAdmissao > DateOnly.FromDateTime(DateTime.Today))
            erros.Add("Data de admissão não pode ser futura.");

        if (erros.Count > 0)
        {
            await Shell.Current.DisplayAlertAsync("Erros de Validação", string.Join("\n", erros), "OK");
            return false;
        }

        return true;
    }

    private static bool ValidarFormatoEmail(string email)
    {
        var partes = email.Split('@');
        if (partes.Length != 2 || string.IsNullOrWhiteSpace(partes[0]) || string.IsNullOrWhiteSpace(partes[1])) return false;
        if (partes[1].StartsWith('.') || partes[1].EndsWith('.')) return false;
        var labels = partes[1].Split('.');
        return labels.Length >= 2 && !labels.Any(string.IsNullOrWhiteSpace);
    }
}
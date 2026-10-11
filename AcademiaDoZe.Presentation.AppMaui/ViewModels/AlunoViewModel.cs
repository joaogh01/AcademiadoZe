using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AcademiaDoZe.Presentation.AppMaui.ViewModels;

[QueryProperty(nameof(AlunoId), "Id")]
public partial class AlunoViewModel : BaseViewModel
{
    private readonly IAlunoService _alunoService;
    private readonly ILogradouroService _logradouroService;

    [ObservableProperty]
    private AlunoDto _aluno = CriarAlunoVazio();

    [ObservableProperty]
    private int _alunoId;

    [ObservableProperty]
    private bool _isEditMode;

    public bool HasEnderecoVinculado => Aluno?.Endereco != null && Aluno.Endereco.Id > 0;
    public bool HasNoEnderecoVinculado => !HasEnderecoVinculado;

    public AlunoViewModel(IAlunoService alunoService, ILogradouroService logradouroService)
    {
        _alunoService = alunoService;
        _logradouroService = logradouroService;
        Title = "Novo Aluno";
    }

    private static AlunoDto CriarAlunoVazio()
    {
        return new AlunoDto
        {
            Nome = string.Empty,
            Cpf = string.Empty,
            Telefone = string.Empty,
            Numero = string.Empty,
            DataNascimento = DateOnly.FromDateTime(DateTime.Today.AddYears(-18)),
            Endereco = new LogradouroDto
            {
                Cep = string.Empty,
                Nome = string.Empty,
                Bairro = string.Empty,
                Cidade = string.Empty,
                Estado = string.Empty,
                Pais = string.Empty
            }
        };
    }

    async partial void OnAlunoIdChanged(int value)
    {
        if (value > 0)
        {
            await CarregarAlunoPorIdAsync(value);
        }
    }

    public async Task CarregarAlunoPorIdAsync(int id)
    {
        try
        {
            IsBusy = true;
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            var alunoExistente = await _alunoService.ObterPorIdAsync(id, cts.Token);

            if (alunoExistente != null)
            {
                Aluno = alunoExistente;
                IsEditMode = true;
                Title = "Editar Aluno";
                NotifyEnderecoProperties();
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Erro", $"Falha ao carregar aluno: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    public Task InitializeAsync()
    {
        if (!IsEditMode && AlunoId <= 0)
        {
            Aluno = CriarAlunoVazio();
            NotifyEnderecoProperties();
        }
        return Task.CompletedTask;
    }

    [RelayCommand]
    public async Task SearchByCpfAsync()
    {
        var cpfLimpo = new string([.. (Aluno.Cpf ?? string.Empty).Where(char.IsDigit)]);
        if (string.IsNullOrWhiteSpace(cpfLimpo))
        {
            await Shell.Current.DisplayAlertAsync("Aviso", "Introduza um CPF válido para pesquisar.", "OK");
            return;
        }

        try
        {
            IsBusy = true;
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            var alunoExistente = await _alunoService.ObterPorCpfAsync(cpfLimpo, cts.Token);

            if (alunoExistente != null)
            {
                Aluno = alunoExistente;
                IsEditMode = true;
                Title = "Editar Aluno";
                NotifyEnderecoProperties();
                await Shell.Current.DisplayAlertAsync("Sucesso", "Aluno localizado e carregado!", "OK");
            }
            else
            {
                await Shell.Current.DisplayAlertAsync("Informação", "Nenhum aluno encontrado com este CPF.", "OK");
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Erro", $"Falha ao pesquisar CPF: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public async Task SearchByCepAsync()
    {
        var cep = Aluno.Endereco?.Cep ?? string.Empty;
        var cepLimpo = new string([.. cep.Where(char.IsDigit)]);

        if (cepLimpo.Length != 8)
        {
            await Shell.Current.DisplayAlertAsync("Validação", "Informe um CEP válido com 8 dígitos para o endereço.", "OK");
            return;
        }

        try
        {
            IsBusy = true;
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            var logradouroData = await _logradouroService.ObterPorCepAsync(cepLimpo, cts.Token);

            if (logradouroData != null)
            {
                Aluno.Endereco = logradouroData;
                NotifyEnderecoProperties();
                await Shell.Current.DisplayAlertAsync("Sucesso", "Logradouro vinculado com sucesso!", "OK");
            }
            else
            {
                await Shell.Current.DisplayAlertAsync("Endereço Obrigatório", "O CEP informado não está cadastrado como Logradouro. Cadastre-o previamente.", "OK");
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Erro", $"Falha ao procurar CEP: {ex.Message}", "OK");
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
            string escolha = await Shell.Current.DisplayActionSheetAsync("Origem da Imagem", "Cancelar", "Remover Foto", "Galeria", "Câmara");

            if (escolha == "Remover Foto")
            {
                Aluno.Foto = null;
                OnPropertyChanged(nameof(Aluno));
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
            else if (escolha == "Câmara")
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
                Aluno.Foto = new ArquivoDto { Conteudo = ms.ToArray() };
                OnPropertyChanged(nameof(Aluno));
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Erro", $"Falha ao carregar imagem: {ex.Message}", "OK");
        }
    }

    [RelayCommand]
    public async Task SaveAlunoAsync()
    {
        if (IsBusy) return;

        if (string.IsNullOrWhiteSpace(Aluno.Nome))
        {
            await Shell.Current.DisplayAlertAsync("Validação", "O campo Nome é obrigatório.", "OK");
            return;
        }

        var cpfLimpo = new string([.. (Aluno.Cpf ?? string.Empty).Where(char.IsDigit)]);
        if (string.IsNullOrWhiteSpace(cpfLimpo))
        {
            await Shell.Current.DisplayAlertAsync("Validação", "O campo CPF é obrigatório.", "OK");
            return;
        }

        if (Aluno.Endereco == null || Aluno.Endereco.Id <= 0)
        {
            await Shell.Current.DisplayAlertAsync("Endereço Obrigatório", "Vincule um Logradouro válido utilizando a busca por CEP.", "OK");
            return;
        }

        try
        {
            IsBusy = true;
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

            Aluno.Nome = Aluno.Nome.Trim();
            Aluno.Cpf = cpfLimpo;
            Aluno.Telefone = new string([.. (Aluno.Telefone ?? string.Empty).Where(char.IsDigit)]);
            Aluno.Numero = (Aluno.Numero ?? string.Empty).Trim();
            Aluno.Complemento = string.IsNullOrWhiteSpace(Aluno.Complemento) ? null : Aluno.Complemento.Trim();
            Aluno.Email = string.IsNullOrWhiteSpace(Aluno.Email) ? null : Aluno.Email.Trim();

            if (IsEditMode)
            {
                await _alunoService.AtualizarAsync(Aluno, cts.Token);
                await Shell.Current.DisplayAlertAsync("Sucesso", "Aluno atualizado com sucesso!", "OK");
            }
            else
            {
                await _alunoService.AdicionarAsync(Aluno, cts.Token);
                await Shell.Current.DisplayAlertAsync("Sucesso", "Aluno cadastrado com sucesso!", "OK");
            }

            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Erro", $"Falha ao salvar aluno: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public async Task CancelAsync()
    {
        await Shell.Current.GoToAsync("..");
    }

    private void NotifyEnderecoProperties()
    {
        OnPropertyChanged(nameof(HasEnderecoVinculado));
        OnPropertyChanged(nameof(HasNoEnderecoVinculado));
        OnPropertyChanged(nameof(Aluno));
    }
}
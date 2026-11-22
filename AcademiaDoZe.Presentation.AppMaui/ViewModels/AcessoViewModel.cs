// João Gabriel Hensen dos Santos
using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Enums;
using AcademiaDoZe.Application.Interfaces;
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Repositories;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace AcademiaDoZe.Presentation.AppMaui.ViewModels;

public class AcessoRegistroHojeItem
{
    public string Nome { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public string Hora { get; set; } = string.Empty;
    public byte[]? FotoBytes { get; set; }
}

public class PessoaSelecaoItem
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Cpf { get; set; } = string.Empty;
    public byte[]? FotoBytes { get; set; }
    public bool IsColaborador { get; set; }
    public AppColaboradorVinculo? Vinculo { get; set; }

    public override string ToString() => $"{Nome} ({Cpf})";
}

public partial class AcessoViewModel : BaseViewModel
{
    private readonly IAlunoService _alunoService;
    private readonly IColaboradorService _colaboradorService;
    private readonly Func<IAlunoRepository> _alunoRepoFactory;
    private readonly Func<IColaboradorRepository> _colaboradorRepoFactory;
    private readonly Func<IMatriculaRepository> _matriculaRepoFactory;
    private readonly Func<IAcessoAlunoRepository> _acessoAlunoRepoFactory;
    private readonly Func<IAcessoColaboradorRepository> _acessoColaboradorRepoFactory;

    private List<PessoaSelecaoItem> _todosAlunos = [];
    private List<PessoaSelecaoItem> _todosColaboradores = [];

    public ObservableCollection<PessoaSelecaoItem> PessoasFiltradas { get; } = [];
    public ObservableCollection<AcessoRegistroHojeItem> AcessosHoje { get; } = [];

    private bool _isModoAluno = true;
    public bool IsModoAluno
    {
        get => _isModoAluno;
        set
        {
            if (SetProperty(ref _isModoAluno, value))
            {
                OnPropertyChanged(nameof(IsModoColaborador));
                OnPropertyChanged(nameof(ModoTexto));
                OnPropertyChanged(nameof(LabelPicker));
                LimparFeedback();
                AplicarFiltroPessoas();
            }
        }
    }

    public bool IsModoColaborador => !IsModoAluno;
    public string ModoTexto => IsModoAluno ? "Modo atual selecionado: Aluno" : "Modo atual selecionado: Colaborador";
    public string LabelPicker => IsModoAluno ? "Selecione o Aluno:" : "Selecione o Colaborador:";

    private string _filtroTexto = string.Empty;
    public string FiltroTexto
    {
        get => _filtroTexto;
        set
        {
            if (SetProperty(ref _filtroTexto, value))
            {
                AplicarFiltroPessoas();
            }
        }
    }

    private PessoaSelecaoItem? _pessoaSelecionada;
    public PessoaSelecaoItem? PessoaSelecionada
    {
        get => _pessoaSelecionada;
        set
        {
            if (SetProperty(ref _pessoaSelecionada, value))
            {
                OnPropertyChanged(nameof(TemPessoaSelecionada));
                _ = AtualizarStatusPessoaAsync();
            }
        }
    }

    public bool TemPessoaSelecionada => PessoaSelecionada != null;

    private bool _estaDentroAcademia;
    public bool EstaDentroAcademia
    {
        get => _estaDentroAcademia;
        set
        {
            if (SetProperty(ref _estaDentroAcademia, value))
            {
                OnPropertyChanged(nameof(StatusPresencaTexto));
                OnPropertyChanged(nameof(StatusPresencaCor));
                OnPropertyChanged(nameof(BotaoAcaoTexto));
                OnPropertyChanged(nameof(BotaoAcaoCor));
            }
        }
    }

    public string StatusPresencaTexto => EstaDentroAcademia ? "DENTRO DA ACADEMIA" : "FORA DA ACADEMIA";
    public Color StatusPresencaCor => EstaDentroAcademia ? Color.FromArgb("#16A34A") : Color.FromArgb("#312E81");
    public string BotaoAcaoTexto => EstaDentroAcademia ? "Registrar Saída" : "Registrar Entrada";
    public Color BotaoAcaoCor => EstaDentroAcademia ? Color.FromArgb("#EA580C") : Color.FromArgb("#B4A5F8");

    private bool _temFeedback;
    public bool TemFeedback
    {
        get => _temFeedback;
        set => SetProperty(ref _temFeedback, value);
    }

    private string _feedbackTitulo = string.Empty;
    public string FeedbackTitulo
    {
        get => _feedbackTitulo;
        set => SetProperty(ref _feedbackTitulo, value);
    }

    private string _feedbackMensagem = string.Empty;
    public string FeedbackMensagem
    {
        get => _feedbackMensagem;
        set => SetProperty(ref _feedbackMensagem, value);
    }

    private Color _feedbackCor = Color.FromArgb("#15803D");
    public Color FeedbackCor
    {
        get => _feedbackCor;
        set => SetProperty(ref _feedbackCor, value);
    }

    public AcessoViewModel(
        IAlunoService alunoService,
        IColaboradorService colaboradorService,
        Func<IAlunoRepository> alunoRepoFactory,
        Func<IColaboradorRepository> colaboradorRepoFactory,
        Func<IMatriculaRepository> matriculaRepoFactory,
        Func<IAcessoAlunoRepository> acessoAlunoRepoFactory,
        Func<IAcessoColaboradorRepository> acessoColaboradorRepoFactory)
    {
        _alunoService = alunoService;
        _colaboradorService = colaboradorService;
        _alunoRepoFactory = alunoRepoFactory;
        _colaboradorRepoFactory = colaboradorRepoFactory;
        _matriculaRepoFactory = matriculaRepoFactory;
        _acessoAlunoRepoFactory = acessoAlunoRepoFactory;
        _acessoColaboradorRepoFactory = acessoColaboradorRepoFactory;

        Title = "Controle de Acesso";
    }

    public async Task InitializeAsync()
    {
        await CarregarDadosIniciaisAsync();
    }

    [RelayCommand]
    public void SelecionarModoAluno()
    {
        IsModoAluno = true;
    }

    [RelayCommand]
    public void SelecionarModoColaborador()
    {
        IsModoAluno = false;
    }

    [RelayCommand]
    public void BuscarPessoa()
    {
        AplicarFiltroPessoas();
    }

    [RelayCommand]
    public async Task CarregarDadosIniciaisAsync()
    {
        if (IsBusy) return;
        try
        {
            IsBusy = true;
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));

            var alunos = await _alunoService.ObterTodosAsync(cts.Token);
            _todosAlunos = alunos.Select(a => new PessoaSelecaoItem
            {
                Id = a.Id,
                Nome = a.Nome,
                Cpf = a.Cpf,
                FotoBytes = a.Foto?.Conteudo,
                IsColaborador = false
            }).OrderBy(a => a.Nome).ToList();

            var colaboradores = await _colaboradorService.ObterTodosAsync(cts.Token);
            _todosColaboradores = colaboradores.Select(c => new PessoaSelecaoItem
            {
                Id = c.Id,
                Nome = c.Nome,
                Cpf = c.Cpf,
                FotoBytes = c.Foto?.Conteudo,
                IsColaborador = true,
                Vinculo = c.Vinculo
            }).OrderBy(c => c.Nome).ToList();

            AplicarFiltroPessoas();
            await CarregarAcessosHojeAsync();
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

    private void AplicarFiltroPessoas()
    {
        var fonte = IsModoAluno ? _todosAlunos : _todosColaboradores;
        var termo = FiltroTexto?.Trim().ToLowerInvariant() ?? string.Empty;

        var filtrados = string.IsNullOrEmpty(termo)
            ? fonte
            : fonte.Where(p => p.Nome.ToLowerInvariant().Contains(termo) || p.Cpf.Contains(termo)).ToList();

        PessoasFiltradas.Clear();
        foreach (var item in filtrados)
            PessoasFiltradas.Add(item);

        if (PessoasFiltradas.Count > 0)
            PessoaSelecionada = PessoasFiltradas[0];
        else
            PessoaSelecionada = null;
    }

    private async Task AtualizarStatusPessoaAsync()
    {
        if (PessoaSelecionada == null)
        {
            EstaDentroAcademia = false;
            return;
        }

        try
        {
            var hoje = DateOnly.FromDateTime(DateTime.Now);
            if (!PessoaSelecionada.IsColaborador)
            {
                var repo = _acessoAlunoRepoFactory();
                EstaDentroAcademia = await repo.EstaNaAcademia(PessoaSelecionada.Id);
            }
            else
            {
                var repo = _acessoColaboradorRepoFactory();
                var acessosHoje = await repo.ObterAcessosPorColaboradorPeriodo(PessoaSelecionada.Id, hoje, hoje);
                EstaDentroAcademia = acessosHoje.Count() % 2 != 0;
            }
        }
        catch
        {
            EstaDentroAcademia = false;
        }
    }

    [RelayCommand]
    public async Task RegistrarAcessoAsync()
    {
        if (PessoaSelecionada == null || IsBusy) return;

        try
        {
            IsBusy = true;
            var agora = DateTime.Now;
            var hoje = DateOnly.FromDateTime(agora);

            if (!PessoaSelecionada.IsColaborador)
            {
                // ================= FLUXO ALUNO =================
                var repoAluno = _acessoAlunoRepoFactory();
                var acessosHoje = (await repoAluno.ObterAcessosPorAlunoPeriodo(PessoaSelecionada.Id, hoje, hoje))
                    .OrderBy(a => a.DataHora)
                    .ToList();

                bool estaDentro = acessosHoje.Count % 2 != 0;

                var alunoEntity = await _alunoRepoFactory().ObterPorId(PessoaSelecionada.Id);
                if (alunoEntity == null)
                {
                    ExibirFeedback("Acesso Negado", "Aluno não encontrado no banco de dados.", true);
                    return;
                }

                if (!estaDentro)
                {
                    var repoMat = _matriculaRepoFactory();
                    var matriculaAtiva = await repoMat.ObterMatriculaAtivaPorAluno(PessoaSelecionada.Id);

                    if (matriculaAtiva == null)
                    {
                        ExibirFeedback(
                            titulo: "Acesso Negado",
                            mensagem: "Acesso negado: o aluno não possui matrícula ativa.",
                            isErro: true);
                        return;
                    }

                    var novoAcesso = AcessoAluno.Criar(0, alunoEntity, agora);
                    if (novoAcesso.IsFailure)
                    {
                        ExibirFeedback("Acesso Negado", string.Join(" ", novoAcesso.Notifications.Select(n => n.Mensagem)), true);
                        return;
                    }

                    await repoAluno.Adicionar(novoAcesso.Value!);
                    EstaDentroAcademia = true;

                    int diasRestantes = matriculaAtiva.DataFim.DayNumber - hoje.DayNumber;
                    ExibirFeedback(
                        titulo: "Entrada Autorizada",
                        mensagem: $"Entrada autorizada. (Dias restantes de plano: {diasRestantes} dias).",
                        isErro: false);
                }
                else
                {
                    var ultimaEntrada = acessosHoje.Last();
                    var permanencia = agora - ultimaEntrada.DataHora;

                    var novoAcesso = AcessoAluno.Criar(0, alunoEntity, agora);
                    if (novoAcesso.IsFailure)
                    {
                        ExibirFeedback("Erro", string.Join(" ", novoAcesso.Notifications.Select(n => n.Mensagem)), true);
                        return;
                    }

                    await repoAluno.Adicionar(novoAcesso.Value!);
                    EstaDentroAcademia = false;

                    ExibirFeedback(
                        titulo: "Saída Registrada",
                        mensagem: $"Saída registrada. Tempo de permanência: {permanencia:hh\\:mm\\:ss}.",
                        isErro: false);
                }
            }
            else
            {
                // ================= FLUXO COLABORADOR =================
                var repoColab = _acessoColaboradorRepoFactory();
                var acessosHoje = (await repoColab.ObterAcessosPorColaboradorPeriodo(PessoaSelecionada.Id, hoje, hoje))
                    .OrderBy(a => a.DataHora)
                    .ToList();

                bool estaDentro = acessosHoje.Count % 2 != 0;
                TimeSpan horasJaTrabalhadas = await repoColab.ObterHorasTrabalhadasNoDia(PessoaSelecionada.Id, hoje);

                bool isEstagio = PessoaSelecionada.Vinculo == AppColaboradorVinculo.Estagio;
                TimeSpan limiteDiario = isEstagio ? TimeSpan.FromHours(6) : TimeSpan.FromHours(8);
                string tipoVinculoTexto = isEstagio ? "Estágio (6h)" : "CLT (8h)";

                // Busca a entidade real do Colaborador no banco de dados
                var colabEntity = await _colaboradorRepoFactory().ObterPorId(PessoaSelecionada.Id);
                if (colabEntity == null)
                {
                    ExibirFeedback("Acesso Negado", "Colaborador não encontrado no banco de dados.", true);
                    return;
                }

                if (!estaDentro)
                {
                    if (horasJaTrabalhadas >= limiteDiario)
                    {
                        ExibirFeedback(
                            titulo: "Acesso Negado",
                            mensagem: $"Limite diário atingido para {tipoVinculoTexto}. Horas acumuladas hoje: {horasJaTrabalhadas:hh\\:mm\\:ss}.",
                            isErro: true);
                        return;
                    }

                    var novoAcesso = AcessoColaborador.Criar(0, colabEntity, agora);
                    if (novoAcesso.IsFailure)
                    {
                        ExibirFeedback("Acesso Negado", string.Join(" ", novoAcesso.Notifications.Select(n => n.Mensagem)), true);
                        return;
                    }

                    await repoColab.Adicionar(novoAcesso.Value!);
                    EstaDentroAcademia = true;

                    ExibirFeedback(
                        titulo: "Entrada Registrada",
                        mensagem: $"Entrada registrada ({tipoVinculoTexto}). Horas trabalhadas hoje até o momento: {horasJaTrabalhadas:hh\\:mm\\:ss}.",
                        isErro: false);
                }
                else
                {
                    var ultimaEntrada = acessosHoje.Last();
                    var sessaoAtual = agora - ultimaEntrada.DataHora;
                    var totalDia = horasJaTrabalhadas + sessaoAtual;

                    var novoAcesso = AcessoColaborador.Criar(0, colabEntity, agora);
                    if (novoAcesso.IsFailure)
                    {
                        ExibirFeedback("Erro", string.Join(" ", novoAcesso.Notifications.Select(n => n.Mensagem)), true);
                        return;
                    }

                    await repoColab.Adicionar(novoAcesso.Value!);
                    EstaDentroAcademia = false;

                    ExibirFeedback(
                        titulo: "Saída Registrada",
                        mensagem: $"Saída registrada. Tempo total de permanência hoje: {totalDia:hh\\:mm\\:ss}.",
                        isErro: false);
                }
            }

            await CarregarAcessosHojeAsync();
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Erro", $"Falha ao registrar acesso: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public async Task CarregarAcessosHojeAsync()
    {
        try
        {
            var hoje = DateOnly.FromDateTime(DateTime.Now);
            var itens = new List<(DateTime DataHora, AcessoRegistroHojeItem Item)>();

            // Acessos de Alunos Hoje
            var repoAluno = _acessoAlunoRepoFactory();
            var acessosAlunos = (await repoAluno.ObterAcessosPorAlunoPeriodo(null, hoje, hoje))
                .OrderBy(a => a.DataHora)
                .ToList();

            var contagemAluno = new Dictionary<int, int>();
            var ultimaEntradaAluno = new Dictionary<int, DateTime>();

            foreach (var ac in acessosAlunos)
            {
                contagemAluno.TryGetValue(ac.AlunoId, out int qtd);
                qtd++;
                contagemAluno[ac.AlunoId] = qtd;

                var infoAluno = _todosAlunos.FirstOrDefault(a => a.Id == ac.AlunoId);
                string nome = infoAluno?.Nome ?? $"Aluno #{ac.AlunoId}";
                string desc;

                if (qtd % 2 != 0)
                {
                    ultimaEntradaAluno[ac.AlunoId] = ac.DataHora;
                    desc = "Entrada autorizada.";
                }
                else
                {
                    var tempo = ac.DataHora - ultimaEntradaAluno[ac.AlunoId];
                    desc = $"Saída registrada. Tempo de permanência: {tempo:hh\\:mm\\:ss}.";
                }

                itens.Add((ac.DataHora, new AcessoRegistroHojeItem
                {
                    Nome = nome,
                    Descricao = desc,
                    Hora = ac.DataHora.ToString("HH:mm:ss"),
                    FotoBytes = infoAluno?.FotoBytes
                }));
            }

            // Acessos de Colaboradores Hoje
            var repoColab = _acessoColaboradorRepoFactory();
            var acessosColabs = (await repoColab.ObterAcessosPorColaboradorPeriodo(null, hoje, hoje))
                .OrderBy(a => a.DataHora)
                .ToList();

            var contagemColab = new Dictionary<int, int>();
            var ultimaEntradaColab = new Dictionary<int, DateTime>();
            var acumuladoColab = new Dictionary<int, TimeSpan>();

            foreach (var ac in acessosColabs)
            {
                contagemColab.TryGetValue(ac.ColaboradorId, out int qtd);
                qtd++;
                contagemColab[ac.ColaboradorId] = qtd;

                acumuladoColab.TryGetValue(ac.ColaboradorId, out TimeSpan acum);
                var infoColab = _todosColaboradores.FirstOrDefault(c => c.Id == ac.ColaboradorId);
                string nome = infoColab?.Nome ?? $"Colaborador #{ac.ColaboradorId}";
                string desc;

                if (qtd % 2 != 0)
                {
                    ultimaEntradaColab[ac.ColaboradorId] = ac.DataHora;
                    desc = $"Entrada registrada. Horas trabalhadas hoje até o momento: {acum:hh\\:mm\\:ss}.";
                }
                else
                {
                    var sessao = ac.DataHora - ultimaEntradaColab[ac.ColaboradorId];
                    acum += sessao;
                    acumuladoColab[ac.ColaboradorId] = acum;
                    desc = $"Saída registrada. Tempo total acumulado no dia: {acum:hh\\:mm\\:ss}.";
                }

                itens.Add((ac.DataHora, new AcessoRegistroHojeItem
                {
                    Nome = nome,
                    Descricao = desc,
                    Hora = ac.DataHora.ToString("HH:mm:ss"),
                    FotoBytes = infoColab?.FotoBytes
                }));
            }

            AcessosHoje.Clear();
            foreach (var entry in itens.OrderByDescending(x => x.DataHora))
            {
                AcessosHoje.Add(entry.Item);
            }
        }
        catch
        {

        }
    }

    private void ExibirFeedback(string titulo, string mensagem, bool isErro)
    {
        FeedbackTitulo = titulo;
        FeedbackMensagem = mensagem;
        FeedbackCor = isErro ? Color.FromArgb("#DC2626") : Color.FromArgb("#15803D");
        TemFeedback = true;
    }

    private void LimparFeedback()
    {
        TemFeedback = false;
        FeedbackTitulo = string.Empty;
        FeedbackMensagem = string.Empty;
    }
}
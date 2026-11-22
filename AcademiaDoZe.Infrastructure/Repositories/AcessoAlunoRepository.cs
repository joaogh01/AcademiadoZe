// João Gabriel Hensen dos Santos
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Repositories;
using AcademiaDoZe.Domain.ValueObjects;
using AcademiaDoZe.Infrastructure.Data;
using AcademiaDoZe.Infrastructure.Exceptions;
using System.Data;
using System.Data.Common;

namespace AcademiaDoZe.Infrastructure.Repositories;

public class AcessoAlunoRepository : BaseRepository, IAcessoAlunoRepository
{
    public AcessoAlunoRepository(string connectionString, DatabaseType databaseType) : base(connectionString, databaseType) { }

    private const int PessoaTipoAluno = 1;

    private static string BaseSelectQuery =>
        @"SELECT a.id_acesso, a.pessoa_id, a.data_hora,
                 al.id_aluno, al.cpf, al.nome, al.nascimento, al.telefone, al.email,
                 al.logradouro_id, al.numero, al.complemento, al.senha, al.foto
          FROM tb_acesso a
          INNER JOIN tb_aluno al ON a.pessoa_id = al.id_aluno
          WHERE a.pessoa_tipo = 1";

    public async Task<AcessoAluno?> ObterPorId(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            string query = $"{BaseSelectQuery} AND a.id_acesso = @Id";
            await using var command = await CreateCommandAsync(query, cancellationToken);
            command.AddParameter("@Id", id, DbType.Int32);

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            return await reader.ReadAsync(cancellationToken) ? Map(reader) : null;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_OBTER_POR_ID", $"Erro ao obter acesso de aluno por ID {id}: {ex.Message}", ex);
        }
    }

    public async Task<IEnumerable<AcessoAluno>> ObterTodos(CancellationToken cancellationToken = default)
    {
        try
        {
            string query = $"{BaseSelectQuery} ORDER BY a.data_hora DESC";
            await using var command = await CreateCommandAsync(query, cancellationToken);
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);

            var lista = new List<AcessoAluno>();
            while (await reader.ReadAsync(cancellationToken))
                lista.Add(Map(reader));

            return lista;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_OBTER_TODOS", $"Erro ao obter acessos de alunos: {ex.Message}", ex);
        }
    }

    public async Task<AcessoAluno> Adicionar(AcessoAluno entity, CancellationToken cancellationToken = default)
    {
        try
        {
            string query = FormatInsertQuery("INSERT INTO tb_acesso (pessoa_tipo, pessoa_id, data_hora) VALUES (@PessoaTipo, @PessoaId, @DataHora)");
            await using var command = await CreateCommandAsync(query, cancellationToken);

            command.AddParameter("@PessoaTipo", PessoaTipoAluno, DbType.Int32);
            command.AddParameter("@PessoaId", entity.AlunoId, DbType.Int32);
            command.AddParameter("@DataHora", entity.DataHora, DbType.DateTime);

            var id = await command.ExecuteScalarIdAsync("ERRO_ADICIONAR_ACESSO_ALUNO", "Falha ao obter ID inserido para acesso do aluno.", cancellationToken);

            var idProperty = typeof(Entity).GetProperty("Id");
            idProperty?.SetValue(entity, id);

            return entity;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_ADICIONAR_ACESSO_ALUNO", $"Erro ao adicionar acesso do aluno: {ex.Message}", ex);
        }
    }

    public async Task<AcessoAluno> Atualizar(AcessoAluno entity, CancellationToken cancellationToken = default)
    {
        try
        {
            string query = "UPDATE tb_acesso SET pessoa_id = @PessoaId, data_hora = @DataHora WHERE id_acesso = @Id AND pessoa_tipo = @PessoaTipo";
            await using var command = await CreateCommandAsync(query, cancellationToken);

            command.AddParameter("@Id", entity.Id, DbType.Int32);
            command.AddParameter("@PessoaTipo", PessoaTipoAluno, DbType.Int32);
            command.AddParameter("@PessoaId", entity.AlunoId, DbType.Int32);
            command.AddParameter("@DataHora", entity.DataHora, DbType.DateTime);

            var rows = await command.ExecuteNonQueryAsync(cancellationToken);
            if (rows == 0)
                throw new InfrastructureException("REGISTRO_NAO_ENCONTRADO", $"Nenhum acesso de aluno encontrado com ID {entity.Id}.");

            return entity;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_ATUALIZAR_ACESSO_ALUNO", $"Erro ao atualizar acesso de aluno: {ex.Message}", ex);
        }
    }

    public async Task<bool> Remover(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            string query = "DELETE FROM tb_acesso WHERE id_acesso = @Id AND pessoa_tipo = @PessoaTipo";
            await using var command = await CreateCommandAsync(query, cancellationToken);
            command.AddParameter("@Id", id, DbType.Int32);
            command.AddParameter("@PessoaTipo", PessoaTipoAluno, DbType.Int32);

            return await command.ExecuteNonQueryAsync(cancellationToken) > 0;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_REMOVER_ACESSO_ALUNO", $"Erro ao remover acesso de aluno: {ex.Message}", ex);
        }
    }

    public async Task<IEnumerable<AcessoAluno>> ObterAcessosPorAlunoPeriodo(int? alunoId = null, DateOnly? inicio = null, DateOnly? fim = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var conditions = new List<string>();
            if (alunoId.HasValue) conditions.Add("a.pessoa_id = @AlunoId");
            if (inicio.HasValue) conditions.Add("a.data_hora >= @Inicio");
            if (fim.HasValue) conditions.Add("a.data_hora < @Fim");

            string whereExtra = conditions.Count > 0 ? " AND " + string.Join(" AND ", conditions) : "";
            string query = $"{BaseSelectQuery}{whereExtra} ORDER BY a.data_hora ASC";

            await using var command = await CreateCommandAsync(query, cancellationToken);
            if (alunoId.HasValue) command.AddParameter("@AlunoId", alunoId.Value, DbType.Int32);
            if (inicio.HasValue) command.AddParameter("@Inicio", inicio.Value.ToDateTime(TimeOnly.MinValue), DbType.DateTime);
            if (fim.HasValue) command.AddParameter("@Fim", fim.Value.AddDays(1).ToDateTime(TimeOnly.MinValue), DbType.DateTime);

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            var lista = new List<AcessoAluno>();
            while (await reader.ReadAsync(cancellationToken))
                lista.Add(Map(reader));

            return lista;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_OBTER_ACESSOS_PERIODO", $"Erro ao obter acessos do aluno por período: {ex.Message}", ex);
        }
    }

    public async Task<AcessoAluno?> ObterUltimoAcesso(int alunoId, CancellationToken cancellationToken = default)
    {
        try
        {
            string query = $"{BaseSelectQuery} AND a.pessoa_id = @AlunoId ORDER BY a.data_hora DESC, a.id_acesso DESC";
            await using var command = await CreateCommandAsync(query, cancellationToken);
            command.AddParameter("@AlunoId", alunoId, DbType.Int32);

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            return await reader.ReadAsync(cancellationToken) ? Map(reader) : null;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_OBTER_ULTIMO_ACESSO", $"Erro ao obter último acesso do aluno {alunoId}: {ex.Message}", ex);
        }
    }

    public async Task<bool> EstaNaAcademia(int alunoId, CancellationToken cancellationToken = default)
    {
        var hoje = DateOnly.FromDateTime(DateTime.Now);
        var acessosHoje = await ObterAcessosPorAlunoPeriodo(alunoId, hoje, hoje, cancellationToken);
        return acessosHoje.Count() % 2 != 0;
    }

    public Task<Dictionary<TimeOnly, int>> ObterHorarioMaisProcuradoPorMes(int mes, CancellationToken cancellationToken = default)
        => Task.FromResult(new Dictionary<TimeOnly, int>());

    public Task<Dictionary<int, TimeSpan>> ObterPermanenciaMediaPorMes(int mes, CancellationToken cancellationToken = default)
        => Task.FromResult(new Dictionary<int, TimeSpan>());

    public Task<IEnumerable<Aluno>> ObterAlunosSemAcessoNosUltimosDias(int dias, CancellationToken cancellationToken = default)
        => Task.FromResult(Enumerable.Empty<Aluno>());

    private static AcessoAluno Map(DbDataReader reader)
    {
        int id = reader.GetInt32Value("id_acesso");
        int alunoId = reader.GetInt32Value("pessoa_id");
        DateTime dataHora = Convert.ToDateTime(reader["data_hora"]);

        var logradouroDummy = Logradouro.Criar(1, "88500000", "Rua", "Centro", "Lages", "SC", "Brasil").Value!;
        var fotoBytes = reader.IsDBNull(reader.GetOrdinal("foto")) ? null : (byte[])reader["foto"];
        var arquivoFoto = fotoBytes != null && fotoBytes.Length > 0 ? Arquivo.Criar(fotoBytes).Value : null;

        var alunoResult = Aluno.Criar(
            id: alunoId,
            nome: reader.GetStringValue("nome"),
            cpf: reader.GetStringValue("cpf"),
            dataNascimento: DateOnly.FromDateTime(Convert.ToDateTime(reader["nascimento"])),
            telefone: reader.GetStringValue("telefone"),
            email: reader.GetStringValue("email"),
            endereco: logradouroDummy,
            numero: reader.GetStringValue("numero"),
            complemento: reader.IsDBNull(reader.GetOrdinal("complemento")) ? string.Empty : reader.GetStringValue("complemento"),
            senha: reader.GetStringValue("senha"),
            foto: arquivoFoto!
        );

        var acessoResult = AcessoAluno.Criar(id, alunoResult.Value!, dataHora);
        if (acessoResult.IsFailure)
            throw new InfrastructureException("ERRO_DOMINIO_MAPEAMENTO", $"Erro ao mapear AcessoAluno ID {id}.");

        return acessoResult.Value!;
    }
}
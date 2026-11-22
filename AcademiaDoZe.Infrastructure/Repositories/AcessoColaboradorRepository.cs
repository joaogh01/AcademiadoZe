// João Gabriel Hensen dos Santos
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Enums;
using AcademiaDoZe.Domain.Repositories;
using AcademiaDoZe.Domain.ValueObjects;
using AcademiaDoZe.Infrastructure.Data;
using AcademiaDoZe.Infrastructure.Exceptions;
using System.Data;
using System.Data.Common;

namespace AcademiaDoZe.Infrastructure.Repositories;

public class AcessoColaboradorRepository : BaseRepository, IAcessoColaboradorRepository
{
    public AcessoColaboradorRepository(string connectionString, DatabaseType databaseType) : base(connectionString, databaseType) { }

    private const int PessoaTipoColaborador = 2;

    private static string BaseSelectQuery =>
        @"SELECT a.id_acesso, a.pessoa_id, a.data_hora,
                 c.id_colaborador, c.cpf, c.nome, c.nascimento, c.telefone, c.email,
                 c.logradouro_id, c.numero, c.complemento, c.senha, c.foto,
                 c.admissao, c.tipo, c.vinculo
          FROM tb_acesso a
          INNER JOIN tb_colaborador c ON a.pessoa_id = c.id_colaborador
          WHERE a.pessoa_tipo = 2";

    public async Task<AcessoColaborador?> ObterPorId(int id, CancellationToken cancellationToken = default)
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
            throw new InfrastructureException("ERRO_OBTER_POR_ID", $"Erro ao obter acesso de colaborador por ID {id}: {ex.Message}", ex);
        }
    }

    public async Task<IEnumerable<AcessoColaborador>> ObterTodos(CancellationToken cancellationToken = default)
    {
        try
        {
            string query = $"{BaseSelectQuery} ORDER BY a.data_hora DESC";
            await using var command = await CreateCommandAsync(query, cancellationToken);
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);

            var lista = new List<AcessoColaborador>();
            while (await reader.ReadAsync(cancellationToken))
                lista.Add(Map(reader));

            return lista;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_OBTER_TODOS", $"Erro ao obter acessos de colaboradores: {ex.Message}", ex);
        }
    }

    public async Task<AcessoColaborador> Adicionar(AcessoColaborador entity, CancellationToken cancellationToken = default)
    {
        try
        {
            string query = FormatInsertQuery("INSERT INTO tb_acesso (pessoa_tipo, pessoa_id, data_hora) VALUES (@PessoaTipo, @PessoaId, @DataHora)");
            await using var command = await CreateCommandAsync(query, cancellationToken);

            command.AddParameter("@PessoaTipo", PessoaTipoColaborador, DbType.Int32);
            command.AddParameter("@PessoaId", entity.ColaboradorId, DbType.Int32);
            command.AddParameter("@DataHora", entity.DataHora, DbType.DateTime);

            var id = await command.ExecuteScalarIdAsync("ERRO_ADICIONAR_ACESSO_COLABORADOR", "Falha ao obter ID inserido para acesso do colaborador.", cancellationToken);

            var idProperty = typeof(Entity).GetProperty("Id");
            idProperty?.SetValue(entity, id);

            return entity;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_ADICIONAR_ACESSO_COLABORADOR", $"Erro ao adicionar acesso do colaborador: {ex.Message}", ex);
        }
    }

    public async Task<AcessoColaborador> Atualizar(AcessoColaborador entity, CancellationToken cancellationToken = default)
    {
        try
        {
            string query = "UPDATE tb_acesso SET pessoa_id = @PessoaId, data_hora = @DataHora WHERE id_acesso = @Id AND pessoa_tipo = @PessoaTipo";
            await using var command = await CreateCommandAsync(query, cancellationToken);

            command.AddParameter("@Id", entity.Id, DbType.Int32);
            command.AddParameter("@PessoaTipo", PessoaTipoColaborador, DbType.Int32);
            command.AddParameter("@PessoaId", entity.ColaboradorId, DbType.Int32);
            command.AddParameter("@DataHora", entity.DataHora, DbType.DateTime);

            var rows = await command.ExecuteNonQueryAsync(cancellationToken);
            if (rows == 0)
                throw new InfrastructureException("REGISTRO_NAO_ENCONTRADO", $"Nenhum acesso de colaborador encontrado com ID {entity.Id}.");

            return entity;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_ATUALIZAR_ACESSO_COLABORADOR", $"Erro ao atualizar acesso de colaborador: {ex.Message}", ex);
        }
    }

    public async Task<bool> Remover(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            string query = "DELETE FROM tb_acesso WHERE id_acesso = @Id AND pessoa_tipo = @PessoaTipo";
            await using var command = await CreateCommandAsync(query, cancellationToken);
            command.AddParameter("@Id", id, DbType.Int32);
            command.AddParameter("@PessoaTipo", PessoaTipoColaborador, DbType.Int32);

            return await command.ExecuteNonQueryAsync(cancellationToken) > 0;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_REMOVER_ACESSO_COLABORADOR", $"Erro ao remover acesso de colaborador: {ex.Message}", ex);
        }
    }

    public async Task<IEnumerable<AcessoColaborador>> ObterAcessosPorColaboradorPeriodo(int? colaboradorId = null, DateOnly? inicio = null, DateOnly? fim = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var conditions = new List<string>();
            if (colaboradorId.HasValue) conditions.Add("a.pessoa_id = @ColaboradorId");
            if (inicio.HasValue) conditions.Add("a.data_hora >= @Inicio");
            if (fim.HasValue) conditions.Add("a.data_hora < @Fim");

            string whereExtra = conditions.Count > 0 ? " AND " + string.Join(" AND ", conditions) : "";
            string query = $"{BaseSelectQuery}{whereExtra} ORDER BY a.data_hora ASC";

            await using var command = await CreateCommandAsync(query, cancellationToken);
            if (colaboradorId.HasValue) command.AddParameter("@ColaboradorId", colaboradorId.Value, DbType.Int32);
            if (inicio.HasValue) command.AddParameter("@Inicio", inicio.Value.ToDateTime(TimeOnly.MinValue), DbType.DateTime);
            if (fim.HasValue) command.AddParameter("@Fim", fim.Value.AddDays(1).ToDateTime(TimeOnly.MinValue), DbType.DateTime);

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            var lista = new List<AcessoColaborador>();
            while (await reader.ReadAsync(cancellationToken))
                lista.Add(Map(reader));

            return lista;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_OBTER_ACESSOS_PERIODO", $"Erro ao obter acessos do colaborador por período: {ex.Message}", ex);
        }
    }

    public async Task<AcessoColaborador?> ObterUltimoAcesso(int colaboradorId, CancellationToken cancellationToken = default)
    {
        try
        {
            string query = $"{BaseSelectQuery} AND a.pessoa_id = @ColaboradorId ORDER BY a.data_hora DESC, a.id_acesso DESC";
            await using var command = await CreateCommandAsync(query, cancellationToken);
            command.AddParameter("@ColaboradorId", colaboradorId, DbType.Int32);

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            return await reader.ReadAsync(cancellationToken) ? Map(reader) : null;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_OBTER_ULTIMO_ACESSO", $"Erro ao obter último acesso do colaborador {colaboradorId}: {ex.Message}", ex);
        }
    }

    public async Task<TimeSpan> ObterHorasTrabalhadasNoDia(int colaboradorId, DateOnly data, CancellationToken cancellationToken = default)
    {
        var acessos = (await ObterAcessosPorColaboradorPeriodo(colaboradorId, data, data, cancellationToken))
            .OrderBy(a => a.DataHora)
            .ToList();

        TimeSpan total = TimeSpan.Zero;
        for (int i = 0; i + 1 < acessos.Count; i += 2)
        {
            total += (acessos[i + 1].DataHora - acessos[i].DataHora);
        }
        return total;
    }

    private static AcessoColaborador Map(DbDataReader reader)
    {
        int id = reader.GetInt32Value("id_acesso");
        int colaboradorId = reader.GetInt32Value("pessoa_id");
        DateTime dataHora = Convert.ToDateTime(reader["data_hora"]);

        var logradouroDummy = Logradouro.Criar(1, "88500000", "Rua", "Centro", "Lages", "SC", "Brasil").Value!;
        var fotoBytes = reader.IsDBNull(reader.GetOrdinal("foto")) ? null : (byte[])reader["foto"];
        var arquivoFoto = fotoBytes != null && fotoBytes.Length > 0 ? Arquivo.Criar(fotoBytes).Value : null;

        var colabResult = Colaborador.Criar(
            id: colaboradorId,
            nome: reader.GetStringValue("nome"),
            cpf: reader.GetStringValue("cpf"),
            dataNascimento: DateOnly.FromDateTime(Convert.ToDateTime(reader["nascimento"])),
            telefone: reader.GetStringValue("telefone"),
            email: reader.GetStringValue("email"),
            endereco: logradouroDummy,
            numero: reader.GetStringValue("numero"),
            complemento: reader.IsDBNull(reader.GetOrdinal("complemento")) ? string.Empty : reader.GetStringValue("complemento"),
            senha: reader.GetStringValue("senha"),
            foto: arquivoFoto!,
            dataAdmissao: DateOnly.FromDateTime(Convert.ToDateTime(reader["admissao"])),
            tipo: (ColaboradorTipo)reader.GetInt32Value("tipo"),
            vinculo: (ColaboradorVinculo)reader.GetInt32Value("vinculo")
        );

        var acessoResult = AcessoColaborador.Criar(id, colabResult.Value!, dataHora);
        if (acessoResult.IsFailure)
            throw new InfrastructureException("ERRO_DOMINIO_MAPEAMENTO", $"Erro ao mapear AcessoColaborador ID {id}.");

        return acessoResult.Value!;
    }
}
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SharedLib.Data;
using SharedLib.Models;

namespace SharedLib.Services;

public class CrudService
{
    private readonly AppDbContext _db;
    public CrudService(AppDbContext db) => _db = db;

    // ===== Assessor =====
    public Assessor CreateAssessor(string nome, string cpf, string? email = null)
    {
        var a = new Assessor { Nome = nome, CPF = cpf, Email = email };
        _db.Assessores.Add(a);
        _db.SaveChanges();
        return a;
    }
    public IEnumerable<Assessor> ListAssessores() => _db.Assessores.AsNoTracking().ToList();

    // ===== Cliente =====
    public Cliente CreateCliente(string nome, string cpf, string? email = null, int? assessorId = null)
    {
        var c = new Cliente { Nome = nome, CPF = cpf, Email = email, AssessorId = assessorId };
        _db.Clientes.Add(c);
        _db.SaveChanges();
        // carteira padrão
        var car = new Carteira { ClienteId = c.Id, Nome = "Padrão" };
        _db.Carteiras.Add(car);
        _db.SaveChanges();
        return c;
    }
    public IEnumerable<Cliente> ListClientes() => _db.Clientes.AsNoTracking().ToList();

    // ===== Ativo =====
    public Ativo CreateAtivo(string ticker, string nome)
    {
        var a = new Ativo { Ticker = ticker, Nome = nome };
        _db.Ativos.Add(a);
        _db.SaveChanges();
        return a;
    }
    public IEnumerable<Ativo> ListAtivos() => _db.Ativos.AsNoTracking().ToList();

    // ===== Ordem =====
    public Ordem CreateOrdem(int clienteId, string ticker, string tipo, int quantidade, decimal preco, DateTime? data = null)
    {
        var ativo = _db.Ativos.FirstOrDefault(x => x.Ticker == ticker) ?? CreateAtivo(ticker, ticker);
        var o = new Ordem
        {
            ClienteId = clienteId,
            AtivoId = ativo.Id,
            Tipo = tipo.ToUpperInvariant(),
            Quantidade = quantidade,
            Preco = preco,
            Data = data ?? DateTime.UtcNow
        };
        _db.Ordens.Add(o);

        // atualiza posição na carteira padrão
        var carteira = _db.Carteiras.First(c => c.ClienteId == clienteId);
        var pos = _db.Posicoes.FirstOrDefault(p => p.CarteiraId == carteira.Id && p.AtivoId == ativo.Id);
        if (pos is null)
        {
            pos = new Posicao { CarteiraId = carteira.Id, AtivoId = ativo.Id, Quantidade = 0, PM = 0m };
            _db.Posicoes.Add(pos);
        }

        if (o.Tipo == "COMPRA")
        {
            var totalAnterior = pos.PM * pos.Quantidade;
            var totalNovo = totalAnterior + (o.Preco * o.Quantidade);
            pos.Quantidade += o.Quantidade;
            pos.PM = pos.Quantidade > 0 ? totalNovo / pos.Quantidade : 0m;
        }
        else if (o.Tipo == "VENDA")
        {
            pos.Quantidade -= o.Quantidade;
            if (pos.Quantidade < 0) pos.Quantidade = 0;
        }

        _db.SaveChanges();
        return o;
    }

    public IEnumerable<Ordem> ListOrdens() =>
        _db.Ordens.Include(o => o.Ativo).AsNoTracking().ToList();
}

public class FileService
{
    private readonly CrudService _crud;
    public FileService(CrudService crud) => _crud = crud;

    public int ImportClientesFromJson(string path)
    {
        var json = File.ReadAllText(path);
        var list = JsonSerializer.Deserialize<List<ClienteDto>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
        int n = 0;
        foreach (var c in list)
        {
            _crud.CreateCliente(c.Nome, c.CPF, c.Email, c.AssessorId);
            n++;
        }
        return n;
    }

    // TXT: cliente|cpf|email|assessorId
    public int ImportClientesFromTxt(string path)
    {
        var lines = File.ReadAllLines(path).Select(x => x.Trim()).Where(x => !string.IsNullOrWhiteSpace(x));
        int n = 0;
        foreach (var ln in lines)
        {
            var p = ln.Split('|');
            var nome = p.ElementAtOrDefault(0) ?? string.Empty;
            var cpf = p.ElementAtOrDefault(1) ?? string.Empty;
            var email = p.ElementAtOrDefault(2);
            int? assessorId = int.TryParse(p.ElementAtOrDefault(3), out var a) ? a : null;
            _crud.CreateCliente(nome, cpf, email, assessorId);
            n++;
        }
        return n;
    }

    public int ImportOrdensFromTxt(string path)
    {
        // ordem: clienteId|ticker|tipo|quantidade|preco|YYYY-MM-DDTHH:MM:SS
        var lines = File.ReadAllLines(path).Select(x => x.Trim()).Where(x => !string.IsNullOrWhiteSpace(x));
        int n = 0;
        foreach (var ln in lines)
        {
            var p = ln.Split('|');
            int clienteId = int.Parse(p.ElementAtOrDefault(0) ?? "0");
            string ticker = p.ElementAtOrDefault(1) ?? "";
            string tipo = p.ElementAtOrDefault(2) ?? "COMPRA";
            int qtd = int.Parse(p.ElementAtOrDefault(3) ?? "0");
            decimal preco = decimal.Parse(p.ElementAtOrDefault(4) ?? "0");
            DateTime? data = null;
            var ds = p.ElementAtOrDefault(5);
            if (!string.IsNullOrWhiteSpace(ds) && DateTime.TryParse(ds, out var d)) data = d;
            _crud.CreateOrdem(clienteId, ticker, tipo, qtd, preco, data);
            n++;
        }
        return n;
    }

    public string ExportOrdensToJson(string outDir = "export")
    {
        Directory.CreateDirectory(outDir);
        var items = _crud.ListOrdens().Select(o => new OrdemDto
        {
            Id = o.Id,
            ClienteId = o.ClienteId,
            Ticker = o.Ativo?.Ticker ?? "",
            Tipo = o.Tipo,
            Quantidade = o.Quantidade,
            Preco = o.Preco,
            Data = o.Data
        }).ToList();
        var json = JsonSerializer.Serialize(items, new JsonSerializerOptions { WriteIndented = true });
        var path = Path.Combine(outDir, $"ordens_{DateTime.Now:yyyyMMdd_HHmmss}.json");
        File.WriteAllText(path, json);
        return path;
    }
}

public class ClienteDto
{
    public string Nome { get; set; } = string.Empty;
    public string CPF { get; set; } = string.Empty;
    public string? Email { get; set; }
    public int? AssessorId { get; set; }
}

public class OrdemDto
{
    public int Id { get; set; }
    public int ClienteId { get; set; }
    public string Ticker { get; set; } = string.Empty;
    public string Tipo { get; set; } = "COMPRA";
    public int Quantidade { get; set; }
    public decimal Preco { get; set; }
    public DateTime Data { get; set; }
}

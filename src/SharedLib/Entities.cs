namespace SharedLib.Models;

public class Assessor
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string CPF { get; set; } = string.Empty;
    public string? Email { get; set; }
    public ICollection<Cliente> Clientes { get; set; } = new List<Cliente>();
}

public class Cliente
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string CPF { get; set; } = string.Empty;
    public string? Email { get; set; }

    public int? AssessorId { get; set; }
    public Assessor? Assessor { get; set; }

    public ICollection<Carteira> Carteiras { get; set; } = new List<Carteira>();
    public ICollection<Ordem> Ordens { get; set; } = new List<Ordem>();
}

public class Ativo
{
    public int Id { get; set; }
    public string Ticker { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public ICollection<Ordem> Ordens { get; set; } = new List<Ordem>();
}

public class Carteira
{
    public int Id { get; set; }
    public int ClienteId { get; set; }
    public Cliente? Cliente { get; set; }
    public string Nome { get; set; } = "Padrão";
    public ICollection<Posicao> Posicoes { get; set; } = new List<Posicao>();
}

public class Posicao
{
    public int Id { get; set; }
    public int CarteiraId { get; set; }
    public Carteira? Carteira { get; set; }
    public int AtivoId { get; set; }
    public Ativo? Ativo { get; set; }
    public int Quantidade { get; set; }
    public decimal PM { get; set; } // preço médio
}

public class Ordem
{
    public int Id { get; set; }
    public int ClienteId { get; set; }
    public Cliente? Cliente { get; set; }
    public int AtivoId { get; set; }
    public Ativo? Ativo { get; set; }
    public string Tipo { get; set; } = "COMPRA"; // COMPRA/VENDA
    public int Quantidade { get; set; }
    public decimal Preco { get; set; }
    public DateTime Data { get; set; } = DateTime.UtcNow;
}

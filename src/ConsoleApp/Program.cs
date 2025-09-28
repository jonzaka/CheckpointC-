using SharedLib.Data;
using SharedLib.Services;

var db = new AppDbContext();
var crud = new CrudService(db);
var files = new FileService(crud);

if (args.Length == 0)
{
    PrintHelp();
    return;
}

switch (args[0].ToLowerInvariant())
{
    case "assessor": HandleAssessor(args.Skip(1).ToArray(), crud); break;
    case "cliente":  HandleCliente(args.Skip(1).ToArray(), crud, files); break;
    case "ativo":    HandleAtivo(args.Skip(1).ToArray(), crud); break;
    case "ordem":    HandleOrdem(args.Skip(1).ToArray(), crud, files); break;
    default: PrintHelp(); break;
}

static void PrintHelp()
{
    Console.WriteLine("""
    Uso:
      assessor create --nome <str> --cpf <str> [--email <str>]
      assessor list

      cliente create --nome <str> --cpf <str> [--email <str>] [--assessorId <int>]
      cliente list
      cliente import-json <caminho>
      cliente import-txt <caminho>

      ativo create --ticker <str> --nome <str>
      ativo list

      ordem create --clienteId <int> --ticker <str> --tipo <COMPRA|VENDA> --quantidade <int> --preco <decimal> [--data <yyyy-mm-dd>]
      ordem list
      ordem import-txt <caminho>
      ordem export-json [<pasta_saida>]
    """.Trim());
}

static void HandleAssessor(string[] a, CrudService crud)
{
    var cmd = a.FirstOrDefault()?.ToLowerInvariant();
    if (cmd == "create")
    {
        var nome = GetOpt(a, "--nome") ?? throw new ArgumentException("--nome é obrigatório");
        var cpf  = GetOpt(a, "--cpf")  ?? throw new ArgumentException("--cpf é obrigatório");
        var email = GetOpt(a, "--email");
        var s = crud.CreateAssessor(nome, cpf, email);
        Console.WriteLine($"Criado: {s.Id} - {s.Nome}");
    }
    else if (cmd == "list")
    {
        foreach (var s in crud.ListAssessores())
            Console.WriteLine($"{s.Id} - {s.Nome} ({s.CPF})");
    }
    else Console.WriteLine("comando inválido");
}

static void HandleCliente(string[] a, CrudService crud, FileService files)
{
    var cmd = a.FirstOrDefault()?.ToLowerInvariant();
    switch (cmd)
    {
        case "create":
            {
                var nome = GetOpt(a, "--nome") ?? throw new ArgumentException("--nome é obrigatório");
                var cpf  = GetOpt(a, "--cpf")  ?? throw new ArgumentException("--cpf é obrigatório");
                var email = GetOpt(a, "--email");
                int? assessorId = int.TryParse(GetOpt(a, "--assessorId"), out var v) ? v : null;
                var c = crud.CreateCliente(nome, cpf, email, assessorId);
                Console.WriteLine($"Criado: {c.Id} - {c.Nome}");
                break;
            }
        case "list":
            foreach (var c in crud.ListClientes())
                Console.WriteLine($"{c.Id} - {c.Nome} ({c.CPF})");
            break;
        case "import-json":
            {
                var path = a.ElementAtOrDefault(1) ?? throw new ArgumentException("caminho JSON requerido");
                var n = files.ImportClientesFromJson(path);
                Console.WriteLine($"Importados {n} clientes de {path}");
                break;
            }
        case "import-txt":
            {
                var path = a.ElementAtOrDefault(1) ?? throw new ArgumentException("caminho TXT requerido");
                var n = files.ImportClientesFromTxt(path);
                Console.WriteLine($"Importados {n} clientes de {path}");
                break;
            }
        default:
            Console.WriteLine("comando inválido"); break;
    }
}

static void HandleAtivo(string[] a, CrudService crud)
{
    var cmd = a.FirstOrDefault()?.ToLowerInvariant();
    if (cmd == "create")
    {
        var ticker = GetOpt(a, "--ticker") ?? throw new ArgumentException("--ticker é obrigatório");
        var nome   = GetOpt(a, "--nome")   ?? throw new ArgumentException("--nome é obrigatório");
        var at = crud.CreateAtivo(ticker, nome);
        Console.WriteLine($"Criado: {at.Id} - {at.Ticker}");
    }
    else if (cmd == "list")
    {
        foreach (var at in crud.ListAtivos())
            Console.WriteLine($"{at.Id} - {at.Ticker} ({at.Nome})");
    }
    else Console.WriteLine("comando inválido");
}

static void HandleOrdem(string[] a, CrudService crud, FileService files)
{
    var cmd = a.FirstOrDefault()?.ToLowerInvariant();
    switch (cmd)
    {
        case "create":
            {
                int clienteId = int.Parse(GetOpt(a, "--clienteId") ?? throw new ArgumentException("--clienteId é obrigatório"));
                string ticker  = GetOpt(a, "--ticker") ?? throw new ArgumentException("--ticker é obrigatório");
                string tipo    = GetOpt(a, "--tipo") ?? "COMPRA";
                int qtd        = int.Parse(GetOpt(a, "--quantidade") ?? "0");
                decimal preco  = decimal.Parse(GetOpt(a, "--preco") ?? "0");
                DateTime? data = null;
                var ds = GetOpt(a, "--data");
                if (!string.IsNullOrWhiteSpace(ds) && DateTime.TryParse(ds, out var d)) data = d;
                var o = crud.CreateOrdem(clienteId, ticker, tipo, qtd, preco, data);
                Console.WriteLine($"Criada: {o.Id} - {o.Tipo} {o.Quantidade}x {ticker} @ {o.Preco}");
                break;
            }
        case "list":
            foreach (var o in crud.ListOrdens())
                Console.WriteLine($"{o.Id} - {o.Tipo} {o.Quantidade}x {o.Ativo?.Ticker} @ {o.Preco} ({o.Data:yyyy-MM-dd})");
            break;
        case "import-txt":
            {
                var path = a.ElementAtOrDefault(1) ?? throw new ArgumentException("caminho TXT requerido");
                var n = files.ImportOrdensFromTxt(path);
                Console.WriteLine($"Importadas {n} ordens de {path}");
                break;
            }
        case "export-json":
            {
                var outDir = a.ElementAtOrDefault(1) ?? "export";
                Directory.CreateDirectory(outDir);
                var path = files.ExportOrdensToJson(outDir);
                Console.WriteLine($"Exportado para {path}");
                break;
            }
        default:
            Console.WriteLine("comando inválido"); break;
    }
}

static string? GetOpt(string[] a, string name)
{
    var idx = Array.FindIndex(a, x => x.Equals(name, StringComparison.OrdinalIgnoreCase));
    if (idx >= 0 && idx + 1 < a.Length) return a[idx + 1];
    return null;
}

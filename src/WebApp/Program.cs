using Microsoft.EntityFrameworkCore;
using SharedLib.Data;
using SharedLib.Models;
using SharedLib.Services;

var builder = WebApplication.CreateBuilder(args);

// Usa Oracle se ORACLE_CONN estiver definido; senão, SQLite local.
var oracleConn = Environment.GetEnvironmentVariable("ORACLE_CONN");
builder.Services.AddDbContext<AppDbContext>(opt =>
{
    if (!string.IsNullOrWhiteSpace(oracleConn))
        opt.UseOracle(oracleConn);
    else
    {
        var dbPath = Path.Combine(AppContext.BaseDirectory, "db.sqlite3");
        opt.UseSqlite($"Data Source={dbPath}");
    }
});

builder.Services.AddScoped<CrudService>();
builder.Services.AddScoped<FileService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/", () => Results.Redirect("/swagger"));

// ===== Endpoints =====

// Assessores
app.MapGet("/api/assessores", (CrudService crud) =>
{
    return Results.Ok(crud.ListAssessores());
});

// Clientes
app.MapGet("/api/clientes", (CrudService crud) =>
{
    return Results.Ok(crud.ListClientes());
});
app.MapPost("/api/clientes", (CrudService crud, Cliente c) =>
{
    var created = crud.CreateCliente(c.Nome, c.CPF, c.Email, c.AssessorId);
    return Results.Created($"/api/clientes/{created.Id}", created);
});

// Ativos
app.MapGet("/api/ativos", (CrudService crud) =>
{
    return Results.Ok(crud.ListAtivos());
});
app.MapPost("/api/ativos", (CrudService crud, Ativo a) =>
{
    var created = crud.CreateAtivo(a.Ticker, a.Nome);
    return Results.Created($"/api/ativos/{created.Id}", created);
});

// Ordens
app.MapGet("/api/ordens", (CrudService crud) =>
{
    return Results.Ok(crud.ListOrdens());
});
app.MapPost("/api/ordens", (CrudService crud, Ordem o) =>
{
    var created = crud.CreateOrdem(o.ClienteId, o.Ativo?.Ticker ?? "", o.Tipo, o.Quantidade, o.Preco, o.Data);
    return Results.Created($"/api/ordens/{created.Id}", created);
});

app.Run();

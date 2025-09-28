# CheckpointC-
# Assessor de Investimentos — Checkpoint C#

## Autores
- Breno Silva — RM97864  
- Enrico Marquez — RM99325  
- Gustavo Dias — RM550820  
- Joel Barros — RM550378  
- Leonardo Moreira — RM550988

---

## Como rodar

### ConsoleApp
```bash
dotnet run --project src/ConsoleApp -- cliente create --nome "Maria" --cpf "11122233344" --email "maria@mail.com"
dotnet run --project src/ConsoleApp -- ativo create --ticker "PETR4" --nome "Petrobras PN"
dotnet run --project src/ConsoleApp -- ordem create --clienteId 1 --ticker "PETR4" --tipo COMPRA --quantidade 10 --preco 37.50
dotnet run --project src/ConsoleApp -- ordem list

# Relatório técnico — Assessor de Investimentos (MVP)

## Autores
- Breno Silva — RM97864  
- Enrico Marquez — RM99325  
- Gustavo Dias — RM550820  
- Joel Barros — RM550378  
- Leonardo Moreira — RM550988

---

## Objetivo
MVP para apoiar um assessor de investimentos: cadastro de assessores e clientes, registro de ativos e ordens, posição de carteira e importação/exportação TXT/JSON.

## Arquitetura
- .NET 8 — SharedLib (Models, DbContext, Services), ConsoleApp (CLI), WebApp (Minimal API + Swagger).
- Persistência: EF Core Oracle (via ORACLE_CONN) e fallback para SQLite (db.sqlite3).

## Decisões
- Carteira padrão criada na criação do cliente (simplifica a demo).
- PM atualizado a cada ordem (modelo simples do MVP).

## Limitações e próximos passos
- Autenticação e autorização ausentes (futuro: JWT).
- Regras de negócio simplificadas (margem, liquidação).
- UI mais rica (Razor/Blazor) e testes E2E.

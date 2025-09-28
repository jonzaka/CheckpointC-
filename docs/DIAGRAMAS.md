# Diagramas (Mermaid)

## Autores
- Breno Silva — RM97864  
- Enrico Marquez — RM99325  
- Gustavo Dias — RM550820  
- Joel Barros — RM550378  
- Leonardo Moreira — RM550988

---

## Módulos
```mermaid
flowchart LR
  subgraph SharedLib
    DB[(Oracle/SQLite via EF Core)]
    MODELS[Entities: Assessor, Cliente, Ativo, Carteira, Posicao, Ordem]
    SERVICES[CrudService + FileService]
  end

  subgraph Interfaces
    CLI[ConsoleApp]
    WEB[WebApp (Minimal API + Swagger)]
  end

  CLI --> SERVICES
  WEB --> SERVICES
  SERVICES <--> DB
  MODELS --> SERVICES

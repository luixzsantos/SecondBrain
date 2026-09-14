# 🧠 SecondBrain

Dicionário técnico pessoal + mapa de conhecimento. API em **C# / ASP.NET Core** que responde a pergunta
"o que eu sei sobre X, e onde já usei isso?" — começando pequeno (V0.1: CRUD de `Concept`) e crescendo
incrementalmente até virar um grafo de conhecimento pessoal integrável a Obsidian, GitHub e LLMs.

Este README documenta a **V0.1 — Foundation**.

---

## Índice

- [Visão do produto](#visão-do-produto)
- [Arquitetura](#arquitetura)
- [Stack](#stack)
- [Estrutura de pastas](#estrutura-de-pastas)
- [Pré-requisitos](#pré-requisitos)
- [Como rodar](#como-rodar)
- [Uso da API](#uso-da-api)
- [Testes](#testes)
- [Segurança](#segurança)
- [Decisões desta versão](#decisões-desta-versão)
- [Roadmap](#roadmap)

---

## Visão do produto

Separar **conhecimento** ("o que é Redis?"), **experiência** ("como eu usei Redis?"), **projeto** ("em qual
projeto?") e **decisão** ("por que Redis e não RabbitMQ?") em vez de jogar tudo num bloco de texto genérico
(estilo Notion). A V0.1 modela só a primeira peça — `Concept` — pra validar a base (API, banco, testes,
arquitetura) antes de crescer pras relações entre entidades.

## Arquitetura

Clean Architecture em 4 camadas, dependência sempre de fora pra dentro:

```
SecondBrain.API            → Controllers, Swagger, middleware de erro, DI, appsettings
      ↓ depende de
SecondBrain.Infrastructure  → EF Core + Npgsql, DbContext, repositórios (implementa Application)
      ↓ depende de
SecondBrain.Application     → DTOs, interfaces (IConceptRepository/IConceptService), regra de negócio
      ↓ depende de
SecondBrain.Domain          → Entidades puras (Concept), sem dependência de nada externo
```

**Por quê Clean Architecture pra um CRUD que hoje é simples:** o projeto existe pra virar um grafo de
conhecimento (relações N:N entre Concept/Note/Project/Tag, depois busca semântica e integração com
Obsidian/GitHub/LLMs) e também é usado como estudo de C#/ASP.NET Core — vale pagar o custo de indireção
agora, com poucas camadas, pra ter fronteiras claras quando a regra de negócio parar de ser trivial. Se o
projeto não crescesse além de um CRUD, isso seria overengineering.

**Nome único de `Concept`** é garantido em duas camadas: o `ConceptService` checa duplicidade antes de
gravar (retorna 409 direto, sem round-trip de exceção de banco) e o banco tem um índice único (`IX_concepts_Name`)
como garantia final contra condição de corrida.

## Stack

- **.NET 8** (LTS) / ASP.NET Core Web API
- **Entity Framework Core** + **Npgsql** (PostgreSQL)
- **PostgreSQL 16**
- **Swagger / OpenAPI** (Swashbuckle)
- **xUnit** (testes unitários e de integração via `WebApplicationFactory`)
- **Docker Compose** (Postgres local)

Sem framework de validação externo (FluentValidation) nem mock library (Moq) — Data Annotations e um
repositório fake resolvem com poucas linhas o que a V0.1 precisa (ver [Dependências](#decisões-desta-versão)).

## Estrutura de pastas

```
second-brain/
├── src/
│   ├── SecondBrain.API/            # Controllers, Program.cs, middleware, appsettings
│   ├── SecondBrain.Application/    # DTOs, interfaces, services (regra de negócio)
│   ├── SecondBrain.Domain/         # Entidades (Concept)
│   └── SecondBrain.Infrastructure/ # DbContext, migrations, repositórios EF Core
├── tests/
│   ├── SecondBrain.UnitTests/        # ConceptService + repositório fake em memória
│   └── SecondBrain.IntegrationTests/ # API real via WebApplicationFactory + EF InMemory
├── compose.yml        # Postgres local
├── .env.example
└── SecondBrain.sln
```

## Pré-requisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Docker + Docker Compose (ou um PostgreSQL local, se preferir não usar Docker)
- Ferramenta `dotnet-ef` (`dotnet tool install --global dotnet-ef`)

## Como rodar

```bash
# 1. Subir o Postgres
docker compose up -d postgres

# 2. Aplicar as migrations (cria o banco/tabelas)
dotnet ef database update --project src/SecondBrain.Infrastructure --startup-project src/SecondBrain.API

# 3. Rodar a API
dotnet run --project src/SecondBrain.API
```

A API sobe em `http://localhost:5080` (ou na porta do `launchSettings.json`) com Swagger em `/swagger`.

Em desenvolvimento, a connection string já vem configurada em `appsettings.Development.json` apontando pro
Postgres do `compose.yml` (usuário/senha `secondbrain`/`secondbrain_dev` — só vale para ambiente local). Em
outros ambientes, defina a variável `ConnectionStrings__DefaultConnection` (nunca commitar credencial real —
ver `.env.example`).

> **Nota Docker Desktop no Windows:** se `docker compose up` travar com erro de `sailor-ingest.sock`, é um bug
> conhecido do Docker Desktop (socket travado de uma sessão anterior). Reiniciar o Windows resolve; ou, via
> WSL, `wsl -d docker-desktop -- rm -f /mnt/c/Users/<você>/AppData/Local/Docker/run/sailor-ingest.sock*`.

## Uso da API

```bash
# Criar um concept
curl -X POST http://localhost:5080/api/concepts \
  -H "Content-Type: application/json" \
  -d '{"name":"Redis","description":"Banco de dados em memória, usado para cache, filas e pub/sub."}'

# Listar
curl http://localhost:5080/api/concepts

# Buscar por id
curl http://localhost:5080/api/concepts/{id}

# Atualizar
curl -X PUT http://localhost:5080/api/concepts/{id} \
  -H "Content-Type: application/json" \
  -d '{"name":"Redis","description":"Descrição atualizada."}'

# Remover
curl -X DELETE http://localhost:5080/api/concepts/{id}
```

Respostas de erro seguem um formato consistente (`ExceptionHandlingMiddleware`):

```json
{ "status": 404, "title": "Concept '...' não encontrado.", "traceId": "..." }
```

| Situação | Status |
|---|---|
| Sucesso (GET/PUT) | 200 |
| Criado (POST) | 201 |
| Sem corpo (DELETE) | 204 |
| Validação (`name` vazio, `description` maior que o limite) | 400 |
| Não encontrado | 404 |
| Nome duplicado | 409 |

## Testes

```bash
dotnet test
```

- **Unit**: `ConceptService` contra um repositório fake em memória (sem mock library, sem banco).
- **Integração**: API real (`WebApplicationFactory<Program>`) com EF Core InMemory no lugar do Postgres —
  cobre roteamento, serialização e DI de ponta a ponta. Limitação conhecida: o provider InMemory não reproduz
  o índice único do Postgres, então o cenário de nome duplicado só é coberto no unit test.

## Segurança

- Sem secrets no código: connection string de dev fica só em `appsettings.Development.json` (credenciais
  locais, sem uso fora da máquina de dev); produção usa variável de ambiente.
- Erros não tratados nunca vazam stack trace pro cliente (middleware global retorna mensagem genérica + loga
  o detalhe no servidor).
- Entrada validada via Data Annotations antes de chegar na regra de negócio.
- Ainda sem autenticação — API de uso pessoal/local por enquanto; entra no roadmap (V0.3).

## Decisões desta versão

- **Sem `KnowledgeRelation` genérico ainda.** Uma tabela `Source/Target` polimórfica (Concept↔Note↔Project…)
  não tem integridade referencial real em EF Core/Postgres. Quando Note/Project entrarem, a relação
  Concept↔Concept vai ser uma FK normal (auto-relacionamento) e "usado em Projeto"/"explicado em Nota" vão
  ser tabelas de junção tipadas — o grafo genérico fica pra quando/se isso realmente for necessário.
- **Sem FluentValidation/Moq.** Data Annotations cobrem a validação da V0.1 e um repositório fake cobre os
  testes — adicionar uma lib pra isso agora seria peso sem ganho real.
- **.NET 8 (LTS)**, não a versão mais nova instalada na máquina — prioriza maturidade de tooling/documentação
  pra um projeto que também é estudo de C#/ASP.NET Core.

## Roadmap

- **V0.2** — Note, Tag, Project + relações (`ConceptNote`, `ConceptProject` N:N) + busca (full-text do
  Postgres, `tsvector`).
- **V0.3** — Users, login, JWT.
- **V0.4** — Experience, Decision, grafo de conhecimento relacionado.
- **V0.5** — Redis, background workers, observabilidade.
- **V1.0** — Integração com Obsidian, GitHub, embeddings/busca semântica, assistente via LLM.

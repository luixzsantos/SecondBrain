# 🧠 SecondBrain

Dicionário técnico pessoal + mapa de conhecimento. API em **C# / ASP.NET Core** que responde a pergunta
"o que eu sei sobre X, e onde já usei isso?" — começando pequeno (V0.1: CRUD de `Concept`) e crescendo
incrementalmente até virar um grafo de conhecimento pessoal integrável a Obsidian, GitHub e LLMs.

Este README documenta até a **V0.2 — Knowledge** (Note, Tag, Project, relações N:N e busca full-text).

---

## Índice

- [Visão do produto](#visão-do-produto)
- [Arquitetura](#arquitetura)
- [Modelo de dados](#modelo-de-dados)
- [Stack](#stack)
- [Estrutura de pastas](#estrutura-de-pastas)
- [Pré-requisitos](#pré-requisitos)
- [Como rodar](#como-rodar)
- [Uso da API](#uso-da-api)
- [Busca full-text](#busca-full-text)
- [Testes](#testes)
- [Segurança](#segurança)
- [Decisões desta versão](#decisões-desta-versão)
- [Roadmap](#roadmap)

---

## Visão do produto

Separar **conhecimento** ("o que é Redis?"), **experiência** ("como eu usei Redis?"), **projeto** ("em qual
projeto?") e **decisão** ("por que Redis e não RabbitMQ?") em vez de jogar tudo num bloco de texto genérico
(estilo Notion). A "página do conceito" (`GET /api/concepts/{id}`) já mostra a visão completa: descrição +
notas, projetos e tags relacionados.

## Arquitetura

Clean Architecture em 4 camadas, dependência sempre de fora pra dentro:

```
SecondBrain.API            → Controllers, Swagger, middleware de erro, DI, appsettings
      ↓ depende de
SecondBrain.Infrastructure  → EF Core + Npgsql, DbContext, repositórios (implementa Application)
      ↓ depende de
SecondBrain.Application     → DTOs, interfaces, services (regra de negócio)
      ↓ depende de
SecondBrain.Domain          → Entidades puras, sem dependência de nada externo
```

**Por quê Clean Architecture pra um CRUD que começou simples:** o projeto existe pra virar um grafo de
conhecimento (relações N:N, depois busca semântica e integração com Obsidian/GitHub/LLMs) e também é usado
como estudo de C#/ASP.NET Core — vale pagar o custo de indireção agora, com poucas camadas, pra ter fronteiras
claras quando a regra de negócio parar de ser trivial.

**Nome único** (`Concept.Name`, `Tag.Name`) é garantido em duas camadas: o service checa duplicidade antes de
gravar (retorna 409 direto) e o banco tem um índice único como garantia final contra condição de corrida.

## Modelo de dados

```
Concept ──┬── ConceptNote ──── Note       ("explicado em")
          ├── ConceptProject ─ Project    ("usado em")
          └── ConceptTag ───── Tag
```

Cada relação é uma **tabela de junção tipada** (`concept_notes`, `concept_projects`, `concept_tags`), não uma
tabela `KnowledgeRelation` genérica/polimórfica — ver [Decisões](#decisões-desta-versão). `Project.Status` é
um enum (`Active`/`Paused`/`Completed`/`Archived`), serializado como texto no JSON.

## Stack

- **.NET 8** (LTS) / ASP.NET Core Web API
- **Entity Framework Core** + **Npgsql** (PostgreSQL) — inclusive full-text search nativo (`tsvector`/`tsquery`)
- **PostgreSQL 16**
- **Swagger / OpenAPI** (Swashbuckle)
- **xUnit** (testes unitários e de integração via `WebApplicationFactory`)
- **Docker Compose** (Postgres local)

Sem framework de validação externo (FluentValidation) nem mock library (Moq) — Data Annotations e repositórios
fake resolvem com poucas linhas o que o projeto precisa até aqui (ver [Decisões](#decisões-desta-versão)).

## Estrutura de pastas

```
second-brain/
├── src/
│   ├── SecondBrain.API/            # Controllers, Program.cs, middleware, appsettings
│   ├── SecondBrain.Application/    # DTOs, interfaces, services (regra de negócio)
│   ├── SecondBrain.Domain/         # Entidades (Concept, Note, Tag, Project, ConceptNote/Project/Tag)
│   └── SecondBrain.Infrastructure/ # DbContext, migrations, repositórios EF Core
├── tests/
│   ├── SecondBrain.UnitTests/        # Services contra repositórios fake em memória
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
# Concepts
curl -X POST http://localhost:5080/api/concepts -H "Content-Type: application/json" \
  -d '{"name":"Redis","description":"Banco de dados em memória, usado para cache, filas e pub/sub."}'
curl http://localhost:5080/api/concepts                # lista
curl http://localhost:5080/api/concepts/{id}           # "página do conceito": + notes/projects/tags relacionados
curl -X PUT http://localhost:5080/api/concepts/{id} -H "Content-Type: application/json" -d '{...}'
curl -X DELETE http://localhost:5080/api/concepts/{id}

# Notes (ConceptIds é opcional — já relaciona na criação)
curl -X POST http://localhost:5080/api/notes -H "Content-Type: application/json" \
  -d '{"title":"Redis Streams na prática","content":"...","conceptIds":["<concept-id>"]}'

# Projects (status: Active|Paused|Completed|Archived)
curl -X POST http://localhost:5080/api/projects -H "Content-Type: application/json" \
  -d '{"name":"Notification Engine","description":"...","status":"Active"}'

# Tags
curl -X POST http://localhost:5080/api/tags -H "Content-Type: application/json" -d '{"name":"database"}'

# Relacionar/desrelacionar depois de já criados
curl -X POST   http://localhost:5080/api/concepts/{conceptId}/notes/{noteId}
curl -X DELETE http://localhost:5080/api/concepts/{conceptId}/notes/{noteId}
curl -X POST   http://localhost:5080/api/concepts/{conceptId}/projects/{projectId}
curl -X POST   http://localhost:5080/api/concepts/{conceptId}/tags/{tagId}
```

Respostas de erro seguem um formato consistente (`ExceptionHandlingMiddleware`):

```json
{ "status": 404, "title": "Concept '...' não encontrado.", "traceId": "..." }
```

| Situação | Status |
|---|---|
| Sucesso (GET/PUT) | 200 |
| Criado (POST) | 201 |
| Sem corpo (DELETE, link/unlink) | 204 |
| Validação de entrada | 400 |
| Não encontrado (Concept/Note/Project/Tag ou a relação) | 404 |
| Nome duplicado / relação já existe | 409 |

## Busca full-text

```bash
curl "http://localhost:5080/api/search?q=redis"
```

Procura em `Concept` (nome+descrição), `Note` (título+conteúdo) e `Project` (nome+descrição) usando o
full-text search nativo do Postgres (`to_tsvector`/`plainto_tsquery`), não `LIKE`. Config `simple` (sem
stemming) em vez de `portuguese`: o vocabulário é bilíngue (termos técnicos em inglês + texto em português), e
stemming de português aplicado a palavras em inglês dava resultado imprevisível.

```json
{ "query": "redis", "results": [{ "type": "concept", "id": "...", "title": "Redis" }, { "type": "note", "...": "..." }] }
```

## Testes

```bash
dotnet test
```

74 testes (unit + integração). **Unit**: cada service contra repositórios fake em memória (sem mock library,
sem banco). **Integração**: API real (`WebApplicationFactory<Program>`) com EF Core InMemory no lugar do
Postgres — cobre roteamento, serialização, DI e as relações N:N de ponta a ponta.

Limitações conhecidas do InMemory provider (por isso também validado manualmente contra Postgres real a cada
versão): não reproduz o índice único do Postgres (nome duplicado só é coberto no unit test) e não sabe traduzir
`EF.Functions.ToTsVector`/`PlainToTsQuery` — a busca full-text não tem teste de integração automatizado ainda,
só validação manual.

## Segurança

- Sem secrets no código: connection string de dev fica só em `appsettings.Development.json` (credenciais
  locais, sem uso fora da máquina de dev); produção usa variável de ambiente.
- Erros não tratados nunca vazam stack trace pro cliente (middleware global retorna mensagem genérica + loga
  o detalhe no servidor).
- Entrada validada via Data Annotations antes de chegar na regra de negócio.
- Ainda sem autenticação — API de uso pessoal/local por enquanto; entra no roadmap (V0.3).

## Decisões desta versão

- **Sem `KnowledgeRelation` genérico.** Uma tabela `Source/Target` polimórfica (Concept↔Note↔Project…) não tem
  integridade referencial real em EF Core/Postgres. Cada par de tipos tem sua própria tabela de junção
  (`concept_notes`, `concept_projects`, `concept_tags`) com FK de verdade.
- **Busca full-text calculada em tempo de consulta, sem coluna gerada + índice GIN ainda.** Funciona correto
  hoje; dataset pessoal de baixo volume não justifica a complexidade extra até isso realmente doer
  (performance medida depois, não otimizada antes de existir problema).
- **Enums como texto no JSON** (`JsonStringEnumConverter`) em vez de número — número puro é opaco pra quem
  consome a API e frágil se a ordem do enum mudar.
- **Sem FluentValidation/Moq.** Data Annotations cobrem a validação e repositórios fake cobrem os testes —
  adicionar uma lib pra isso agora seria peso sem ganho real.
- **.NET 8 (LTS)**, não a versão mais nova instalada na máquina — prioriza maturidade de tooling/documentação
  pra um projeto que também é estudo de C#/ASP.NET Core.

## Roadmap

- **V0.3** — Users, login, JWT.
- **V0.4** — Experience, Decision, grafo de conhecimento relacionado.
- **V0.5** — Redis, background workers, observabilidade.
- **V1.0** — Integração com Obsidian (importar `.md` do vault como Notes), GitHub, embeddings/busca semântica,
  assistente via LLM.

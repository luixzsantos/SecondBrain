<#
Importa o conhecimento de linguagens/bibliotecas real (extraido dos projetos
do usuario) para o SecondBrain: cada arquivo em scripts/data/language-knowledge/
vira uma Tag (a linguagem), varios Concepts (um por topico, basico->avancado)
e uma Note por Concept com a explicacao completa + o codigo real do projeto,
ja relacionada ao Concept e ao Project de origem.

Idempotente: casa Concept por nome, Note por titulo, Tag por nome e Project
por nome - roda de novo sem duplicar, so atualiza a descricao/conteudo.
Requer a API do SecondBrain no ar (rode start.bat primeiro).
#>

$ErrorActionPreference = "Stop"
$ProgressPreference = "SilentlyContinue"

$ApiBase = "http://localhost:5080/api"
$DataDir = Join-Path $PSScriptRoot "data\language-knowledge"

function Invoke-JsonApi {
    param([string]$Uri, [string]$Method, [hashtable]$Payload)

    $json = $Payload | ConvertTo-Json -Depth 6
    $bytes = [System.Text.Encoding]::UTF8.GetBytes($json)
    return Invoke-RestMethod -Uri $Uri -Method $Method -Body $bytes -ContentType "application/json; charset=utf-8"
}

function Invoke-JsonApiIgnoreConflict {
    param([string]$Uri, [string]$Method)
    try {
        Invoke-RestMethod -Uri $Uri -Method $Method | Out-Null
    } catch {
        $status = $_.Exception.Response.StatusCode.value__
        if ($status -ne 409) { throw }
    }
}

function Test-ApiUp {
    try {
        Invoke-RestMethod -Uri "$ApiBase/concepts" -Method Get -TimeoutSec 3 | Out-Null
        return $true
    } catch {
        return $false
    }
}

if (-not (Test-ApiUp)) {
    Write-Host "[ERRO] A API do SecondBrain nao esta respondendo em $ApiBase." -ForegroundColor Red
    Write-Host "Rode start.bat primeiro e tente de novo." -ForegroundColor Red
    exit 1
}

if (-not (Test-Path $DataDir)) {
    Write-Host "[ERRO] Pasta de dados nao encontrada: $DataDir" -ForegroundColor Red
    exit 1
}

# --- caches (uma consulta cada, reaproveitados por todos os arquivos) ---
$conceptByName = @{}
foreach ($c in (Invoke-RestMethod -Uri "$ApiBase/concepts" -Method Get)) { $conceptByName[$c.name.ToLower()] = $c }

$noteByTitle = @{}
foreach ($n in (Invoke-RestMethod -Uri "$ApiBase/notes" -Method Get)) { $noteByTitle[$n.title.ToLower()] = $n }

$tagByName = @{}
foreach ($t in (Invoke-RestMethod -Uri "$ApiBase/tags" -Method Get)) { $tagByName[$t.name.ToLower()] = $t }

$projectByName = @{}
foreach ($p in (Invoke-RestMethod -Uri "$ApiBase/projects" -Method Get)) { $projectByName[$p.name.ToLower()] = $p }

$fenceByTag = @{
    go = "go"; csharp = "csharp"; cpp = "cpp"; python = "python"; java = "java"; javascript = "javascript"
    redis = "bash"; postgresql = "sql"; html = "html"
}
# 3 backticks como variavel evita ter que escapar backtick dentro de string
# interpolada do PowerShell (onde backtick e o caractere de escape).
$fence3 = [string]::new([char]96, 3)

# Projetos reais que vieram do GitHub de um colaborador (nao do usuario) - a
# descricao do Project precisa deixar isso explicito, em vez de dizer "projeto
# real do usuario" pra algo que na verdade e codigo de outra pessoa.
$contributorProjects = @{
    "personal finance" = "GustavoMelo1"
    "daily routine app" = "GustavoMelo1"
    "front-end studies" = "GustavoMelo1"
    "financial analysis" = "GustavoMelo1"
}

# O JSON traz "level" em portugues com acento (basico/intermediario/avancado) -
# nao bate com os nomes do enum ConceptLevel no C# (sem acento, PascalCase),
# entao precisa mapear antes de mandar pra API.
$levelMap = @{
    "basico" = "Basico"
    "intermediario" = "Intermediario"
    "avancado" = "Avancado"
}

function Get-ConceptLevel {
    param([string]$Level)
    if (-not $Level) { return $null }
    $normalized = $Level.ToLower() -replace "[áàâã]", "a" -replace "[éê]", "e" -replace "í", "i" -replace "[óô]", "o" -replace "ç", "c"
    if ($levelMap.ContainsKey($normalized)) { return $levelMap[$normalized] }
    return $null
}

$totalCreated = 0
$totalUpdated = 0
$totalFailed = 0

Get-ChildItem -Path $DataDir -Filter "*.json" | ForEach-Object {
    $data = [System.IO.File]::ReadAllText($_.FullName, [System.Text.Encoding]::UTF8) | ConvertFrom-Json
    Write-Host ""
    Write-Host "=== $($data.language) ($($data.concepts.Count) topicos) ===" -ForegroundColor Cyan

    # Tag da linguagem
    $tagKey = $data.tag.ToLower()
    if ($tagByName.ContainsKey($tagKey)) {
        $tagId = $tagByName[$tagKey].id
    } else {
        $tagResp = Invoke-JsonApi -Uri "$ApiBase/tags" -Method Post -Payload @{ name = $data.tag }
        $tagByName[$tagKey] = $tagResp
        $tagId = $tagResp.id
    }

    foreach ($concept in $data.concepts) {
        try {
            # Project de origem
            $projKey = $concept.projectName.ToLower()
            if ($projectByName.ContainsKey($projKey)) {
                $projectId = $projectByName[$projKey].id
            } else {
                $isGenericRef = $concept.projectName -like "Referencia Geral*" -or $concept.projectName -like "Refer*ncia Geral*"
                $contributor = $contributorProjects[$concept.projectName.ToLower()]
                $projDescription = if ($isGenericRef) {
                    "Conteudo de referencia geral da linguagem, nao vinculado a um repositorio especifico do usuario."
                } elseif ($contributor) {
                    "Projeto real do colaborador $contributor (GitHub), fonte dos exemplos de codigo importados."
                } else {
                    "Projeto real do usuario, fonte dos exemplos de codigo importados."
                }
                $projResp = Invoke-JsonApi -Uri "$ApiBase/projects" -Method Post -Payload @{ name = $concept.projectName; description = $projDescription }
                $projectByName[$projKey] = $projResp
                $projectId = $projResp.id
            }

            # Concept (verbete)
            $conceptKey = $concept.name.ToLower()
            $fence = $fenceByTag[$data.tag]
            $conceptLevel = Get-ConceptLevel -Level $concept.level
            if ($conceptByName.ContainsKey($conceptKey)) {
                $conceptId = $conceptByName[$conceptKey].id
                Invoke-JsonApi -Uri "$ApiBase/concepts/$conceptId" -Method Put -Payload @{ name = $concept.name; description = $concept.shortDescription; level = $conceptLevel } | Out-Null
                $script:totalUpdated++
            } else {
                $conceptResp = Invoke-JsonApi -Uri "$ApiBase/concepts" -Method Post -Payload @{ name = $concept.name; description = $concept.shortDescription; level = $conceptLevel }
                $conceptByName[$conceptKey] = $conceptResp
                $conceptId = $conceptResp.id
                $script:totalCreated++
            }

            # Note: primeiro uma analogia simples pra quem nao entende de programacao,
            # depois o nivel e a explicacao tecnica + codigo, e so no final o
            # repositorio real onde esse mesmo codigo foi usado - nessa ordem de
            # leitura (do mais simples ao mais tecnico, terminando no "isso e real").
            $noteTitle = "$($concept.name) (nota completa)"
            $noteLines = @(
                "## Em termos simples"
                ""
                $concept.simpleAnalogy
                ""
                "**Nivel:** $($concept.level)"
                ""
                $concept.explanation
                ""
                "## Exemplo de codigo"
                ""
                "$fence3$fence"
                $concept.codeExample
                $fence3
            )
            if ($concept.PSObject.Properties.Name -contains "resposta" -and $concept.resposta) {
                $noteLines += ""
                $noteLines += "## Resposta esperada"
                $noteLines += ""
                $noteLines += $concept.resposta
            }
            $noteLines += @(
                ""
                "## Onde isso aparece na pratica"
                ""
                "**Repositorio:** $($concept.projectName)"
                ""
                $concept.whereUsed
            )
            if ($concept.PSObject.Properties.Name -contains "documentationUrl" -and $concept.documentationUrl) {
                $noteLines += ""
                $noteLines += "**Documentacao oficial:** $($concept.documentationUrl)"
            }
            $noteContent = $noteLines -join "`n"
            $noteKey = $noteTitle.ToLower()
            if ($noteByTitle.ContainsKey($noteKey)) {
                $noteId = $noteByTitle[$noteKey].id
                Invoke-JsonApi -Uri "$ApiBase/notes/$noteId" -Method Put -Payload @{ title = $noteTitle; content = $noteContent } | Out-Null
            } else {
                $noteResp = Invoke-JsonApi -Uri "$ApiBase/notes" -Method Post -Payload @{ title = $noteTitle; content = $noteContent }
                $noteByTitle[$noteKey] = $noteResp
                $noteId = $noteResp.id
                Invoke-JsonApiIgnoreConflict -Uri "$ApiBase/concepts/$conceptId/notes/$noteId" -Method Post
            }

            # Relacionamentos: Concept <-> Project e Concept <-> Tag (idempotente, ignora 409)
            Invoke-JsonApiIgnoreConflict -Uri "$ApiBase/concepts/$conceptId/projects/$projectId" -Method Post
            Invoke-JsonApiIgnoreConflict -Uri "$ApiBase/concepts/$conceptId/tags/$tagId" -Method Post

            Write-Host "  OK  $($concept.name)" -ForegroundColor Green
        } catch {
            Write-Host "  FALHOU  $($concept.name) -- $_" -ForegroundColor Red
            $script:totalFailed++
        }
    }
}

Write-Host ""
Write-Host "Importacao concluida: $totalCreated criado(s), $totalUpdated atualizado(s), $totalFailed falha(s)."

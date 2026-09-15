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
}
# 3 backticks como variavel evita ter que escapar backtick dentro de string
# interpolada do PowerShell (onde backtick e o caractere de escape).
$fence3 = [string]::new([char]96, 3)

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
                $projResp = Invoke-JsonApi -Uri "$ApiBase/projects" -Method Post -Payload @{ name = $concept.projectName; description = "Projeto real do usuario, fonte dos exemplos de codigo importados." }
                $projectByName[$projKey] = $projResp
                $projectId = $projResp.id
            }

            # Concept (verbete)
            $conceptKey = $concept.name.ToLower()
            $fence = $fenceByTag[$data.tag]
            if ($conceptByName.ContainsKey($conceptKey)) {
                $conceptId = $conceptByName[$conceptKey].id
                Invoke-JsonApi -Uri "$ApiBase/concepts/$conceptId" -Method Put -Payload @{ name = $concept.name; description = $concept.shortDescription } | Out-Null
                $script:totalUpdated++
            } else {
                $conceptResp = Invoke-JsonApi -Uri "$ApiBase/concepts" -Method Post -Payload @{ name = $concept.name; description = $concept.shortDescription }
                $conceptByName[$conceptKey] = $conceptResp
                $conceptId = $conceptResp.id
                $script:totalCreated++
            }

            # Note com a explicacao completa + codigo real
            $noteTitle = "$($concept.name) (nota completa)"
            $noteContent = @(
                "**Nivel:** $($concept.level)"
                ""
                $concept.explanation
                ""
                "## Codigo real do projeto"
                ""
                "$fence3$fence"
                $concept.codeExample
                $fence3
                ""
                "**Onde usei:** $($concept.whereUsed)"
            ) -join "`n"
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

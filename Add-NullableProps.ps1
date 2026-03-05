# Tilføj Nullable enable og WarningsAsErrors Nullable til alle .csproj filer i den aktuelle mappe
Get-ChildItem -Path . -Recurse -Filter "*.csproj" | ForEach-Object {
    $content = Get-Content $_.FullName -Raw -Encoding UTF8
    if ($content -notmatch 'WarningsAsErrors') {
        if ($content -match '<Nullable>enable</Nullable>') {
            $content = $content -replace '(<Nullable>enable</Nullable>)', "`$1`r`n  <WarningsAsErrors>Nullable</WarningsAsErrors>"
        } else {
            $content = $content -replace '(<PropertyGroup>)', "`$1`r`n  <Nullable>enable</Nullable>`r`n  <WarningsAsErrors>Nullable</WarningsAsErrors>"
        }
        Set-Content -Path $_.FullName -Value $content -NoNewline -Encoding UTF8
    }
}

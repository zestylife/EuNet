$set_version = "1.1.18"

$files = "\src\EuNet\EuNet.csproj",
"\src\EuNet.Client\EuNet.Client.csproj",
"\src\EuNet.CodeGenerator\EuNet.CodeGenerator.csproj",
"\src\EuNet.CodeGenerator.Templates\EuNet.CodeGenerator.Templates.csproj",
"\src\EuNet.Core\EuNet.Core.csproj",
"\src\EuNet.Rpc\EuNet.Rpc.csproj",
"\src\EuNet.Server\EuNet.Server.csproj",
"\src\EuNet.UnityShims\EuNet.UnityShims.csproj"

Foreach ($file in $files) {
    $xmlFileName = $PSScriptRoot + $file
    Write-Output $xmlFileName

    [xml]$xmlDoc = Get-Content $xmlFileName
    $node = $xmlDoc.SelectSingleNode("//Project/PropertyGroup/Version");
    $node.InnerText = $set_version

    $xmlDoc.Save($xmlFileName)
}

#Nuget
$xmlFileName = $PSScriptRoot + "\src\EuNet.CodeGenerator.Templates\EuNet.CodeGenerator.Templates.nuspec"
Write-Output $xmlFileName

[xml]$xmlDoc = Get-Content $xmlFileName
$xmlDoc.package.metadata.version = $set_version

$nodes = $xmlDoc.package.metadata.dependencies.dependency
Write-Output $nodes.Count
Foreach ($node in $nodes) {
    $node.Attributes.GetNamedItem("version").InnerText = $set_version
}
$xmlDoc.Save($xmlFileName)

# Unity Package
$jsonFileName = $PSScriptRoot + "\src\EuNet.Unity\Assets\Plugins\EuNet\package.json"
Write-Output $jsonFileName
$json = Get-Content -Raw -Path $jsonFileName | Out-String | ConvertFrom-Json
$json.version = $set_version
$json | ConvertTo-Json -Depth 32 | set-content $jsonFileName 
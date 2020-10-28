dotnet pack -c Release
copy /y .\src\EuNet\bin\Release\*.nupkg .\nuget
copy /y .\src\EuNet.Core\bin\Release\*.nupkg .\nuget
copy /y .\src\EuNet.Rpc\bin\Release\*.nupkg .\nuget
copy /y .\src\EuNet.Client\bin\Release\*.nupkg .\nuget
copy /y .\src\EuNet.Server\bin\Release\*.nupkg .\nuget
copy /y .\src\EuNet.CodeGenerator.Templates\bin\Release\*.nupkg .\nuget
@PAUSE
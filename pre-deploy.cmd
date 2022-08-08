dotnet restore

dotnet build --configuration Debug
dotnet build --configuration Release

dotnet test -c Debug .\test\TauCode.Mq.Testing.Tests\TauCode.Mq.Testing.Tests.csproj
dotnet test -c Release .\test\TauCode.Mq.Testing.Tests\TauCode.Mq.Testing.Tests.csproj

nuget pack nuget\TauCode.Mq.Testing.nuspec

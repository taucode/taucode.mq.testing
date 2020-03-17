dotnet restore

dotnet build --configuration Debug
dotnet build --configuration Release

dotnet test -c Debug .\tests\TauCode.Mq.Testing.Tests\TauCode.Mq.Testing.Tests.csproj
dotnet test -c Release .\tests\TauCode.Mq.Testing.Tests\TauCode.Mq.Testing.Tests.csproj

nuget pack nuget\TauCode.Mq.Testing.nuspec

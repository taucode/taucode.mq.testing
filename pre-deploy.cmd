dotnet restore

dotnet build TauCode.Mq.Testing.sln -c Debug
dotnet build TauCode.Mq.Testing.sln -c Release

dotnet test TauCode.Mq.Testing.sln -c Debug
dotnet test TauCode.Mq.Testing.sln -c Release

nuget pack nuget\TauCode.Mq.Testing.nuspec
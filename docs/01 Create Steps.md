#Commands to create project

DotNet Core must be preinstalled in your system.

$>mkdir src
$>mkdir tests
$>cd src

$>dotnet new console -n SamlIntegration.MetadataFileGenerator
$>dotnet new classlib -n SamlIntegration.Utilities
$>dotnet add SamlIntegration.MetadataFileGenerator/SamlIntegration.MetadataFileGenerator.csproj reference SamlIntegration.Utilities/SamlIntegration.Utilities.csproj

$>dotnet new sln
$>dotnet sln add SamlIntegration.MetadataFileGenerator
$>dotnet sln add SamlIntegration.Utilities

$>dotnet add package System.Security.Cryptography.Xml

$>dotnet restore
$>dotnet build
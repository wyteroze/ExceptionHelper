using System.Threading;
using JetBrains.Application.BuildScript.Application.Zones;
using JetBrains.ReSharper.Feature.Services;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.TestFramework;
using JetBrains.TestFramework;
using JetBrains.TestFramework.Application.Zones;
using NUnit.Framework;

[assembly: Apartment(ApartmentState.STA)]

namespace ReSharperPlugin.ExceptionHelper.Tests
{
    [ZoneDefinition]
    public class ExceptionHelperTestEnvironmentZone : ITestsEnvZone, IRequire<PsiFeatureTestZone>, IRequire<IExceptionHelperZone> { }

    [ZoneMarker]
    public class ZoneMarker : IRequire<ICodeEditingZone>, IRequire<ILanguageCSharpZone>, IRequire<ExceptionHelperTestEnvironmentZone> { }

    [SetUpFixture]
    public class ExceptionHelperTestsAssembly : ExtensionTestEnvironmentAssembly<ExceptionHelperTestEnvironmentZone> { }
}

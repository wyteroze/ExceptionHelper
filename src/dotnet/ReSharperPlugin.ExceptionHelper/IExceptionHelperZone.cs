using JetBrains.Application.BuildScript.Application.Zones;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services;
using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.Application.UI.Options.OptionPages;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Resources.Settings;
using JetBrains.ReSharper.UnitTestFramework;

namespace ReSharperPlugin.ExceptionHelper
{
    [ZoneDefinition]
    public interface IExceptionHelperZone : IZone, 
        IRequire<ILanguageCSharpZone>, 
        IRequire<DaemonZone>, 
        IRequire<IProjectModelZone>,
        IRequire<IUnitTestingZone>,
        IRequire<ICodeEditingZone>,
        IRequire<IToolsOptionsPageImplZone>
    {
    }
}

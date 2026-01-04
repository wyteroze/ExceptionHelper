using JetBrains.Application.BuildScript.Application.Zones;
using JetBrains.Application.UI.Options.OptionPages;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services;
using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Resources.Settings;

namespace ReSharperPlugin.ExceptionHelper;

[ZoneMarker]
public class ExceptionHelperZoneMarker : IRequire<IToolsOptionsPageImplZone>, 
    IRequire<ILanguageCSharpZone>, 
    IRequire<DaemonZone>, 
    IRequire<IProjectModelZone>,
    IRequire<ICodeEditingZone>
{
}
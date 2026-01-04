using System;
using JetBrains.Application.Settings;
using JetBrains.Diagnostics;
using JetBrains.ReSharper.Daemon.CSharp.Stages;
using JetBrains.ReSharper.Feature.Services.CSharp.Daemon;
using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.Util.Logging;

namespace ReSharperPlugin.ExceptionHelper;

[DaemonStage(StagesBefore = [ typeof(GlobalFileStructureCollectorStage) ])]
public class GenericExceptionAnalyzer : CSharpDaemonStageBase
{
    protected override IDaemonStageProcess CreateProcess(IDaemonProcess process, IContextBoundSettingsStore settings,
        DaemonProcessKind processKind, ICSharpFile file)
    {
        return new GenericExceptionAnalyzerProcess(process, file);
    }
}

public class GenericExceptionAnalyzerProcess(IDaemonProcess process, ICSharpFile file)  : CSharpDaemonStageProcessBase(process, file)
{
    public override void VisitThrowStatement(IThrowStatement statement, IHighlightingConsumer consumer)
    {
        base.VisitThrowStatement(statement, consumer);
        
        // check if throwing an object creation
        if (statement.Exception is not IObjectCreationExpression creationExpression)
            return;
        
        var type = creationExpression.Type().GetScalarType();
        
        // make sure it's exactly system.exception and not some derived type
        if (type?.GetClrName().FullName != "System.Exception")
            return;
    
        // add highlight
        consumer.AddHighlighting(new GenericExceptionHighlighting(creationExpression));
    }

    public override void Execute(Action<DaemonStageResult> committer)
    {
        var consumer = new FilteringHighlightingConsumer(DaemonProcess.SourceFile, File, DaemonProcess.ContextBoundSettingsStore);
        // infrastructure handles traversal via Visit* methods
        File.ProcessDescendants(this, consumer);
        
        committer(new DaemonStageResult(consumer.CollectHighlightings()));
    }
}
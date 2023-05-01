﻿using Lodeon.Terminal.UI.Units;
using System.Security.Cryptography;

namespace Lodeon.Terminal.UI;

public abstract class Page : ITransform
{
    protected Script Program { get { if (_program is null) throw new ArgumentNullException(nameof(Out), "Element was not initialized"); return _program; } }

    public PixelPoint GetPosition()
    {
        throw new NotImplementedException();
    }

    public PixelPoint GetSize()
    {
        throw new NotImplementedException();
    }

    private Script? _program;

    protected ExceptionHandler ExceptionHandler { get { if (_exceptionHandler is null) throw new ArgumentNullException(nameof(ExceptionHandler), "Element was not initialized"); return _exceptionHandler; } }
    private ExceptionHandler? _exceptionHandler;

    protected Driver Out { get { if (_driver is null) throw new ArgumentNullException(nameof(Out), "Element was not initialized"); return _driver; } }
    private Driver? _driver;

    protected GraphicBuffer ProgramBuffer { get { if (_programBuffer is null) throw new ArgumentNullException(nameof(ProgramBuffer), "Element was not initialized"); return _programBuffer; } }
    private GraphicBuffer? _programBuffer;

    protected RootElement Root { get { if (_root is null) throw new ArgumentNullException(nameof(RootElement), "Element was not initialized"); return _root; } }
    private RootElement? _root;
    public event TransformChangedEvent PositionChanged;
    public event TransformChangedEvent SizeChanged;

    public bool IsMain { get; private set; }

    /// <summary>
    /// Can't be put in constructor because a generic type deriving from <see cref="Page"/> can't specify a constructor
    /// with parameters
    /// </summary>
    /// <param name="isMain"></param>
    internal void Initialize(Script program, Driver driver, bool isMain, GraphicBuffer programBuffer, ExceptionHandler handler)
    {
        _driver = driver;
        IsMain = isMain;
        _programBuffer = programBuffer;
        _program = program;
        _exceptionHandler = handler;

        _program.OnExiting += ProgramExitCallback;
    }

    internal void Display(Element element)
    {
        OverlayParent(element);
        ProgramBuffer.Overlay(element.GetGraphics(), element.GetScreenArea());
        OverlayChildren(element);
        Out.Display(ProgramBuffer);
    }

    private void OverlayChildren(Element element)
    {
        ReadOnlySpan<Element> children = element.GetChildren();

        for(int i = 0; i < children.Length; i++)
        {
            ProgramBuffer.Overlay(children[i].GetGraphics(), children[i].GetScreenArea());
            OverlayChildren(children[i]);
        }
    }

    private void OverlayParent(Element element)
    {
        if (element.Parent != null)
            OverlayParent(element);

        ProgramBuffer.Overlay(element.GetGraphics(), element.GetScreenArea());
    }

    private void ProgramExitCallback()
    {
        OnDeselect();
        Program.OnExiting -= ProgramExitCallback;
    }

    internal async Task Execute(CancellationToken token)
    {
        Task mainTask = Task.Run(OnSelect, token);
        Task waitTask = token.WaitAsync();

        try {
            await Task.WhenAll(mainTask, waitTask);
        }
        catch(OperationCanceledException) { }
    }

    protected abstract void OnSelect();
    protected abstract void Load();
    protected abstract void OnDeselect();

    internal abstract void Popup(string title, string text);

    // These two: to implement
    //protected abstract void Main();
    //protected abstract void OnExit();

    //protected virtual void OnInitialize() { }
}
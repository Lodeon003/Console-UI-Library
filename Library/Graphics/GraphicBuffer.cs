﻿using System.Drawing;
using System.Text.Encodings.Web;

namespace Lodeon.Terminal;

// Check if passed data is too big to fit in max output buffer

/// <summary>
/// [!] Testing. <see cref="Overlay(GraphicBuffer)"/> doesn't work correctly
/// [!] Make thread safe
/// </summary>
public class GraphicBuffer : IRenderable
{
    // STATIC Properties
    private static Pixel[] EmptyBuffer { get; } = new Pixel[] { Pixel.Invisible };
    
    // PUBLIC Properties
    public int Width { get; private set; }
    public int Height { get; private set; }
    public Point Position { get; private set; }
    public int Length => Width * Height;
    // PUBLIC Indexers
    public Pixel this[int index]
        => _buffer[index];

    public Pixel this[int x, int y]
        => _buffer[x + Width * y];

    // PRIVATE Members
    private Pixel[] _buffer;

    public GraphicBuffer(int baseWidth, int baseHeight)
    {
        Width = baseWidth;
        Height = baseHeight;

        _buffer = new Pixel[baseWidth * baseHeight];
        Fill(Pixel.Invisible);
    }

    public GraphicBuffer(int baseWidth, params Pixel[] buffer)
    {
        if (buffer == null)
            throw new ArgumentNullException();

        if (buffer.Length % baseWidth != 0)
            throw new ArgumentException($"The buffer passed as an argument was not a rectangle (last row was shorter than other rows). Make sure you input the correct ${nameof(baseWidth)}", nameof(buffer));

        int baseHeight = (int)(buffer.Length / baseWidth);

        Width = baseWidth;
        Height = baseHeight;

        _buffer = buffer;
    }
    public GraphicBuffer(int baseWidth, int baseHeight, params Pixel[] buffer)
    {
        if (buffer == null)
            throw new ArgumentNullException();

        if (buffer.Length != baseWidth * baseHeight)
            throw new ArgumentException($"The buffer passed as an argument was the size of '{nameof(baseWidth)}' * '{nameof(baseHeight)}'", nameof(buffer));

        Width = baseWidth;
        Height = baseHeight;

        _buffer = buffer;
    }
    public GraphicBuffer(Rectangle screenArea, Pixel[] buffer)
    {
        if (buffer == null)
            throw new ArgumentNullException();

        if (buffer.Length % screenArea.Width != 0)
            throw new ArgumentException($"The buffer passed as an argument was not a rectangle (last row was shorter than other rows). Make sure you input the correct ${screenArea.Width}", nameof(buffer));

        Width = (int)screenArea.Width;
        Height = (int)screenArea.Height;

        _buffer = buffer;
    }
    public GraphicBuffer(Rectangle screenArea)
    {
        Width = (int)screenArea.Width;
        Height = (int)screenArea.Height;

        Position = new Point(screenArea.Left, screenArea.Top);

        _buffer = new Pixel[Width * Height];
        Fill(Pixel.Invisible);
    }
    public GraphicBuffer()
    {
        _buffer = EmptyBuffer;
    }

    public void Write(int x, int y, Pixel pixel)
        => _buffer[x + Width * y] = pixel;
    public void Write(int index, Pixel pixel)
        => _buffer[index] = pixel;

    /// <summary>
    /// [!] to check
    /// </summary>
    /// <param name="width"></param>
    /// <param name="height"></param>
    public virtual void Resize(int width, int height)
    {
        Pixel[] newBuffer = new Pixel[width * height];
        _buffer = newBuffer;

        Fill(Pixel.Invisible);

        Width = width;
        Height = height;
    }

    /// <summary>
    /// [!]Should implement data copying<br/>
    /// [!] to check
    /// </summary>
    public virtual void EnsureSize(int width, int height, bool copy)
    {
        if (Width >= width && Height >= height)
            return;

        Pixel[] newBuffer = new Pixel[width * height];

        if (copy)
        {
            throw new NotImplementedException("Copy not implemented");            // Implememt bidimensional array data copying from old array to new one
        }

        _buffer = newBuffer;

        Fill(Pixel.Invisible);

        Width = width;
        Height = height;
    }

    public virtual void SetPosition(Point position)
        => Position = position;
    public void SetPosition(int x, int y)
        => SetPosition(new Point(x, y));

    public virtual ReadOnlySpan<Pixel> GetGraphics()
        => _buffer;

    public Rectangle GetScreenArea()
        => new Rectangle(Position.X, Position.Y, Width, Height);

    public Rectangle GetSourceArea()
        => new Rectangle(0, 0, Width, Height);

    public Rectangle LocalToScreen(Rectangle screenArea)
        => new Rectangle(screenArea.X + Position.X, screenArea.Y + Position.Y, screenArea.Width, screenArea.Height);
    public Point LocalToScreen(Point sourcePoint)
     => new Point(sourcePoint.X + Position.X, sourcePoint.Y + Position.Y);

    public Rectangle ScreenToLocal(Rectangle screenArea)
        => new Rectangle(screenArea.X - Position.X, screenArea.Y - Position.Y, screenArea.Width, screenArea.Height);
    public Point ScreenToLocal(Point screenPoint)
     => new Point(screenPoint.X - Position.X, screenPoint.Y - Position.Y);

    /// <summary>
    /// [!] Reviewed but To Test [!]<br/>Modifies this <see cref="GraphicBuffer"/> by overlaying <paramref name="overlay"/> pixels onto this instance's pixels
    /// </summary>
    /// <param name="overlay"></param>
    public void Overlay(GraphicBuffer otherElement)
    {
        Rectangle thisArea = this.GetScreenArea();
        Rectangle otherArea = otherElement.GetScreenArea();

        // Rectangle.IntersectsWith returns false if rectangles are empty so if a rectangle is empty just ignore it
        if (otherArea.Width == 0 || otherArea.Height == 0)
            return;

        // Debug method
        if (!thisArea.IntersectsWith(otherArea))
            throw new Exception($"The caller buffer and the {nameof(otherElement)} buffer are not overlayed");

        // Overlapping screen area
        Rectangle overlappingArea = thisArea;
        overlappingArea.Intersect(otherArea);

        // Get other's buffer direclty. GetGraphics() can be overridden and not return it's buffer. This can cause an infinite loop
        ReadOnlySpan<Pixel> thisGraphics = _buffer;
        ReadOnlySpan<Pixel> otherGraphics = otherElement._buffer;

        // Iterate through all overlapping screen positions
        for (int y = overlappingArea.Top; y < overlappingArea.Bottom; y++)
        {
            for (int x = overlappingArea.Left; x < overlappingArea.Right; x++)
            {
                // x = [screen x coordinate] - [buffer's screen x position]
                // y = [screen y coordinate] - [buffer's screen y position] * [buffer's width]
                // Buffers are one-dimension so every Y is stored with an offset of [buffer's width] one from the other 

                int thisIndex = (x - thisArea.Left) + (y - thisArea.Top) * thisArea.Width;
                int otherIndex = (x - otherArea.Left) + (y - otherArea.Top) * otherArea.Width;

                Pixel result = Pixel.Overlay(thisGraphics[thisIndex], otherGraphics[otherIndex]);
                Write(thisIndex, result);
            }
        }
    }

    /// <summary>
    /// [!] Reviewed but To Test [!]<br/>Modifies this <see cref="GraphicBuffer"/> by overlaying <paramref name="overlay"/> pixels onto this instance's pixels
    /// </summary>
    /// <param name="overlay"></param>
    public void Overlay(ReadOnlySpan<Pixel> buffer, Rectangle screenArea)
    {
        Rectangle thisArea = this.GetScreenArea();
        Rectangle otherArea = screenArea;

        // Rectangle.IntersectsWith returns false if rectangles are empty so if a rectangle is empty just ignore it
        if (otherArea.Width == 0 || otherArea.Height == 0)
            return;

        // Debug method
        if (!thisArea.IntersectsWith(otherArea))
            throw new Exception($"The caller buffer and the specified buffer are not overlayed");

        // Overlapping screen area
        Rectangle overlappingArea = thisArea;
        overlappingArea.Intersect(otherArea);

        // Get other's buffer direclty. GetGraphics() can be overridden and not return it's buffer. This can cause an infinite loop
        ReadOnlySpan<Pixel> thisGraphics = _buffer;
        ReadOnlySpan<Pixel> otherGraphics = buffer;

        // Iterate through all overlapping screen positions
        for (int y = overlappingArea.Top; y < overlappingArea.Bottom; y++)
        {
            for (int x = overlappingArea.Left; x < overlappingArea.Right; x++)
            {
                // x = [screen x coordinate] - [buffer's screen x position]
                // y = [screen y coordinate] - [buffer's screen y position] * [buffer's width]
                // Buffers are one-dimension so every Y is stored with an offset of [buffer's width] one from the other 

                int thisIndex = (x - thisArea.Left) + (y - thisArea.Top) * thisArea.Width;
                int otherIndex = (x - otherArea.Left) + (y - otherArea.Top) * otherArea.Width;

                Pixel result = Pixel.Overlay(thisGraphics[thisIndex], otherGraphics[otherIndex]);
                Write(thisIndex, result);
            }
        }
    }
    public void Overlay(Pixel pixel)
    {
        for (int i = 0; i < _buffer.Length; i++)
            _buffer[i] = _buffer[i].Overlay(pixel);
    }

    #region Overlay Backup
    public void OverlayB(GraphicBuffer otherElement)
    {
        Rectangle thisArea = this.ScreenToLocal(this.GetScreenArea());
        Rectangle otherArea = otherElement.ScreenToLocal(otherElement.GetScreenArea());

        // Debug method
        if (!thisArea.IntersectsWith(otherArea))
            throw new Exception($"The caller buffer and the {nameof(otherElement)} buffer are not overlayed");

        thisArea.Intersect(otherArea);

        // Get other's buffer direclty. GetGraphics() can be overridden and not return it's buffer. This can cause an infinite loop
        ReadOnlySpan<Pixel> thisGraphics = _buffer;
        ReadOnlySpan<Pixel> otherGraphics = otherElement._buffer;


        for (int y = thisArea.Top; y < thisArea.Height; y++)
        {
            for (int x = thisArea.Left; x < thisArea.Width; x++)
            {
                int index = x + x * y;
                Pixel result = Pixel.Overlay(thisGraphics[index], otherGraphics[index]);
                Write(index, result);
            }
        }
    }
    #endregion

    public void Fill(Pixel pixel)
    {
        for (int i = 0; i < _buffer.Length; i++)
            _buffer[i] = pixel;
    }

    public void Fill(Color background)
    {
        Pixel pixel = new Pixel().WithBackground(background);

        for (int i = 0; i < _buffer.Length; i++)
            _buffer[i] = pixel;
    }
}

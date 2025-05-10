using System;
using System.CommandLine;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace PwSThumbnailGenerator;

[SuppressMessage("Interoperability", "CA1416:Validate platform compatibility")]
internal static class Program
{
    static int Main(string[] args)
    {
        if (!OperatingSystem.IsWindows())
        {
            throw new Exception("This program only works on Microsoft Windows later than Vista.");
        }

        var episodeOption = new Option<int>("--episode", "Episode number") { IsRequired = true };
        var titleOption = new Option<string>("--title", "Lesson title") { IsRequired = true };

        var rootCommand = new RootCommand { episodeOption, titleOption };

        rootCommand.Description = "Generate episode image with circle and text in the current directory.";

        rootCommand.SetHandler(GenerateImage, episodeOption, titleOption);

        return rootCommand.Invoke(args);
    }

    static void GenerateImage(int episodeNumber, string title)
    {
        const int width = 512;
        const int height = 512;
        const int padding = 50;
        const int circleDiameter = 510;
        const int circleX = (width - circleDiameter) / 2;
        const int circleY = (height - circleDiameter) / 2;

        using Bitmap bmp = new Bitmap(width, height);

        using Graphics g = Graphics.FromImage(bmp);
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.Clear(Color.Black);

        Rectangle circleRect = new Rectangle(circleX, circleY, circleDiameter, circleDiameter);
        Rectangle paddedRect = new Rectangle(
            circleRect.X + padding,
            circleRect.Y + padding,
            circleRect.Width - 2 * padding,
            circleRect.Height - 2 * padding
        );

        using Brush circleBrush = new SolidBrush(ColorTranslator.FromHtml("#00A2E8"));
        g.FillEllipse(circleBrush, circleRect);

        string text = $"Programming with Shadow Episode {episodeNumber} {title}";

        using Font font = FindBestFitFont(g, text, new Font("Arial", 20), paddedRect.Size);
        using StringFormat format = new();
        format.Alignment = StringAlignment.Center;
        format.LineAlignment = StringAlignment.Center;
        using (Brush textBrush = new SolidBrush(Color.Black))
        {
            g.DrawString(text, font, textBrush, circleRect, format);
        }

        var output = $"ProgrammingWithShadowEp{episodeNumber}{title}.png";
        bmp.Save(output, ImageFormat.Png);
        Console.WriteLine($"✅ Image saved as '{output}'");
    }

    static Font FindBestFitFont(Graphics g, string text, Font preferredFont, Size layoutSize)
    {
        float fontSize = preferredFont.Size;

        while (true)
        {
            var testFont = new Font(preferredFont.FontFamily, fontSize, preferredFont.Style);
            SizeF textSize = g.MeasureString(text, testFont, layoutSize.Width);

            if (textSize.Height <= layoutSize.Height && textSize.Width <= layoutSize.Width)
                fontSize += 1f;
            else
                break;
        }

        return new Font(preferredFont.FontFamily, fontSize - 1, preferredFont.Style);
    }
}
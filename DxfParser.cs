using netDxf;
using netDxf.Entities;
using netDxf.Header;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

public class ColoredPoint
{
    public Point3D Position { get; set; }
    public Color Color { get; set; }
}

public class ColoredLine
{
    public Point3D Start { get; set; }
    public Point3D End { get; set; }
    public Color Color { get; set; }
}

public static class DxfParser
{
    public static List<ColoredPoint> LoadColoredPoints(string path)
    {
        string finalPath = EnsureCompatibleDxf(path);
        var result = new List<ColoredPoint>();

        try
        {
            var doc = DxfDocument.Load(finalPath);

            foreach (var point in doc.Entities.Points)
            {
                var pos = new Point3D(point.Position.X, point.Position.Y, point.Position.Z);
                var acColor = point.Color.IsByLayer ? point.Layer.Color : point.Color;
                var mediaColor = Color.FromRgb(acColor.R, acColor.G, acColor.B);

                result.Add(new ColoredPoint
                {
                    Position = pos,
                    Color = mediaColor
                });
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"B³¹d odczytu punktów DXF: {ex.Message}", "B³¹d", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        return result;
    }

    public static List<ColoredLine> LoadColoredLines(string path)
    {
        string finalPath = EnsureCompatibleDxf(path);
        var result = new List<ColoredLine>();

        try
        {
            var doc = DxfDocument.Load(finalPath);

            foreach (var line in doc.Entities.Lines)
            {
                var start = new Point3D(line.StartPoint.X, line.StartPoint.Y, line.StartPoint.Z);
                var end = new Point3D(line.EndPoint.X, line.EndPoint.Y, line.EndPoint.Z);
                var acColor = line.Color.IsByLayer ? line.Layer.Color : line.Color;
                var mediaColor = Color.FromRgb(acColor.R, acColor.G, acColor.B);

                result.Add(new ColoredLine
                {
                    Start = start,
                    End = end,
                    Color = mediaColor
                });
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"B³¹d odczytu linii DXF: {ex.Message}", "B³¹d", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        return result;
    }

    private static string EnsureCompatibleDxf(string path)
    {
        try
        {
            var versionDoc = DxfDocument.Load(path);
            if (versionDoc.DrawingVariables?.AcadVer < DxfVersion.AutoCad2000)
            {
                MessageBox.Show("Plik DXF jest w starej wersji (R12/R10). Trwa konwersja...", "Konwersja DXF", MessageBoxButton.OK, MessageBoxImage.Warning);

                string convertedPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".dxf");
                if (ConvertDxf(path, convertedPath))
                {
                    return convertedPath;
                }

                MessageBox.Show("Nie uda³o siê przekonwertowaæ pliku. U¿yj AutoCAD 2000+ i zapisz rêcznie.", "B³¹d konwersji", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show("B³¹d analizy wersji DXF: " + ex.Message, "B³¹d", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        return path;
    }

    private static bool ConvertDxf(string inputPath, string outputPath)
    {
        string odaPath = @"C:\Program Files\ODA\ODAFileConverter\ODAFileConverter.exe"; // dostosuj
        if (!File.Exists(odaPath))
        {
            MessageBox.Show("Brak ODAFileConverter. Pobierz z: https://www.opendesign.com/guestfiles/oda_file_converter",
                "Konwersja DXF", MessageBoxButton.OK, MessageBoxImage.Error);
            return false;
        }

        string inputDir = Path.GetDirectoryName(inputPath);
        string fileName = Path.GetFileName(inputPath);
        string tempOutputDir = Path.Combine(Path.GetTempPath(), "DXF_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempOutputDir);

        var psi = new ProcessStartInfo
        {
            FileName = odaPath,
            Arguments = $"\"{inputDir}\" \"{tempOutputDir}\" .dxf ACAD2010 \"\" 1 0",
            CreateNoWindow = true,
            UseShellExecute = false
        };

        try
        {
            var proc = Process.Start(psi);
            proc.WaitForExit(5000);

            var convertedFile = Path.Combine(tempOutputDir, fileName);
            if (File.Exists(convertedFile))
            {
                File.Copy(convertedFile, outputPath, true);
                return true;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show("B³¹d konwersji: " + ex.Message, "Wyj¹tek", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        return false;
    }
}

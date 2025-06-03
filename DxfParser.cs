using netDxf;
using netDxf.Entities;
using netDxf.Header;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Media.Media3D;

public static class DxfParser
{
    public static List<Point3D> LoadPoints(string path)
    {
        string finalPath = path;

        // SprawdŸ wersjê DXF i automatycznie skonwertuj jeœli R12 lub R10
        try
        {
            var versionDoc = DxfDocument.Load(path);
            if (versionDoc.DrawingVariables?.AcadVer < DxfVersion.AutoCad2000)
            {
                MessageBox.Show("Plik DXF jest w starej wersji (AutoCAD R12 lub R10). Próbujê automatycznej konwersji...",
                    "Konwersja pliku", MessageBoxButton.OK, MessageBoxImage.Warning);

                string convertedPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".dxf");
                if (ConvertDxf(path, convertedPath))
                {
                    finalPath = convertedPath;
                }
                else
                {
                    MessageBox.Show("Nie uda³o siê automatycznie przekonwertowaæ pliku. Zapisz rêcznie jako AutoCAD 2000 lub nowszy.",
                        "B³¹d konwersji", MessageBoxButton.OK, MessageBoxImage.Error);
                    return new List<Point3D>();
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"B³¹d podczas sprawdzania wersji DXF: {ex.Message}", "B³¹d", MessageBoxButton.OK, MessageBoxImage.Error);
            return new List<Point3D>();
        }

        // Wczytanie punktów z zabezpieczeniem
        var result = new List<Point3D>();
        try
        {
            var doc = DxfDocument.Load(path);

            foreach (var point in doc.Entities.Points)
            {
                if (point?.Position != null)
                {
                    result.Add(new Point3D(point.Position.X, point.Position.Y, point.Position.Z));
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"B³¹d podczas odczytu punktów DXF: {ex.Message}", "B³¹d", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        return result;
    }

    public static List<(Point3D, Point3D)> LoadLines(string path)
    {
        var doc = DxfDocument.Load(path);
        var result = new List<(Point3D, Point3D)>();

        foreach (var line in doc.Entities.Lines)
        {
            var start = new Point3D(line.StartPoint.X, line.StartPoint.Y, line.StartPoint.Z);
            var end = new Point3D(line.EndPoint.X, line.EndPoint.Y, line.EndPoint.Z);
            result.Add((start, end));
        }

        return result;
    }
    private static bool ConvertDxf(string inputPath, string outputPath)
    {
        string odaPath = @"C:\Program Files\ODA\ODAFileConverter\ODAFileConverter.exe"; // Œcie¿kê dostosuj do siebie
        if (!File.Exists(odaPath))
        {
            MessageBox.Show("Brak ODAFileConverter. Pobierz i zainstaluj z: https://www.opendesign.com/guestfiles/oda_file_converter",
                "Konwersja DXF", MessageBoxButton.OK, MessageBoxImage.Error);
            return false;
        }

        string inputDir = Path.GetDirectoryName(inputPath);
        string outputDir = Path.GetDirectoryName(outputPath);
        string fileName = Path.GetFileName(inputPath);

        // ODA nie pozwala na konwersjê pojedynczego pliku — trzeba foldery
        // Wyczyszczony folder tymczasowy
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
            else
            {
                MessageBox.Show("Nie znaleziono skonwertowanego pliku. Konwersja prawdopodobnie siê nie powiod³a.",
                    "B³¹d konwersji", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show("B³¹d konwersji: " + ex.Message, "Wyj¹tek", MessageBoxButton.OK, MessageBoxImage.Error);
            return false;
        }
    }
}

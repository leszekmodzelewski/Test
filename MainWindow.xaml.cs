using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using HelixToolkit.Wpf;
using Microsoft.Win32;

namespace Simple3DCAD
{
    public partial class MainWindow : Window
    {
        private LinesVisual3D linesVisual = new LinesVisual3D();

        public MainWindow()
        {
            InitializeComponent();
            viewPort3D.Children.Clear();
            viewPort3D.Background = Brushes.Black;
            viewPort3D.Children.Add(new DefaultLights());
            viewPort3D.Children.Add(linesVisual);
        }

        private void OpenDXF_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog { Filter = "DXF Files (*.dxf)|*.dxf" };
            if (dlg.ShowDialog() == true)
            {
                var points = DxfParser.LoadColoredPoints(dlg.FileName);
                var lines = DxfParser.LoadColoredLines(dlg.FileName);

                linesVisual.Points.Clear();

                // Usuń poprzednie kulki
                var oldSpheres = viewPort3D.Children.OfType<LabeledSphere>().ToList();
                foreach (var sphere in oldSpheres)
                    viewPort3D.Children.Remove(sphere);

                // Dodaj kulki
                foreach (var p in points)
                {
                    var sphere = new LabeledSphere
                    {
                        Center = p.Position,
                        Radius = pointSizeSlider.Value,
                        Material = MaterialHelper.CreateMaterial(p.Color),
                        DataPoint = p.Position
                    };
                    viewPort3D.Children.Add(sphere);
                }

                // Dodaj linie
                foreach (var l in lines)
                {
                    var line = new LinesVisual3D
                    {
                        Thickness = 1,
                        Color = l.Color
                    };
                    line.Points.Add(l.Start);
                    line.Points.Add(l.End);
                    viewPort3D.Children.Add(line);
                }

                viewPort3D.ZoomExtents();
            }
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            var loginWindow = new LoginWindow();
            loginWindow.ShowDialog();
            if (loginWindow.IsAuthenticated)
            {
                MessageBox.Show("Zalogowano pomyślnie!", "Login", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void viewPort3D_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var mousePos = e.GetPosition(viewPort3D);
            var hit = viewPort3D.Viewport.FindHits(mousePos).FirstOrDefault();

            if (hit != null && hit.Visual is LabeledSphere sphere)
            {
                var point = sphere.DataPoint;
                statusText.Text = $"X: {point.X:0.###}   Y: {point.Y:0.###}   Z: {point.Z:0.###}";
            }
        }

        private void pointSizeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            foreach (var sphere in viewPort3D.Children.OfType<LabeledSphere>())
            {
                sphere.Radius = pointSizeSlider.Value;
            }
        }
    }

    public class LabeledSphere : SphereVisual3D
    {
        public Point3D DataPoint { get; set; }
    }

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
            var doc = netDxf.DxfDocument.Load(path);
            var result = new List<ColoredPoint>();

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

            return result;
        }

        public static List<ColoredLine> LoadColoredLines(string path)
        {
            var doc = netDxf.DxfDocument.Load(path);
            var result = new List<ColoredLine>();

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

            return result;
        }
    }
}

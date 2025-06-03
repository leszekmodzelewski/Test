using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using HelixToolkit.Wpf; // NuGet: HelixToolkit.Wpf
using Microsoft.Win32;
using netDxf;
using System.Linq;

namespace Simple3DCAD
{
    public partial class MainWindow : Window
    {
        private LinesVisual3D linesVisual = new LinesVisual3D();

        public MainWindow()
        {
            InitializeComponent();
            viewPort3D.Children.Clear();
            viewPort3D.Background = Brushes.Black; // ciemne tło jak w AutoCAD
            viewPort3D.Children.Add(new DefaultLights());
            viewPort3D.Children.Add(linesVisual);
        }

        private void OpenDXF_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog { Filter = "DXF Files (*.dxf)|*.dxf" };
            if (dlg.ShowDialog() == true)
            {
                var points = DxfParser.LoadPoints(dlg.FileName);
                var lines = DxfParser.LoadLines(dlg.FileName);

                linesVisual.Points.Clear();

                // Usuń istniejące kulki (punkty)
                var oldSpheres = viewPort3D.Children.OfType<SphereVisual3D>().ToList();
                foreach (var sphere in oldSpheres)
                    viewPort3D.Children.Remove(sphere);

                // Dodaj kulki jako punkty
                foreach (var p in points)
                {
                    var sphere = new SphereVisual3D
                    {
                        Center = p,
                        Radius = pointSizeSlider.Value,
                        Material = MaterialHelper.CreateMaterial(Colors.Yellow),
                        Tag = p // zapamiętaj współrzędne jako tag
                    };
                    viewPort3D.Children.Add(sphere);
                }

                // Dodaj linie
                foreach (var l in lines)
                {
                    linesVisual.Points.Add(l.Item1); // start
                    linesVisual.Points.Add(l.Item2); // end
                }

                viewPort3D.ZoomExtents();
            }
        }
        private void viewPort3D_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var mousePos = e.GetPosition(viewPort3D);
            var hit = viewPort3D.Viewport.FindHits(mousePos).FirstOrDefault();

            if (hit != null && hit.Visual is SphereVisual3D sphere && sphere.Tag is Point3D point)
            {
                MessageBox.Show($"X: {point.X:0.###}\nY: {point.Y:0.###}\nZ: {point.Z:0.###}",
                    "Pozycja punktu", MessageBoxButton.OK, MessageBoxImage.Information);
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
    }
}

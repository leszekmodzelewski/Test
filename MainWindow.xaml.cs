using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Media3D;
using HelixToolkit.Wpf; // NuGet: HelixToolkit.Wpf
using Microsoft.Win32;
using netDxf;
using System.Windows.Media;

namespace Simple3DCAD
{
    public partial class MainWindow : Window
    {
        
        private LinesVisual3D linesVisual = new LinesVisual3D();
        private PointsVisual3D pointsVisual = new PointsVisual3D();

        public MainWindow()
        {
            InitializeComponent();
            viewPort3D.Children.Clear();
            viewPort3D.Children.Add(new DefaultLights());
            viewPort3D.Children.Add(pointsVisual);
            viewPort3D.Children.Add(linesVisual);
        }

        private void OpenDXF_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new Microsoft.Win32.OpenFileDialog { Filter = "DXF Files (*.dxf)|*.dxf" };
            if (dlg.ShowDialog() == true)
            {
                var points = DxfParser.LoadPoints(dlg.FileName);
                var lines = DxfParser.LoadLines(dlg.FileName);

                pointsVisual.Points.Clear();
                linesVisual.Points.Clear();

                foreach (var p in points)
                {
                    pointsVisual.Points.Add(p);
                }

                foreach (var l in lines)
                {
                    linesVisual.Points.Add(l.Item1); // start
                    linesVisual.Points.Add(l.Item2); // end
                }

                // Auto-zoom
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
    }
}




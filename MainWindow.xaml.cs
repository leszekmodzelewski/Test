using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Media3D;
using HelixToolkit.Wpf; // NuGet: HelixToolkit.Wpf
using Microsoft.Win32;
using netDxf;

namespace Simple3DCAD
{
    public partial class MainWindow : Window
    {
        private PointsVisual3D pointsVisual = new PointsVisual3D();

        public MainWindow()
        {
            InitializeComponent();
            viewPort3D.Children.Add(new DefaultLights());
            viewPort3D.Children.Add(pointsVisual);
        }

        private void OpenDXF_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog { Filter = "DXF Files (*.dxf)|*.dxf" };
            if (dlg.ShowDialog() == true)
            {
                var points = DxfParser.LoadPoints(dlg.FileName);
                pointsVisual.Points.Clear();
                foreach (var pt in points)
                    pointsVisual.Points.Add(pt);
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




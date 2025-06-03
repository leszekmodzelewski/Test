using System.Windows;

namespace Simple3DCAD
{
    public partial class LoginWindow : Window
    {
        public bool IsAuthenticated { get; private set; } = false;

        public LoginWindow()
        {
            InitializeComponent();
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            if (txtUser.Text == "admin" && txtPass.Password == "admin")
            {
                IsAuthenticated = true;
                Close();
            }
            else
            {
                MessageBox.Show("Nieprawidłowe dane logowania.");
            }
        }
    }
}

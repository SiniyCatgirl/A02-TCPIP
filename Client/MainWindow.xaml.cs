using System.Windows;
using TCP_Client;

namespace Client {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();
        }
        
        private void File_Exit_Click(object sender, RoutedEventArgs e) {
            this.Close();

            return;
        }

        private void Edit_Config_Click(object sender, RoutedEventArgs e) {
            ConfigForm cfg = new ConfigForm();
            cfg.ShowDialog();

            return;
        }

        private void Help_About_Click(object sender, RoutedEventArgs e) {
            AboutWindow about = new AboutWindow();
            about.ShowDialog();

            return;
        }
    }
}

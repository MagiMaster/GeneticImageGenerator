using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace GeneticTest {
    /// <summary>
    /// Interaction logic for NewDialog.xaml
    /// </summary>
    public partial class NewDialog : Window {
        public bool? result {
            get;
            private set;
        }

        public NewDialog() {
            InitializeComponent();
        }

        private void makeNew_Click(object sender, RoutedEventArgs e) {
            result = true;
            Close();
        }

        private void cancelNew_Click(object sender, RoutedEventArgs e) {
            result = false;
            Close();
        }
    }
}

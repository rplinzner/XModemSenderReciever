using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO.Ports;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using XModem.library;

namespace XmodemSenderReciever
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly ObservableCollection<string> _list = new ObservableCollection<string>();

        private Port SelectedPort { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            LoadGuiData();
        }

        private void SendReciveButton_Click(object sender, RoutedEventArgs e)
        {   
           
            if (this.SendingRadioButton.IsChecked == true)
            {
                if (this.CHKSUMRadioButton.IsChecked == true)
                {
                    Sender.Send(SelectedPort, Port.ControlValues["NAK"], MainTextBox.Text);
                }

                if (CRCRadioButton.IsChecked == true)
                {
                    Sender.Send(SelectedPort, Port.ControlValues["C"], MainTextBox.Text);
                }
            }

            if (RecievingRadioButton.IsChecked == true)
            {
                var letters = "/^[0-9a-zA-Z]+$/";
                if (this.CHKSUMRadioButton.IsChecked == true)
                {
                    var te = Reciever.Recieve(SelectedPort, Port.ControlValues["NAK"]);
                    MainTextBox.Text = te.Replace("\0", string.Empty);
                }

                if (CRCRadioButton.IsChecked == true)
                {
                    var te = Reciever.Recieve(SelectedPort, Port.ControlValues["C"]);

                    MainTextBox.Text = te.Replace("\0", string.Empty);

                }
            }

        }


        private void RecievingRadioButton_OnClick(object sender, RoutedEventArgs e)
        {
            this.SendReciveButton.Content = "RECIEVE";
        }


        private void SendingRadioButton_OnClick(object sender, RoutedEventArgs e)
        {
            this.SendReciveButton.Content = "SEND";
        }

        private void LoadGuiData()
        {
            var ports = SerialPort.GetPortNames();
            foreach (var port in ports)
            {
                _list.Add(port);
            }

            this.PortsSelectComboBox.ItemsSource = _list;

        }

        private void OpenPortButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.PortsSelectComboBox.Text == "" || SppedSelectComboBox.Text == "")
            {
                MessageBox.Show("Please select all values (Serial Port and Baud Rate) before opening port!", "Information", MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }

            SelectedPort = new Port(this.PortsSelectComboBox.Text, Int32.Parse(this.SppedSelectComboBox.Text));

            ClosePortButton.IsEnabled = true;
            OpenPortButton.IsEnabled = false;
            SendReciveButton.IsEnabled = true;
            SelectedPort.Open();

        }

        private void ClosePortButton_Click(object sender, RoutedEventArgs e)
        {
            ClosePortButton.IsEnabled = false;
            OpenPortButton.IsEnabled = true;
            SendReciveButton.IsEnabled = false;
            SelectedPort.Close();
        }
        
    }
}

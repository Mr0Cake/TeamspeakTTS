using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
using TeamspeakReader;

namespace TeamspeakTTS
{
    /// <summary>
    /// Interaction logic for Config.xaml
    /// </summary>
    public partial class Config : Window
    {
        public Config()
        {
            InitializeComponent();
            Reader = new ChatListener();
        }

        private ChatListener _Reader;

        public ChatListener Reader
        {
            get { return _Reader; }
            set { _Reader = value; }
        }




        //public ObservableCollection<string> Users
        //{
        //    get; set;
        //}


        //private void btnAdd_Click(object sender, RoutedEventArgs e)
        //{
        //    txtAllow.UpdateLayout();
        //    Users.Add(txtAllow.Text);
        //}

        //private void btnRemove_Click(object sender, RoutedEventArgs e)
        //{
        //    Users.Remove(lstUsers.SelectedItem as string);
        //}
        public bool Started = false;
        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            if (!Started) {
                Started = true;
                txtInfo.Content = "Running, quit to exit";
                Reader.startThread();
            }
        }
    }
}

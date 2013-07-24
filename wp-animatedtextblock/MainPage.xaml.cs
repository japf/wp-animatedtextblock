using System;
using System.Windows;
using Microsoft.Phone.Controls;

namespace wp_animatedtextblock
{
    public partial class MainPage : PhoneApplicationPage
    {
        public MainPage()
        {
            InitializeComponent();

            this.atbText.Text = DateTime.Now.ToString("G");
        }

        private void OnButtonUpdateText(object sender, RoutedEventArgs e)
        {
            this.atbText.Text = DateTime.Now.ToString("G");
        }

        private void OnButtonRemove1Click(object sender, RoutedEventArgs e)
        {
            this.atbNumber.Count -= 1;
        }

        private void OnButtonRemove10Click(object sender, RoutedEventArgs e)
        {
            this.atbNumber.Count -= 10;
        }

        private void OnButtonAdd1Click(object sender, RoutedEventArgs e)
        {
            this.atbNumber.Count += 1;
        }

        private void OnButtonAdd10Click(object sender, RoutedEventArgs e)
        {
            this.atbNumber.Count += 10;
        }
    }
}
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using SharpPcap;
using SharpPcap.WinPcap;
using System.Diagnostics;
using PcapDotNet.Core;



namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 


    public partial class MainWindow : Window
    {
        //public IList<ICaptureDevice> devices2 = new List<ICaptureDevice>();
        public IList<WinPcapDevice> devices2 = new List<WinPcapDevice>();
        public static int cTCP = 0;
        public static int dev1_out = 0;
        public static int dev1_in = 0;
        public static int dev2_out = 0;
        public static int dev2_in = 0;

        int num_Dev = 0;

         WinPcapDevice dev1;
         WinPcapDevice dev2;
      //WpfApplication1.Switch sw = new WpfApplication1.Switch();

        public MainWindow()
        {
            InitializeComponent();
         
            CaptureDeviceList devices = CaptureDeviceList.Instance;
            

            if (devices.Count < 1)
            {
                Label label_error = new Label();
                label_error.Content = "No network interfaces found";
                InterfacesStackPanel.Children.Add(label_error);

                //System.Diagnostics.Trace.WriteLine("No devices were found on this machine");
                return;
            }
            else {
                //System.Diagnostics.Trace.WriteLine("\nThe following devices are available on this machine:");
                //System.Diagnostics.Trace.WriteLine("----------------------------------------------------\n");
                int counter = 0;
                int first = -1;
                int second = -1;

                Label label_error = new Label();
                label_error.Content = "";
                InterfacesStackPanel.Children.Add(label_error);
                //Label label_one = new Label();
                //label_one.Content = "Interface1:";
                //InterfacesStackPanel.Children.Add(label_one);
                Label label_two = new Label();
                label_two.Content = "Interfaces:";
                InterfacesStackPanel.Children.Add(label_two);

                // Print out the available network devices
                foreach (WinPcapDevice dev in devices)
                {
                    System.Diagnostics.Trace.WriteLine("{0}\n", dev.ToString());
                    CheckBox checkbox = new CheckBox()
                    {
                        //Checkbox properties
                        Content = devices[counter].Description,
                        Name = "interface_" + counter.ToString(),
                        IsChecked = false
                        
                    };
                                       
                    checkbox.Checked += (sender, args) => //Handler if checkbox is checked
                    {
                        if (num_Dev >= 2)
                        {
                            label_error.Content = "Uncheck one interface";
                            num_Dev++;
                        }
                        else
                        {

                            if (first == -1) { dev1 = dev; first = 0; num_Dev++; /*label_one.Content = /*("Interface1: {0}",dev.Description);*/ }
                            else if (second == -1) { dev2 = dev; second = 0; num_Dev++; /*label_two.Content = ("Interface2: {0}", dev.Description);*/ }
                        }                     

                    };
                    checkbox.Unchecked += (sender, args) => //Handler if checkbox is unchecked
                    {
                        num_Dev--;
                        if (num_Dev <=2)
                        {
                            label_error.Content = "";
                            
                        }
                        if (num_Dev == 1)
                        {
                            label_error.Content = "Unchecked all interfaces";

                        }

                        if (first == 0) {
                            first = -1;
                            //label_one.Content = "Interface1:";
                            dev1.StopCapture();
                            dev1_in = 0;
                            dev1_out = 0; }
                        else if (second == 0) { second = -1; label_two.Content = "Interface2:"; dev2.StopCapture(); dev2_in = 0; dev2_out = 0; }
                       
                    };

                    counter++;
                    InterfacesStackPanel.Children.Add(checkbox); //Dynamically add checkbox for every device found
                }
            }


        }

        private void ThreadStartButton_OnClick(object sender, RoutedEventArgs e)
        //Method is binded to Button that starts threads on selected interfaces
        {
             if (num_Dev == 2)
             {

                WpfApplication1.SwitchWindow sw = new WpfApplication1.SwitchWindow(dev1 , dev2);
                sw.Owner = this;
                this.Hide();
                sw.ShowDialog();
            }
             /*else if (num_Dev == 1)
             {
                dev1.OnPacketArrival += new SharpPcap.PacketArrivalEventHandler(device_OnPacketArrival1);
                //dev1.Open(DeviceMode.Normal, 1000);
                dev1.Open(OpenFlags.Promiscuous | OpenFlags.NoCaptureLocal, 10);
                dev1.StartCapture();
             }*/

             else
                 System.Diagnostics.Trace.WriteLine("Error {0", num_Dev.ToString());
        }
}


    
}

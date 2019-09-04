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
using SharpPcap;
using SharpPcap.WinPcap;
using System.Diagnostics;
using PcapDotNet.Core;
using System.Threading;
using System.Data;
using System.ComponentModel;
//using System.Windows.Forms;


namespace WpfApplication1
{
    /// <summary>
    /// Interaction logic for SwitchWindow.xaml
    /// </summary>
    /// 

    public class Item {
        public int Port { get; set; }
        public string Address { get; set; }
        public int Timer { get; set; }
    }

    public class LLDPNeighbour {
        public int Timer { get; set; }
        public string LocalHostName { get; set; }
        public string RemoteHostname { get; set; }
        public string LocalPort { get; set; }
        public string RemotePort { get; set; }
    }

    public class Rules {
        public int ID { get; set; }
        public int Port { get; set; }
        public string Direct { get; set; }
        public string SDMAC { get; set; }
        public string MAC { get; set; }
        public string SDIP { get; set; }
        public string IP { get; set; }
        public bool ICMPReq { get; set; }
        public bool ICMPRep { get; set; }
        public bool TCP { get; set; }
        public bool UDP { get; set; }
        public bool HTTPS { get; set; }
        public bool HTTPD { get; set; }
        public bool ARP { get; set; }

    }

    public partial class SwitchWindow : Window
    {
        WpfApplication1.Switch sw;
       public int ruleNum = 1;
        public int initialLLDP = 1;
        private Thread t3 = null;
        public SwitchWindow(WinPcapDevice dev1 , WinPcapDevice dev2)
        {
            InitializeComponent();
            this.sw = new WpfApplication1.Switch(dev1, dev2);
            Thread t1 = new Thread(() => updateStats());
            Thread t2 = new Thread(() => updateMacTable());
            t3 = new Thread(() => updateLLDPGrid());
            t1.Start();
            t2.Start();
            t3.Start();

            DataGridTextColumn textColumn21 = new DataGridTextColumn();
            textColumn21.Header = "LocalPort";
            textColumn21.Binding = new Binding("LocalPort");
            textColumn21.Width = 80;
            lldpGrid.Columns.Add(textColumn21);

            DataGridTextColumn textColumn22 = new DataGridTextColumn();
            textColumn22.Header = "RemotePort";
            textColumn22.Binding = new Binding("RemotePort");
            textColumn22.Width = 80;
            lldpGrid.Columns.Add(textColumn22);

            DataGridTextColumn textColumn23 = new DataGridTextColumn();
            textColumn23.Header = "LocalHostName";
            textColumn23.Binding = new Binding("LocalHostName");
            textColumn23.Width = 190;
            lldpGrid.Columns.Add(textColumn23);

            DataGridTextColumn textColumn24 = new DataGridTextColumn();
            textColumn24.Header = "RemoteHostname";
            textColumn24.Binding = new Binding("RemoteHostname");
            textColumn24.Width = 190;
            lldpGrid.Columns.Add(textColumn24);

            DataGridTextColumn textColumn25 = new DataGridTextColumn();
            textColumn25.Header = "Timer";
            textColumn25.Binding = new Binding("Timer");
            textColumn25.Width = 70;
            lldpGrid.Columns.Add(textColumn25);

            DataGridTextColumn textColumn14 = new DataGridTextColumn();
            textColumn14.Header = "ID";
            textColumn14.Binding = new Binding("ID");
            textColumn14.Width = 30;
            dataGridRules.Columns.Add(textColumn14);

            DataGridTextColumn textColumn1 = new DataGridTextColumn();
            textColumn1.Header = "Port";
            textColumn1.Binding = new Binding("Port");
            textColumn1.Width = 90;
            dataGrid.Columns.Add(textColumn1);

            DataGridTextColumn textColumn2 = new DataGridTextColumn();
            textColumn2.Header = "Address";
            textColumn2.Binding = new Binding("Address");
            textColumn2.Width = 110;
            dataGrid.Columns.Add(textColumn2);

            DataGridTextColumn textColumn = new DataGridTextColumn();
            textColumn.Header = "Timer";
            textColumn.Binding = new Binding("Timer");
            textColumn.Width = 90;
            dataGrid.Columns.Add(textColumn);

            dataGrid.Items.Add(new Item() { Port = 1, Address = "afsas", Timer = 100 });

            DataGridTextColumn textColumn3 = new DataGridTextColumn();
            textColumn3.Header = "Port";
            textColumn3.Binding = new Binding("Port");
            textColumn3.Width = 40;
            dataGridRules.Columns.Add(textColumn3);

            DataGridTextColumn textColumn4 = new DataGridTextColumn();
            textColumn4.Header = "Direct";
            textColumn4.Binding = new Binding("Direct");
            textColumn4.Width = 50;
            dataGridRules.Columns.Add(textColumn4);

            DataGridTextColumn textColumn5 = new DataGridTextColumn();
            textColumn5.Header = "SDMAC";
            textColumn5.Binding = new Binding("SDMAC");
            textColumn5.Width = 40;
            dataGridRules.Columns.Add(textColumn5);

            DataGridTextColumn textColumn6 = new DataGridTextColumn();
            textColumn6.Header = "MAC";
            textColumn6.Binding = new Binding("MAC");
            textColumn6.Width = 100;
            dataGridRules.Columns.Add(textColumn6);

            DataGridTextColumn textColumn7 = new DataGridTextColumn();
            textColumn7.Header = "SDIP";
            textColumn7.Binding = new Binding("SDIP");
            textColumn7.Width = 40;
            dataGridRules.Columns.Add(textColumn7);

            DataGridTextColumn textColumn8 = new DataGridTextColumn();
            textColumn8.Header = "IP";
            textColumn8.Binding = new Binding("IP");
            textColumn8.Width = 100;
            dataGridRules.Columns.Add(textColumn8);

            DataGridTextColumn textColumn9 = new DataGridTextColumn();
            textColumn9.Header = "ICMPReq";
            textColumn9.Binding = new Binding("ICMPReq");
            textColumn9.Width = 40;
            dataGridRules.Columns.Add(textColumn9);

            DataGridTextColumn textColumn19 = new DataGridTextColumn();
            textColumn19.Header = "ICMPRep";
            textColumn19.Binding = new Binding("ICMPRep");
            textColumn19.Width = 40;
            dataGridRules.Columns.Add(textColumn19);

            DataGridTextColumn textColumn10 = new DataGridTextColumn();
            textColumn10.Header = "UDP";
            textColumn10.Binding = new Binding("UDP");
            textColumn10.Width = 40;
            dataGridRules.Columns.Add(textColumn10);

            DataGridTextColumn textColumn11 = new DataGridTextColumn();
            textColumn11.Header = "TCP";
            textColumn11.Binding = new Binding("TCP");
            textColumn11.Width = 40;
            dataGridRules.Columns.Add(textColumn11);

            DataGridTextColumn textColumn12 = new DataGridTextColumn();
            textColumn12.Header = "HTTPS";
            textColumn12.Binding = new Binding("HTTPS");
            textColumn12.Width = 40;
            dataGridRules.Columns.Add(textColumn12);

            DataGridTextColumn textColumn18 = new DataGridTextColumn();
            textColumn18.Header = "HTTPD";
            textColumn18.Binding = new Binding("HTTPD");
            textColumn18.Width = 40;
            dataGridRules.Columns.Add(textColumn18);

            DataGridTextColumn textColumn13 = new DataGridTextColumn();
            textColumn13.Header = "ARP";
            textColumn13.Binding = new Binding("ARP");
            textColumn13.Width = 40;
            dataGridRules.Columns.Add(textColumn13);

            textBoxTimer.Text = "10";
            textBoxIP.Text = "0.0.0.0";
            textBoxMAC.Text = "";

            comboBoxPort.Items.Add("Any");
            comboBoxPort.Items.Add("1");
            comboBoxPort.Items.Add("2");

            comboBoxS_DIP.Items.Add("Any");
            comboBoxS_DIP.Items.Add("Src");
            comboBoxS_DIP.Items.Add("Dst");


            comboBoxS_DMac.Items.Add("Any");
            comboBoxS_DMac.Items.Add("Src");
            comboBoxS_DMac.Items.Add("Dst");


            comboBoxDirection.Items.Add("Any");
            comboBoxDirection.Items.Add("In");
            comboBoxDirection.Items.Add("Out");

        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            
            this.sw.printMAC();
        }

        private void updateStats() {
            while (true)
            {
                this.Dispatcher.Invoke(() => {
                    countStatsIn2.Content = this.sw.countIn2.ToString();
                    ipStatsIn2.Content = this.sw.Ipv4In2.ToString();
                    ipv6StatsIn2.Content = this.sw.ARPIn2.ToString();
                    tcpStatsIn2.Content = this.sw.TCPIn2.ToString();
                    udpStatsIn2.Content = this.sw.UDPIn2.ToString();
                    icmpStatsIn2.Content = this.sw.ICMPIn2.ToString();
                    httpStatsIn2.Content = this.sw.HTTPIn2.ToString();

                    countStatsIn1.Content = this.sw.countIn1.ToString();
                    ipStatsIn1.Content = this.sw.Ipv4In1.ToString();
                    ipv6StatsIn1.Content = this.sw.ARPIn1.ToString();
                    tcpStatsIn1.Content = this.sw.TCPIn1.ToString();
                    udpStatsIn1.Content = this.sw.UDPIn1.ToString();
                    icmpStatsIn1.Content = this.sw.ICMPIn1.ToString();
                    httpStatsIn1.Content = this.sw.HTTPIn1.ToString();

                    countStatsOut1.Content = this.sw.countOut1.ToString();
                    ipStatsOut1.Content = this.sw.Ipv4Out1.ToString();
                    ipv6StatsOut1.Content = this.sw.ARPOut1.ToString();
                    tcpStatsOut1.Content = this.sw.TCPOut1.ToString();
                    udpStatsOut1.Content = this.sw.UDPOut1.ToString();
                    icmpStatsOut1.Content = this.sw.ICMPOut1.ToString();
                    httpStatsOut1.Content = this.sw.HTTPOut1.ToString();

                    countStatsOut2.Content = this.sw.countOut2.ToString();
                    ipStatsOut2.Content = this.sw.Ipv4Out2.ToString();
                    ipv6StatsOut2.Content = this.sw.ARPOut2.ToString();
                    tcpStatsOut2.Content = this.sw.TCPOut2.ToString();
                    udpStatsOut2.Content = this.sw.UDPOut2.ToString();
                    icmpStatsOut2.Content = this.sw.ICMPOut2.ToString();
                    httpStatsOut2.Content = this.sw.HTTPOut2.ToString();

                });
                
               // Console.WriteLine("Ja som okno a pocet je " + this.sw.countIn2);

                System.Threading.Thread.Sleep(2000);
            }
        }

        private void updateMacTable() {
            while (true)
            {
                this.Dispatcher.Invoke(() =>
                {

                    List<WpfApplication1.MacTable> list = new List<WpfApplication1.MacTable>();
                    list = sw.sendTable();

                    dataGrid.Items.Clear();

                    foreach (var i in list) {
                        //Console.WriteLine(i.sourceMac + " Ahoj");
                        dataGrid.Items.Add(new Item() { Port = i.port, Address = i.sourceMac, Timer = i.timer });
                        
                    }
  
                });

                System.Threading.Thread.Sleep(1000);
            }
        }

        private void ExitSWButton_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
            this.Close();
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            this.sw.deleteStats();
            this.sw.deleteTable();
        }

        private void buttonSetTimer_Click(object sender, RoutedEventArgs e)
        {
            this.sw.setTimer(Int32.Parse(textBoxTimer.Text));
            this.sw.updateTable();
        }

        private void buttonAdd_Click(object sender, RoutedEventArgs e)
        {
            Rule rule = new Rule();
            rule.ID = ruleNum++;
            if (comboBoxPort.SelectedItem.ToString().Equals("Any"))
            {
                rule.Port = -1;
            }
            else
            {
                rule.Port = Int32.Parse(comboBoxPort.SelectedItem.ToString());
               // Console.WriteLine("Port je " + rule.Port);
            }

            rule.Direction = comboBoxDirection.SelectedItem.ToString();
           // Console.WriteLine(rule.Direction);
            rule.sdMac = comboBoxS_DMac.SelectedItem.ToString();
            rule.sdIP = comboBoxS_DIP.SelectedItem.ToString();
            rule.ip = textBoxIP.Text;
            rule.mac = textBoxMAC.Text;
            rule.ICMPReq = checkBoxICMPReq.IsChecked.Value;
            rule.ICMPRep = checkBoxICMPReq.IsChecked.Value;
            rule.UDP = checkBoxUDP.IsChecked.Value;
            rule.TCP = checkBoxTCP.IsChecked.Value;
            rule.HTTPS = checkBoxHTTPS.IsChecked.Value;
            rule.HTTPD = checkBoxHTTPS.IsChecked.Value;
            rule.ARP = checkBoxARP.IsChecked.Value;
            //Console.WriteLine(rule.ICMP);

            comboBoxDeleteRule.Items.Add(rule.ID);

            dataGridRules.Items.Add(new Rules { ID = rule.ID, Port = rule.Port, Direct = rule.Direction,
                HTTPS = rule.HTTPS, HTTPD = rule.HTTPD, ICMPReq = rule.ICMPReq,
                ICMPRep = rule.ICMPRep, IP = rule.ip, ARP = rule.ARP, MAC = rule.mac, SDIP = rule.sdIP
                , SDMAC = rule.sdMac, TCP = rule.TCP , UDP = rule.UDP });

            lock (this.sw.locker) {
                this.sw.tableofrules.Add(rule);
            }
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            // delete function
           // if(comboBoxDeleteRule.SelectedItem.ToString() != null)
            Console.WriteLine("Delete rule nb. " + comboBoxDeleteRule.SelectedItem.ToString());

            int idDelete = Int32.Parse(comboBoxDeleteRule.SelectedItem.ToString());

            Rule j = new Rule();

            foreach (var i in this.sw.tableofrules) {

               // Console.WriteLine("som vo foreach " + i.ID);
                if (i.ID == idDelete) {
                   // Console.WriteLine("Mam deletnut list");
                     j = i;
                }
            }
            lock (this.sw.locker) {
                this.sw.tableofrules.Remove(j);
            }
            dataGridRules.Items.Clear();
            comboBoxDeleteRule.Items.Clear();

            foreach (var i in this.sw.tableofrules) {
                //Console.WriteLine(i.ID);
                comboBoxDeleteRule.Items.Add(i.ID);
                dataGridRules.Items.Add(new Rules
                {
                    ID = i.ID,
                    Port = i.Port,
                    Direct = i.Direction,
                    HTTPS = i.HTTPS,
                    ICMPReq = i.ICMPReq,
                    HTTPD = i.HTTPD,
                    ICMPRep = i.ICMPRep,
                    IP = i.ip,
                    ARP = i.ARP,
                    MAC = i.mac,
                    SDIP = i.sdIP,
                    SDMAC = i.sdMac,
                    TCP = i.TCP,
                    UDP = i.UDP
                });
            }



        }

        private void updateLLDPGrid() {
            while (true)
            {
                this.Dispatcher.Invoke(() =>
                {

                    lldpGrid.Items.Clear();

                    lock (this.sw.locker)
                    {

                        foreach (var i in this.sw.tableofLLDP)
                        {
                            //Console.WriteLine(i.sourceMac + " Ahoj");
                            lldpGrid.Items.Add(new LLDPNeighbour()
                            {
                                LocalPort = i.LocalPort,
                                RemotePort = i.RemotePort,
                                LocalHostName = i.LocalHostName,
                                RemoteHostname = i.RemoteHostname,
                                Timer = i.Timer
                            });

                        }
                    }
                });

                System.Threading.Thread.Sleep(1000);
            }
        }

        private void button3_Click(object sender, RoutedEventArgs e)
        {
            lock (this.sw.locker) {
                sw.tableofLLDP.Clear();
            }
        }

        private void button4_Click(object sender, RoutedEventArgs e)
        {
            if (initialLLDP++ % 2 == 1)
            {
                t3.Suspend();
                this.sw.t2.Suspend();
                this.sw.t3.Suspend();
                this.sw.t4.Suspend();
                this.sw.tableofLLDP.Clear();
                lldpGrid.Items.Clear();
            }
            else {
                t3.Resume();
                this.sw.t2.Resume();
                this.sw.t3.Resume();
                this.sw.t4.Resume();
            }
        }
    }
}

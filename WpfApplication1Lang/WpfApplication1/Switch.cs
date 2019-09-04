using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpPcap;
using SharpPcap.WinPcap;
using System.Diagnostics;
using PcapDotNet.Core;
using System.Net.NetworkInformation;
using PacketDotNet;
using System.Threading;
using PacketDotNet.Utils;
using PacketDotNet.LLDP;
using System.Collections;

namespace WpfApplication1
{
    

    class Switch
    {

       // private Mutex mutex = new Mutex();

        WinPcapDevice dev1;
        WinPcapDevice dev2;

        public readonly PhysicalAddress destinationLLDPHW = new PhysicalAddress(new byte[] { 0x01 , 0x80 , 0xc2 , 0x00 , 0x00 , 0x0e});

        public readonly object locker = new object();

        LinkedList<WpfApplication1.MacTable> tabulka = new LinkedList<MacTable>();
        public List<WpfApplication1.Rule> tableofrules = new List<Rule>();
        public List<WpfApplication1.LLDPNeighbour> tableofLLDP = new List<LLDPNeighbour>();
        
        WpfApplication1.MacTable pomNode = new WpfApplication1.MacTable();

        public int countIn1 = 0, countIn2 = 0, countOut1 = 0, countOut2 = 0;
        public int TCPIn1 = 0, TCPIn2 = 0, TCPOut1 = 0 , TCPOut2 = 0;
        public int UDPIn1 = 0, UDPIn2 = 0, UDPOut1 = 0, UDPOut2 = 0;
        public int ARPIn1 = 0, ARPIn2 = 0, ARPOut1 = 0, ARPOut2 = 0;
        public int ICMPIn1 = 0, ICMPIn2 = 0, ICMPOut1 = 0, ICMPOut2 = 0;
        public int Ipv4In1 = 0, Ipv4In2 = 0, Ipv4Out1 = 0, Ipv4Out2 = 0;
        public int Ipv6In1 = 0, Ipv6In2 = 0, Ipv6Out1 = 0, Ipv6Out2 = 0;
        public int HTTPIn1 = 0, HTTPIn2 = 0, HTTPOut1 = 0, HTTPOut2 = 0;
        public int timeInTimer = 100;

        public Thread t2;
        public Thread t3;
        public Thread t4;


        public Switch(WinPcapDevice dev1, WinPcapDevice dev2)
        {
            this.dev1 = dev1;
            this.dev2 = dev2;


            dev1.OnPacketArrival += new SharpPcap.PacketArrivalEventHandler(device_OnPacketArrival1);
            ////dev1.Open(DeviceMode.Normal, 1000);
            dev1.Open(OpenFlags.Promiscuous | OpenFlags.NoCaptureLocal, 10);
             dev1.StartCapture();
            dev2.OnPacketArrival += new SharpPcap.PacketArrivalEventHandler(device_OnPacketArrival1);
            ////dev2.Open(DeviceMode.Normal, 1000);
            dev2.Open(OpenFlags.Promiscuous | OpenFlags.NoCaptureLocal, 10);
            dev2.StartCapture();

            Thread t1 = new Thread(() => timer());
            t2 = new Thread(() => sendPacketLLDP(dev1,'1'));
            t3 = new Thread(() => sendPacketLLDP(dev2,'2'));
            t4 = new Thread(() => lldpTime());

            t1.Start();
            t2.Start();
            t3.Start();
            t4.Start();
        }

        private void device_OnPacketArrival1(object sender, CaptureEventArgs packet)
        {
                int portNumber = -1;

                if (((WinPcapDevice)packet.Device).MacAddress == this.dev1.MacAddress)
                {
                  //  System.Diagnostics.Trace.WriteLine("I am port 1");
                    portNumber = 1;
                }
                else
                {
                   // System.Diagnostics.Trace.WriteLine("I am port 2");
                    portNumber = 2;
                }
                String data = packet.Packet.ToString();

                var packet2 = PacketDotNet.Packet.ParsePacket(packet.Packet.LinkLayerType, packet.Packet.Data);


                var ethernetPacket = (PacketDotNet.EthernetPacket)packet2;

                if (ethernetPacket != null)
                {

                lock (locker)
                {
                    // countIn1++;
                    // print source mac address on standard output - works
                    //  System.Diagnostics.Trace.WriteLine(ethernetPacket.Type.ToString());

                    var lldp = (PacketDotNet.LLDPPacket)ethernetPacket.Extract(typeof(PacketDotNet.LLDPPacket));

                    if (lldp != null) { 

                       // Console.WriteLine("Nasiel som LLDP");

                        readLLDP(lldp,portNumber);

                        return;
                    }

                    if (searchMac(tabulka, ethernetPacket.SourceHwAddress.ToString(), portNumber) == 1)
                    {
                       // Console.WriteLine("Zaznam exituje");
                       // Console.WriteLine(pomNode.sourceMac + " " + pomNode.port + " " + pomNode.timer);
                    }
                    else
                    {
                        WpfApplication1.MacTable newNode = new WpfApplication1.MacTable();
                       // Console.WriteLine("Zaznam neexistuje treba ho pridat " + ethernetPacket.SourceHwAddress.ToString());
                        newNode.sourceMac = ethernetPacket.SourceHwAddress.ToString();
                        newNode.port = 1;
                        newNode.timer = timeInTimer;
                        tabulka.AddLast(newNode);
                    }

                   string protokol = findProtocol(ethernetPacket);

                    if (protokol != null && protokol.Equals("ARP"))
                    {
                        var ArpPacket = (PacketDotNet.ARPPacket)ethernetPacket.Extract(typeof(PacketDotNet.ARPPacket));
                        if (ArpPacket != null && checkFilter(portNumber, "In", ethernetPacket.SourceHwAddress.ToString()
                        , ethernetPacket.DestinationHwAddress.ToString(), ArpPacket.SenderProtocolAddress.ToString(),
                        ArpPacket.TargetProtocolAddress.ToString(), protokol) == 1)
                        {
                            return;
                        }
                    }
                    else
                    {
                        var IpPacket = (PacketDotNet.IpPacket)ethernetPacket.Extract(typeof(PacketDotNet.IpPacket));

                        //   Console.WriteLine("ja som " + protokol + "\n");
                        if (IpPacket != null && checkFilter(portNumber, "In", ethernetPacket.SourceHwAddress.ToString()
                            , ethernetPacket.DestinationHwAddress.ToString(), IpPacket.SourceAddress.ToString(),
                            IpPacket.DestinationAddress.ToString(), protokol) == 1)
                        {
                            return;
                        }
                    }

                    incrementStatsIn(portNumber, protokol);

                 //   Console.WriteLine("------- Ja som destinacia " + ethernetPacket.DestinationHwAddress.ToString()
                 //       + "---------");

                    int destPort = searchMactoSend(tabulka, ethernetPacket.DestinationHwAddress.ToString());

                    if (protokol != null && protokol.Equals("ARP"))
                    {
                        var ArpPacket = (PacketDotNet.ARPPacket)ethernetPacket.Extract(typeof(PacketDotNet.ARPPacket));
                        if (ArpPacket != null && checkFilter(destPort, "Out", ethernetPacket.SourceHwAddress.ToString()
                        , ethernetPacket.DestinationHwAddress.ToString(), ArpPacket.SenderProtocolAddress.ToString(),
                        ArpPacket.TargetProtocolAddress.ToString(), protokol) == 1)
                        {
                            return;
                        }
                    }
                    else
                    {
                        var IpPacket = (PacketDotNet.IpPacket)ethernetPacket.Extract(typeof(PacketDotNet.IpPacket));

                        //   Console.WriteLine("ja som " + protokol + "\n");
                        if (IpPacket != null && checkFilter(destPort, "Out", ethernetPacket.SourceHwAddress.ToString()
                            , ethernetPacket.DestinationHwAddress.ToString(), IpPacket.SourceAddress.ToString(),
                            IpPacket.DestinationAddress.ToString(), protokol) == 1)
                        {
                            return;
                        }
                    }

                    int odoslane =  sendMsg(portNumber, destPort , packet2);

                    if (odoslane != -1) {
                        incrementStatsOut(odoslane, protokol);
                    }


                }
            }
        }

        private int searchMac(LinkedList<WpfApplication1.MacTable> tabulka, String hladanaMac , int portNumber)
        {

            foreach (var i in tabulka)
            {
                if (i.sourceMac.Equals(hladanaMac)) {
                    i.port = portNumber;
                    i.timer = timeInTimer;
                    this.pomNode = i;
                    return 1;
                }
            }

            return 0;
        }

        private int searchMactoSend(LinkedList<WpfApplication1.MacTable> tabulka, String hladanaMac) {

            foreach (var i in tabulka) {
                if (i.sourceMac.Equals(hladanaMac)) {
                    return i.port;
                }
            }

            return -1;
        }

        public void printMAC()
        {
            if (tabulka.First != null)
                foreach (var i in tabulka)
                {
                    Console.WriteLine("Ja som vypis Tabulky: " + i.sourceMac + " timer: " + i.timer + " port " + i.port);
                }
            else Console.WriteLine("prazdna tabulka\n");
        }

        private string findProtocol(PacketDotNet.EthernetPacket packet)
        {
            if (packet != null)
            {
                //Console.WriteLine("-------------------------------");
                //System.Diagnostics.Trace.WriteLine("{0}", packet.Type.ToString());
                //Console.WriteLine("-------------------------------");

                if (packet.Type.ToString().Equals("IpV4"))
                {
                    //System.Diagnostics.Trace.WriteLine("{0}", packet.Type.ToString());
                  //  Console.WriteLine("Som v IPv4");
                    

                    var IpPacket = (PacketDotNet.IpPacket)packet.Extract(typeof(PacketDotNet.IpPacket));

                    //System.Diagnostics.Trace.WriteLine("{0}", IpPacket.Protocol.ToString());

                    if (IpPacket.Protocol.ToString().Equals("TCP"))
                    {
                        // System.Diagnostics.Trace.WriteLine("{0}", packet.Type.ToString());
                        //    Console.WriteLine("Som v TCP");
                        var TcpPacket = (PacketDotNet.TcpPacket)IpPacket.Extract(typeof(PacketDotNet.TcpPacket));
                        if (TcpPacket.SourcePort == 80)
                        {
                            return "HTTPS";
                        }
                        else if (TcpPacket.DestinationPort == 80) {
                            return "HTTPD";
                        }
                        return "TCP";

                    }
                    else if (IpPacket.Protocol.ToString().Equals("UDP"))
                    {
                        //  Console.WriteLine("Som v UDP");

                        return "UDP";
                    }
                    else if (IpPacket.Protocol.ToString().Equals("ICMP"))
                    {
                        var icmpPacket = (PacketDotNet.ICMPv4Packet)IpPacket.Extract(typeof(PacketDotNet.ICMPv4Packet));

                        if (icmpPacket.TypeCode.ToString().Equals("EchoReply"))
                        {
                            return "ICMPRep";
                        }
                        else if (icmpPacket.TypeCode.ToString().Equals("EchoRequest"))
                        { 
                            return "ICMPReq";
                        }
                    }
                }
                else if (packet.Type.ToString().Equals("Arp")) {
                   //System.Diagnostics.Trace.WriteLine("{0}", packet.Type.ToString());
                   // Console.WriteLine("Som v ARP");
                    return "ARP";
                }
                else 
               // System.Diagnostics.Trace.WriteLine("{0}", packet.Type.ToString());
                return null;
            }
            return null;
        }

        private void incrementStatsIn(int port,string protocol) {
            if (protocol != null)
            {
                if (port == 1)
                {
                    countIn1++;
                    if (protocol.Equals("UDP"))
                    {
                        UDPIn1++;
                        Ipv4In1++;
                    }
                    else if (protocol.Equals("TCP"))
                    {
                        TCPIn1++;
                        Ipv4In1++;
                    }
                    else if (protocol.Equals("HTTP"))
                    {
                        HTTPIn1++;
                        TCPIn1++;
                        Ipv4In1++;
                    }
                    else if (protocol.Equals("ICMP"))
                    {
                        ICMPIn1++;
                    }
                    else if (protocol.Equals("ARP"))
                    {
                        ARPIn1++;
                    }
                    else if (protocol.Equals("IPv6"))
                    {
                        Ipv6In1++;
                    }
                    else if (protocol.Equals("IPv4"))
                    {
                        Ipv4In1++;
                    }
                }
                else
                {
                    countIn2++;
                   // Console.WriteLine("pocet celkovy je: " + countIn2);
                    if (protocol.Equals("UDP"))
                    {
                        UDPIn2++;
                        Ipv4In2++;
                    }
                    else if (protocol.Equals("TCP"))
                    {
                        TCPIn2++;
                        Ipv4In2++;
                    }
                    else if (protocol.Equals("HTTP"))
                    {
                        HTTPIn2++;
                        TCPIn2++;
                        Ipv4In2++;
                    }
                    else if (protocol.Equals("ICMP"))
                    {
                        ICMPIn2++;
                    }
                    else if (protocol.Equals("ARP"))
                    {
                        ARPIn2++;
                    }
                    else if (protocol.Equals("IPv6"))
                    {
                        Ipv6In2++;
                    }
                    else if (protocol.Equals("IPv4"))
                    {
                        Ipv4In2++;
                    }
                }
            }

        }

        private void incrementStatsOut(int port , string protocol)
        {
            if (protocol != null)
            {
                if (port == 1)
                {

                    countOut1++;
                    if (protocol.Equals("UDP"))
                    {
                        UDPOut1++;
                        Ipv4Out1++;
                    }
                    else if (protocol.Equals("TCP"))
                    {
                        TCPOut1++;
                        Ipv4Out1++;
                    }
                    else if (protocol.Equals("HTTP"))
                    {
                        HTTPOut1++;
                        TCPOut1++;
                        Ipv4Out1++;
                    }
                    else if (protocol.Equals("ICMP"))
                    {
                        ICMPOut1++;
                    }
                    else if (protocol.Equals("ARP"))
                    {
                        ARPOut1++;
                    }
                    else if (protocol.Equals("IPv6"))
                    {
                        Ipv6Out1++;
                    }
                    else if (protocol.Equals("IPv4"))
                    {
                        Ipv4Out1++;
                    }
                }
                else
                {
                    countOut2++;
                    if (protocol.Equals("UDP"))
                    {
                        UDPOut2++;
                        Ipv4Out2++;
                    }
                    else if (protocol.Equals("TCP"))
                    {
                        TCPOut2++;
                        Ipv4Out2++;
                    }
                    else if (protocol.Equals("HTTP"))
                    {
                        HTTPOut2++;
                        TCPOut2++;
                        Ipv4Out2++;
                    }
                    else if (protocol.Equals("ICMP"))
                    {
                        ICMPOut2++;
                    }
                    else if (protocol.Equals("ARP"))
                    {
                        ARPOut2++;
                    }
                    else if (protocol.Equals("IPv6"))
                    {
                        Ipv6Out2++;
                    }
                    else if (protocol.Equals("IPv4"))
                    {
                        Ipv4Out2++;
                    }
                }
            }
        }

        private void timer() {

            while (true)
            {
                lock (locker)
                {
                    if (tabulka.First != null)
                    {
                        /*foreach (var i in tabulka)
                        {
                            i.timer--;
                        }*/
                        var j = tabulka.First;
                        while (j != null) {
                            j.Value.timer--;
                            if (j.Value.timer == 0)
                                tabulka.Remove(j);
                            j = j.Next;
                        } 
                    }
                }
                System.Threading.Thread.Sleep(1000);
            }

        }

        private int sendMsg(int srcPort , int destPort , PacketDotNet.Packet packet) {

            if (destPort.Equals(srcPort))
            {
               // Console.WriteLine("Src a dest sa rovnaju zahod paket");
                return -1;
            }
            else if (destPort.Equals(-1))
            {
               // Console.WriteLine("Robime flood");
                if (srcPort.Equals(1))
                {

                   // Console.WriteLine("Posielame flood na 2 a ja som src " + srcPort);

                    try
                    {
                        //Send the packet out the network device
                        dev2.SendPacket(packet);

                      //  System.Diagnostics.Trace.WriteLine("-- Packet sent successfuly.");
                    }
                    catch (Exception e)
                    {
                        System.Diagnostics.Trace.WriteLine("-- " + e.Message);
                    }
                    return 2;

                }
                else if (srcPort.Equals(2)) {

                  //  Console.WriteLine("Posielame flood na 1 a ja som src " + srcPort);

                    try
                    {
                        //Send the packet out the network device
                        dev1.SendPacket(packet);

                       // System.Diagnostics.Trace.WriteLine("-- Packet sent successfuly.");
                    }
                    catch (Exception e)
                    {
                        System.Diagnostics.Trace.WriteLine("-- " + e.Message);
                    }

                    return 1;
                }
            }
            else if (destPort.Equals(1) && !destPort.Equals(srcPort))
            {
                //Console.WriteLine("Posielame klasika na 1");

                try
                {
                    //Send the packet out the network device
                    dev1.SendPacket(packet);

                 //   System.Diagnostics.Trace.WriteLine("-- Packet sent successfuly.");
                }
                catch (Exception e)
                {
                    System.Diagnostics.Trace.WriteLine("-- " + e.Message);
                }
                return 1;

            }
            else if (destPort.Equals(2) && !destPort.Equals(srcPort)) {
              //  Console.WriteLine("Posielame klasika na 2");

                try
                {
                    //Send the packet out the network device
                    dev2.SendPacket(packet);

                   // System.Diagnostics.Trace.WriteLine("-- Packet sent successfuly.");
                }
                catch (Exception e)
                {
                    System.Diagnostics.Trace.WriteLine("-- " + e.Message);
                }
                return 2;
            }
            return -1;

        }

        public List<WpfApplication1.MacTable> sendTable() {

            lock (locker)
            {

                List<WpfApplication1.MacTable> list = new List<WpfApplication1.MacTable>();
                foreach (var i in this.tabulka) {
                    list.Add(new MacTable() { port = i.port, sourceMac = i.sourceMac, timer = i.timer });
                   // Console.WriteLine(i.port + " vypis z tabulky");
                }
                //foreach (var i in list)
                  //  Console.WriteLine(i.sourceMac);


                return list;
            }
        }

        public void deleteTable() {
            lock (locker)
            {
                if (tabulka.First != null) {
                    tabulka.Clear();
                }
            }
        }

        public void deleteStats()
        {
            lock (locker) {
                countIn1 = 0; countIn2 = 0; countOut1 = 0; countOut2 = 0;
                TCPIn1 = 0; TCPIn2 = 0; TCPOut1 = 0; TCPOut2 = 0;
                UDPIn1 = 0; UDPIn2 = 0; UDPOut1 = 0; UDPOut2 = 0;
                ARPIn1 = 0; ARPIn2 = 0; ARPOut1 = 0; ARPOut2 = 0;
                ICMPIn1 = 0; ICMPIn2 = 0; ICMPOut1 = 0; ICMPOut2 = 0;
                Ipv4In1 = 0; Ipv4In2 = 0; Ipv4Out1 = 0; Ipv4Out2 = 0;
                Ipv6In1 = 0; Ipv6In2 = 0; Ipv6Out1 = 0; Ipv6Out2 = 0;
                HTTPIn1 = 0; HTTPIn2 = 0; HTTPOut1 = 0; HTTPOut2 = 0;
    }
        }

        public void setTimer(int newTime) {
            lock (locker)
            {
                timeInTimer = newTime;
            }

        }

        public void updateTable() {
            lock (locker)
            {
                foreach (var i in this.tabulka)
                {
                    i.timer = timeInTimer;
                }
            }
        }

        public void addRule(WpfApplication1.Rule rule) {
            lock (locker) {
                tableofrules.Add(rule);
            }
        }

        public int checkFilter(int portNumber, string direction, string sourceMAC , string destMAC ,string
            sourceIP , string destIP, string protocol) {
            lock (locker)
            {
                if (tableofrules == null) {
                    return 0;
                }
                if (protocol == null)
                {
                    return 0;
                }
                foreach (var i in tableofrules) {
                    if (portNumber == i.Port || i.Port == -1) {
                        if (i.Direction.Equals(direction) || i.Direction.Equals("Any")) {
                            if ((i.mac.Equals(sourceMAC) && (i.sdMac.Equals("Src") || i.sdMac.Equals("Any")))
                                || (i.mac.Equals("All") && (i.sdMac.Equals("Src") || i.sdMac.Equals("Any")))) {
                                if ((i.ip.Equals(sourceIP) && (i.sdIP.Equals("Src") || i.sdIP.Equals("Any")))
                                || (i.ip.Equals("All") && (i.sdIP.Equals("Src") || i.sdIP.Equals("Any")))) {
                                    if (protocol.Equals("ICMPReq") && i.ICMPReq)
                                    {

                                        Console.WriteLine("Filtrujem ICMPReques" + portNumber + " " + direction );
                                        // zahod ICMP paket
                                        return 1;
                                    }
                                    else if (protocol.Equals("ICMPRep") && i.ICMPRep)
                                    {

                                        Console.WriteLine("Filtrujem ICMPReplay" + portNumber + " " + direction);
                                        // zahod ICMP paket
                                        return 1;
                                    }
                                    else if (protocol.Equals("UDP") && i.UDP)
                                    {
                                        // zahod UDP
                                        return 1;
                                    }
                                    else if (protocol.Equals("TCP") && i.TCP)
                                    {
                                        //zahod TCP
                                        return 1;
                                    }
                                    else if (protocol.Equals("HTTPS") && i.HTTPS)
                                    {
                                        // zahod HTTPS
                                        return 1;
                                    }
                                    else if (protocol.Equals("HTTPD") && i.HTTPD)
                                    {
                                        // zahod HTTPD
                                        return 1;
                                    }
                                    else if (protocol.Equals("ARP") && i.ARP) {
                                        //zahod ARP
                                        Console.WriteLine("Zahadzujem ARPcko");
                                        return 1;
                                    }
                                }
                                else if ((i.ip.Equals(destIP) && (i.sdIP.Equals("Dst") || i.sdIP.Equals("Any")))
                                || (i.ip.Equals("All") && (i.sdIP.Equals("Dst") || i.sdIP.Equals("Any"))))
                                {
                                    if (protocol.Equals("ICMPReq") && i.ICMPReq)
                                    {

                                        Console.WriteLine("Filtrujem ICMPReques" + portNumber + " " + direction);
                                        // zahod ICMP paket
                                        return 1;
                                    }
                                    else if (protocol.Equals("ICMPRep") && i.ICMPRep)
                                    {

                                        Console.WriteLine("Filtrujem ICMPReplay" + portNumber + " " + direction);
                                        // zahod ICMP paket
                                        return 1;
                                    }
                                    else if (protocol.Equals("UDP") && i.UDP)
                                    {
                                        // zahod UDP
                                        return 1;
                                    }
                                    else if (protocol.Equals("TCP") && i.TCP)
                                    {
                                        //zahod TCP
                                        return 1;
                                    }
                                    else if (protocol.Equals("HTTPS") && i.HTTPS)
                                    {
                                        // zahod HTTPS
                                        return 1;
                                    }
                                    else if (protocol.Equals("HTTPD") && i.HTTPD)
                                    {
                                        // zahod HTTPD
                                        return 1;
                                    }
                                    else if (protocol.Equals("ARP") && i.ARP)
                                    {
                                        //zahod ARP
                                        Console.WriteLine("Zahadzujem ARPcko");
                                        return 1;
                                    }
                                }
                            }
                            else if ((i.mac.Equals(destMAC) && (i.sdMac.Equals("Dst") || i.sdMac.Equals("Any")))
                                || (i.mac.Equals("All") && (i.sdMac.Equals("Dst") || i.sdMac.Equals("Any"))))
                            {
                                if ((i.ip.Equals(sourceIP) && (i.sdIP.Equals("Src") || i.sdIP.Equals("Any")))
                                || (i.ip.Equals("All") && (i.sdIP.Equals("Src") || i.sdIP.Equals("Any"))))
                                {
                                    if (protocol.Equals("ICMPReq") && i.ICMPReq)
                                    {

                                        Console.WriteLine("Filtrujem ICMPReques" + portNumber + " " + direction);
                                        // zahod ICMP paket
                                        return 1;
                                    }
                                    else if (protocol.Equals("ICMPRep") && i.ICMPRep)
                                    {

                                        Console.WriteLine("Filtrujem ICMPReplay" + portNumber + " " + direction);
                                        // zahod ICMP paket
                                        return 1;
                                    }
                                    else if (protocol.Equals("UDP") && i.UDP)
                                    {
                                        // zahod UDP
                                        return 1;
                                    }
                                    else if (protocol.Equals("TCP") && i.TCP)
                                    {
                                        //zahod TCP
                                        return 1;
                                    }
                                    else if (protocol.Equals("HTTPS") && i.HTTPS)
                                    {
                                        // zahod HTTPS
                                        return 1;
                                    }
                                    else if (protocol.Equals("HTTPD") && i.HTTPD)
                                    {
                                        // zahod HTTPD
                                        return 1;
                                    }
                                    else if (protocol.Equals("ARP") && i.ARP)
                                    {
                                        //zahod ARP
                                        Console.WriteLine("Zahadzujem ARPcko");
                                        return 1;
                                    }
                                }
                                else if ((i.ip.Equals(destIP) && (i.sdIP.Equals("Dst") || i.sdIP.Equals("Any")))
                                || (i.ip.Equals("All") && (i.sdIP.Equals("Dst") || i.sdIP.Equals("Any"))))
                                {
                                    if (protocol.Equals("ICMPReq") && i.ICMPReq)
                                    {

                                        Console.WriteLine("Filtrujem ICMPReques" + portNumber + " " + direction);
                                        // zahod ICMP paket
                                        return 1;
                                    }
                                    else if (protocol.Equals("ICMPRep") && i.ICMPRep)
                                    {

                                        Console.WriteLine("Filtrujem ICMPReplay" + portNumber + " " + direction);
                                        // zahod ICMP paket
                                        return 1;
                                    }
                                    else if (protocol.Equals("UDP") && i.UDP)
                                    {
                                        // zahod UDP
                                        return 1;
                                    }
                                    else if (protocol.Equals("TCP") && i.TCP)
                                    {
                                        //zahod TCP
                                        return 1;
                                    }
                                    else if (protocol.Equals("HTTPS") && i.HTTPS)
                                    {
                                        // zahod HTTPS
                                        return 1;
                                    }
                                    else if (protocol.Equals("HTTPD") && i.HTTPD)
                                    {
                                        // zahod HTTPD
                                        return 1;
                                    }
                                    else if (protocol.Equals("ARP") && i.ARP)
                                    {
                                        //zahod ARP
                                        Console.WriteLine("Zahadzujem ARPcko");
                                        return 1;
                                    }
                                }
                            }
                        }
                    }
                }

            }
            return 0;
        }

        public void sendPacketLLDP(WinPcapDevice device, char devNum) {

            //System.Threading.Thread.Sleep(5000);

           // var packetToSend = PacketDotNet.LLDPPacket.RandomPacket();

            LLDPPacket packetik = new LLDPPacket();

            packetik.TlvCollection.Add(new ChassisID("HP shitty PC"));
            packetik.TlvCollection.Add(new PortID(PortSubTypes.MACAddress,device.MacAddress));
            packetik.TlvCollection.Add(new TimeToLive(10));
            packetik.TlvCollection.Add(new EndOfLLDPDU());

            String systemnm = "HP shitty PC";
            String portDesc = device.Description;
            String srcMAC = device.MacAddress.ToString();
            ushort timicek = 10;


            byte[] chasiss = BitConverter.GetBytes((long)(Math.Pow(2,9) + 7));
            byte[] portID = BitConverter.GetBytes((long)(Math.Pow(2,10) + 2));
            byte[] timetolive = BitConverter.GetBytes((long)Math.Pow(2,10) + (long)Math.Pow(2,9) + 2);
            byte[] portdescription = BitConverter.GetBytes((long)Math.Pow(2,11) + portDesc.Length);
            byte[] systemName = BitConverter.GetBytes((long)Math.Pow(2, 11) + systemnm.Length + (long)Math.Pow(2,9));


            byte[] chasContent = hovno(srcMAC);
            byte[] timeContent = BitConverter.GetBytes(timicek);

            Byte[] lldpRamec = new Byte[chasContent.Length + portDesc.Length + systemnm.Length + 17];

            int i = 0;

            for (int j = 1; j >= 0; j--) {
                lldpRamec[i++] = chasiss[j];
            }

            lldpRamec[i++] = 4;

            for (int j = 0; j < chasContent.Length; j++) {
                lldpRamec[i++] = chasContent[j];
            }

            for (int j = 1; j >= 0; j--)
            {
                lldpRamec[i++] = portID[j];
            }

            lldpRamec[i++] = 5;

            lldpRamec[i++] = Convert.ToByte(devNum);

            for (int j = 1; j >= 0; j--)
            {
                lldpRamec[i++] = timetolive[j];
            }

            lldpRamec[i++] = timeContent[1];
            lldpRamec[i++] = timeContent[0];

            lldpRamec[i++] = portdescription[1];
            lldpRamec[i++] = portdescription[0];

            for (int j = 0; j < portDesc.Length; j++) {
                lldpRamec[i++] = Convert.ToByte(portDesc[j]);
            }

            lldpRamec[i++] = systemName[1];
            lldpRamec[i++] = systemName[0];

            for (int j = 0; j < systemnm.Length; j++)
            {
                lldpRamec[i++] = Convert.ToByte(systemnm[j]);
            }

            //var lldpBytes = packetik.Bytes;
           // var lldpPacket = new PacketDotNet.LLDPPacket(new ByteArraySegment(lldpBytes));

            //Console.WriteLine("Krasne LLDP: " + lldpPacket.ToString());
            //Console.WriteLine("Krasne LLDP: " + lldpBytes[0].ToString());


            // Console.WriteLine("TUTTTTTTTTTU " + device.Addresses[2].Addr.ToString());
            var ethernetPacket = new EthernetPacket(device.Addresses[2].Addr.hardwareAddress, PhysicalAddress.Parse("01-80-C2-00-00-0E"), EthernetPacketType.LLDP);

            ethernetPacket.PayloadData = lldpRamec;

            while (true)
            {
                lock (locker)
                {
                   // Console.WriteLine("LLDP is on the way");
                    device.SendPacket(ethernetPacket);
                }
                System.Threading.Thread.Sleep(5000);
            }


        }

        public static byte[] hovno(string hex) {

            return Enumerable.Range(0, hex.Length)
                .Where(x => x % 2 == 0).Select(x => Convert.ToByte(hex.Substring(x, 2), 16)).ToArray();
        }

        public void readLLDP(LLDPPacket packet, int port) {

            var tlvFields = packet.TlvCollection;

            LLDPNeighbour lldpRow = new LLDPNeighbour();

            lldpRow.LocalHostName = "HP PC";
            lldpRow.LocalPort = port.ToString();

            byte[] lldpRec = packet.Bytes;
            int i = 0;

            while (lldpRec[i++] != 4) {
                i += lldpRec[i] + 1;

            }

            int portContentLength = lldpRec[i] - 1;
            byte[] portContent = new byte[portContentLength];

            i += 2;

            for (int j = portContentLength - 1; j >= 0; j--) {
                portContent[j] = lldpRec[i++];
            }

            lldpRow.RemotePort = Encoding.ASCII.GetString(portContent);

            while (lldpRec[i++] != 6)
            {
                i += lldpRec[i] + 1;

            }

            int timeLength = lldpRec[i++];
            byte[] timicek = new byte[timeLength];

            

            for (int j = timeLength - 1; j >= 0; j--)
            {
                timicek[j] = lldpRec[i++];
            }

            lldpRow.Timer = (int)BitConverter.ToInt16(timicek, 0);

            while (lldpRec[i] != 10 && lldpRec[i++] != 0) {
                i += lldpRec[i] + 1;
            }

            if (lldpRec[i++] != 0) {
                int sysNameLength = lldpRec[i++];
                byte[] sysNameCon = new byte[sysNameLength];

                for (int j = 0; j < sysNameLength; j++) {
                    sysNameCon[j] = lldpRec[i++];
          
                }

                lldpRow.RemoteHostname = Encoding.ASCII.GetString(sysNameCon);

            }

            //Console.WriteLine(lldpRow.RemoteHostname + " aha ze to ide");

            if (searchLLDP(lldpRow.RemoteHostname, lldpRow.Timer, lldpRow.RemotePort) == 1) {
                tableofLLDP.Add(lldpRow);
            }

        }

        public int searchLLDP(string name,int timer,string port) {

            lock (locker) {

                foreach (var i in tableofLLDP)
                {
                    if (i.RemoteHostname.Equals(name))
                    {
                        i.Timer = timer;
                        i.RemotePort = port;
                        return 0;
                    }
                }
            }

            return 1;
        }

        public void lldpTime() {
            while (true)
            {
                lock (locker)
                {
                    if (tableofLLDP.Count != 0) {
                        int index = 0;

                        while (index < tableofLLDP.Count) {
                            tableofLLDP[index].Timer--;
                            if (tableofLLDP[index].Timer == 0) {
                                tableofLLDP.Remove(tableofLLDP[index]);
                            }
           
                            index++;
                        }

                    }
                }
                System.Threading.Thread.Sleep(1000);
            }
        }
    }
}

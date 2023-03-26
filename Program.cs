using Microsoft.FlightSimulator.SimConnect;
using System.Runtime.InteropServices;
using System.Timers;

namespace SimconnectAirportDataTest
{
    internal class Program
    {
        public static SimConnect OSimConnect { get; set; } = null!;

        /// User-defined win32 event
        public const int WM_USER_SIMCONNECT = 0x048;
        /// Window handle
        private static IntPtr m_hWnd = new IntPtr(0);

        public static uint m_iCurrentDefinition = 1;
        public static uint m_iCurrentRequest = 1;

        //Very simple timer to check for new Simconnect messages
        public static System.Timers.Timer checkMessagesTimer = new(500);

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct airport
        {
            public double latitude;
            public int nRunways;
        };

        static void Main(string[] args)
        {
            Console.WriteLine("Connecting Simconnect...");

            try
            {
                OSimConnect = new SimConnect("Simconnect - DummyTest", m_hWnd, WM_USER_SIMCONNECT, null, 0);

                /// Listen to connect and quit msgs
                OSimConnect.OnRecvOpen += OSimConnect_OnRecvOpen;
                OSimConnect.OnRecvQuit += OSimConnect_OnRecvQuit;

                OSimConnect.OnRecvFacilityData += OSimConnect_OnRecvFacilityData;
                OSimConnect.OnRecvFacilityDataEnd += OSimConnect_OnRecvFacilityDataEnd;
                Console.WriteLine("Connection established");

                //Start the timer to check for new simconnect messages
                checkMessagesTimer.Elapsed += CheckMessagesTimer_Elapsed;
                checkMessagesTimer.Start();
            } catch (Exception ex)
            {
                Console.WriteLine(ex);
                Console.ReadLine();
            }

            Thread.Sleep(10000);
            Console.ReadLine();
        }
      
        private static void CheckMessagesTimer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            OSimConnect?.ReceiveMessage();
        }

        private static void OSimConnect_OnRecvFacilityDataEnd(SimConnect sender, SIMCONNECT_RECV_FACILITY_DATA_END data)
        {
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod()?.Name);
        }

        private static void OSimConnect_OnRecvFacilityData(SimConnect sender, SIMCONNECT_RECV_FACILITY_DATA data)
        {
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod()?.Name);
            try
            {
                airport a = (airport)data.Data[0];
                Console.WriteLine($"Lat: {a.latitude}; Rwys: {a.nRunways}");
            } catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            /*
              System.InvalidCastException: Unable to cast object of type 'System.UInt32' to type 'airport'.
              at SimconnectAirportDataTest.Program.OSimConnect_OnRecvFacilityData(SimConnect sender, SIMCONNECT_RECV_FACILITY_DATA data) in C:\GitHub\SimconnectAirportDataTest\SimconnectAirportDataTest\Program.cs:line 73
             * */
        }

        private static void OSimConnect_OnRecvQuit(SimConnect sender, SIMCONNECT_RECV data)
        {
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod()?.Name);
        }

        private static void OSimConnect_OnRecvOpen(SimConnect sender, SIMCONNECT_RECV_OPEN data)
        {
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod()?.Name);

            ++m_iCurrentRequest;
            REQUEST_DEFINITON rd = (REQUEST_DEFINITON)m_iCurrentRequest;

            ++m_iCurrentDefinition;
            SIMVAR_DEFINITION sd = (SIMVAR_DEFINITION)m_iCurrentDefinition;


            OSimConnect.AddToFacilityDefinition(sd, "OPEN AIRPORT");

            OSimConnect.AddToFacilityDefinition(sd, "LATITUDE");
            OSimConnect.AddToFacilityDefinition(sd, "N_RUNWAYS"); //Requesting only this (without LATITUDE) returns the correct number in the SIMCONNECT_RECV_FACILITY_DATA.Data[0] response

            OSimConnect.AddToFacilityDefinition(sd, "CLOSE AIRPORT");

            OSimConnect.RequestFacilityData(sd, rd, "EDDF", "");
        }

        
    }
}
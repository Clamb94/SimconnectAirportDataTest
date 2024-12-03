using Microsoft.FlightSimulator.SimConnect;
using System.Runtime.InteropServices;
using System.Timers;

namespace SimconnectAirportDataTest
{
    public static class ObjectExtensions
    {
        public static T StaticCast<T>(this T o) => o;
    }

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
        struct Airport
        {
            public double latitude;
            public System.Int32 arrivals;
            public System.Int32 nRunways;
        };

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct Runway
        {
            public System.Int32 primaryNumber;
            public double latitude;
            public float heading;
            public float length;
            public float width;
        };

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct Pavement
        {
            public float length;
            public float width;
            public System.Int32 enable;
        }

        public static object[] objects;

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
                Type t = data.Data[0].GetType();
                if (t == typeof(Airport))
                {
                    Airport a = (Airport)data.Data[0];
                    Console.WriteLine($"Lat: {a.latitude}");
                    Console.WriteLine($"Runways: {a.nRunways}");
                } else if (t == typeof(Runway)) {
                    Runway r = (Runway)data.Data[0];
                    Console.WriteLine($"Rwy Number: {r.primaryNumber}");
                    Console.WriteLine($"Rwy Heading: {r.heading}");
                    Console.WriteLine($"Rwy Length: {r.length}");
                } else if (t == typeof(Pavement))
                {
                    Pavement p = (Pavement)data.Data[0];
                    Console.WriteLine($"Pavement length: {p.length}");
                } else
                {
                    Console.WriteLine("SomethingElse");
                }


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
            OSimConnect.AddToFacilityDefinition(sd, "N_ARRIVALS");
            OSimConnect.AddToFacilityDefinition(sd, "N_RUNWAYS");

            OSimConnect.AddToFacilityDefinition(sd, "OPEN RUNWAY");

            OSimConnect.AddToFacilityDefinition(sd, "PRIMARY_NUMBER");
            OSimConnect.AddToFacilityDefinition(sd, "LATITUDE");
            OSimConnect.AddToFacilityDefinition(sd, "HEADING");
            OSimConnect.AddToFacilityDefinition(sd, "LENGTH");
            OSimConnect.AddToFacilityDefinition(sd, "WIDTH");

            OSimConnect.AddToFacilityDefinition(sd, "OPEN PRIMARY_THRESHOLD");
            OSimConnect.AddToFacilityDefinition(sd, "LENGTH");
            OSimConnect.AddToFacilityDefinition(sd, "WIDTH");
            OSimConnect.AddToFacilityDefinition(sd, "ENABLE");
            OSimConnect.AddToFacilityDefinition(sd, "CLOSE PRIMARY_THRESHOLD");
            OSimConnect.AddToFacilityDefinition(sd, "OPEN SECONDARY_THRESHOLD");
            OSimConnect.AddToFacilityDefinition(sd, "LENGTH");
            OSimConnect.AddToFacilityDefinition(sd, "WIDTH");
            OSimConnect.AddToFacilityDefinition(sd, "ENABLE");
            OSimConnect.AddToFacilityDefinition(sd, "CLOSE SECONDARY_THRESHOLD");


            OSimConnect.AddToFacilityDefinition(sd, "CLOSE RUNWAY");

            OSimConnect.AddToFacilityDefinition(sd, "CLOSE AIRPORT");

            
            OSimConnect.RegisterFacilityDataDefineStruct<Airport>(SIMCONNECT_FACILITY_DATA_TYPE.AIRPORT);
            OSimConnect.RegisterFacilityDataDefineStruct<Runway>(SIMCONNECT_FACILITY_DATA_TYPE.RUNWAY);
            OSimConnect.RegisterFacilityDataDefineStruct<Pavement>(SIMCONNECT_FACILITY_DATA_TYPE.PAVEMENT);
            OSimConnect.RequestFacilityData(sd, rd, "LFML", "");
        }

        
    }
}

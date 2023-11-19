using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using BravoLights.Ast;
using BravoLights.Common;

namespace BravoLights.Connections
{       
    enum DefineId : uint
    {
        WASMRequestResponse = 1,
        WASMLVars = 2,

        // The start of dynamically-allocated definitions for simulator variables
        DynamicStart = 100
    }

    enum RequestId : uint
    {
        SimState = 1,
        WASMResponse = 2,
        WASMLVars = 3,
        AircraftLoaded = 4,
        FlightLoaded = 5,

        // The start of dynamically-allocated request ids for simulator variables
        DynamicStart = DefineId.DynamicStart
    }

    enum WASMReaderState
    {
        Neutral,
        ReadingLVars
    }

    class SimConnectConnection : IConnection, IWASMChannel
    {
        // Names of the client data areas established by the WASM module
        private const string CDA_NAME_SIMVAR = "BetterBravoLights.LVars";
        private const string CDA_NAME_REQUEST = "BetterBravoLights.Request";
        private const string CDA_NAME_RESPONSE = "BetterBravoLights.Response";

        // Ids for the client data areas
        private enum ClientDataId
        {
            LVars = 0,
            Request = 1,
            Response = 2
        }

        private const string RESPONSE_LVAR_START = "!LVARS-START";
        private const string RESPONSE_LVAR_END = "!LVARS-END";

        // Size of the request and response CDAs
        private const int RequestResponseSize = 256;

        [StructLayout(LayoutKind.Sequential)]
        public struct RequestString
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = RequestResponseSize)]
            public byte[] data;

            public RequestString(string str)
            {
                var bytes = Encoding.ASCII.GetBytes(str);
                var ret = new byte[RequestResponseSize];
                Array.Copy(bytes, ret, bytes.Length);
                data = ret;
            }
        }

        public struct ResponseString
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = RequestResponseSize)]
            public string Data;
        }

        // This MUST match the value in the WASM
        private const int MaxDataValuesInPacket = 10;

        public struct LVarData : ILVarData
        {
            [MarshalAs(UnmanagedType.U2)]
            public short ValueCount;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxDataValuesInPacket, ArraySubType = UnmanagedType.U2)]
            public short[] Ids;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxDataValuesInPacket, ArraySubType = UnmanagedType.R8)]
            public double[] Values;

            short ILVarData.ValueCount => ValueCount;

            short[] ILVarData.Ids => Ids;

            double[] ILVarData.Values => Values;
        }

        private readonly uint LVarDataSize = (uint)Marshal.SizeOf<LVarData>();

        public static SimConnectConnection Connection = new();

        private Timer reconnectTimer = null;
        private SimState simState = SimState.SimExited;

        private SimConnectConnection()
        {
            LVarManager.Connection.SetWASMChannel(this);
        }

        public void Start()
        {
            ConnectNow();
        }

        private uint nextVariableId = (uint)DefineId.DynamicStart;

        private readonly Dictionary<uint, NameAndUnits> idToName = new();
        private readonly Dictionary<NameAndUnits, uint> nameToId = new(new NameAndUnitsComparer());
        private readonly Dictionary<NameAndUnits, EventHandler<ValueChangedEventArgs>> variableHandlers =
            new(new NameAndUnitsComparer());
        private readonly Dictionary<NameAndUnits, double> lastReportedValue = new(new NameAndUnitsComparer());

        private void SubscribeToSimConnect(NameAndUnits nameAndUnits)
        {
            
        }

        private void UnsubscribeFromSimConnect(SimVarExpression simvar)
        {
        }

        public void AddListener(IVariable variable, EventHandler<ValueChangedEventArgs> handler)
        {
        }

        private void SendLastValue(NameAndUnits variable, EventHandler<ValueChangedEventArgs> handler)
        {
        }


        public void RemoveListener(IVariable variable, EventHandler<ValueChangedEventArgs> handler)
        {
        }

        private const int WM_USER_SIMCONNECT = 0x0402;


        public void ReceiveMessage()
        {
        }

        private void ReconnectTimerElapsed(object sender)
        {
        }

        private void ConnectNow()
        {
       
        }

        private void ConfigureWASMComms()
        {
          
        }

        private void RequestAircraftAndFlightStatus()
        {
        }

       

        private Timer periodicLVarTimer = null;

        private void PeriodicLVarTimerElapsed(object state)
        {
            Debug.WriteLine("PeriodicLVarTimerElapsed");
            ScheduleLVarCheck();
        }

        private void RaiseSimStateChanged(SimState state)
        {
        }

        private static readonly Regex AircraftPathRegex = new("Airplanes\\\\(.*)\\\\");

        private void HandleAircraftChanged(string aircraftPath)
        {
            if (OnAircraftLoaded != null)
            {
                // aircraftPath is something like
                // SimObjects\\Airplanes\\Asobo_C172SP_Classic\\aircraft.CFG
                // We just want 'Asobo_C172SP_Classic'
                var match = AircraftPathRegex.Match(aircraftPath);
                if (match.Success)
                {
                    var aircraftName = match.Groups[1].Value;
                    Debug.WriteLine($"HandleAircraftChanged {aircraftName}. Checking LVars");

                    // Note: LVars are not registered by an aircraft until a little while _after_ it has loaded.
                    // So the first time we get an AircraftChanged event, the lvars will not be present.
                    // However, we request aircraft + flight information on each SimStart/SimStop, which should catch them.
                    ScheduleLVarCheck();
                    OnAircraftLoaded(this, new AircraftEventArgs { Aircraft = aircraftName });
                }
            }
        }

        private void HandleFlightLoaded(string flightPath)
        {
            // Examples:
            // flights\other\MainMenu.FLT
            // C:\Users\royston\AppData\Local\Packages\Microsoft.FlightSimulator_8wekyb3d8bbwe\LocalState\MISSIONS\ACTIVITIES\ASOBO-BUSHTRIP-FINLAND_SAVE\ASOBO-BUSHTRIP-FINLAND_SAVE\ASOBO-BUSHTRIP-FINLAND_SAVE.FLT
            // C:\Users\royston\AppData\Local\Packages\Microsoft.FlightSimulator_8wekyb3d8bbwe\LocalState\MISSIONS\ACTIVITIES\ASOBO-BUSHTRIP-FINLAND_SAVE\AUTOSAVE\\BCKP\\ASOBO-BUSHTRIP-FINLAND_SAVE.FLT
            // missions\Asobo\Tutorials\VFRNavigation\LandmarkNavigation\03_Training_LandmarkNavigation.FLT

            InMainMenu = flightPath.EndsWith("flights\\other\\mainmenu.flt", StringComparison.InvariantCultureIgnoreCase);
            Debug.WriteLine($"HandleFlightLoaded. {flightPath}. Checking LVars");
            ScheduleLVarCheck();
        }

        /// <summary>
        /// Called when we receive some event that suggests that the simulator LVars may have changed, e.g. aircraft/flight change/simstart/simstop
        /// </summary>
        private void ScheduleLVarCheck()
        {
#if DEBUG
            Debug.WriteLine("ScheduleLVarCheck");
#endif

            if (lvarCheckTimer == null)
            {
                lvarCheckTimer = new(LVarCheckTimerElapsed, null, TimeSpan.FromSeconds(2), Timeout.InfiniteTimeSpan);
            }
        }

        private Timer lvarCheckTimer = null;

        /// <summary>
        /// Called every ~30s or so, just to check if new lvars have appeared. Some aircraft take a while to register theirs.
        /// </summary>
        private void LVarCheckTimerElapsed(object state)
        {
#if DEBUG
            Debug.WriteLine("LVarCheckTimerElapsed");
#endif
            lvarCheckTimer.Dispose();
            lvarCheckTimer = null;

            CheckForNewLVars();
        }

        private bool hasEverCheckedForLVars = false;

        /// <summary>
        /// Asks the WASM module to check for new LVars.
        /// </summary>
        private void CheckForNewLVars()
        {

        }

        public event EventHandler OnInMainMenuChanged;
        
        private bool inMainMenu = true;
        public bool InMainMenu
        {
            get {
                return inMainMenu;
            }
            private set
            {
                if (inMainMenu == value)
                {
                    return;
                }

                CheckForNewLVars();
                inMainMenu = value;
                OnInMainMenuChanged?.Invoke(this, EventArgs.Empty);
            }
        }

      

        private void RegisterCurrentVariables()
        {
            foreach (var nau in this.variableHandlers.Keys)
            {
                SubscribeToSimConnect(nau);
            }
        }

        private List<string> incomingLVars = new();

        private WASMReaderState readerState = WASMReaderState.Neutral;

    

        public static IntPtr HWnd { get; set; }

        public event EventHandler<AircraftEventArgs> OnAircraftLoaded;
        public void ClearSubscriptions()
        {
        }

        public void Subscribe(short id)
        {
        }

        public void Unsubscribe(short id)
        {
        }

        public SimState SimState
        {
            get { return simState; }
        }
        public event EventHandler<SimStateEventArgs> OnSimStateChanged;

    }

    public enum SimState
    {
        SimRunning,
        SimStopped,
        SimExited
    }

    class NameAndUnits
    {
        public string Name;
        public string Units;

        public override string ToString()
        {
            return $"{Name}, {Units}";
        }
    }

    class NameAndUnitsComparer : IEqualityComparer<NameAndUnits>
    {
        public bool Equals(NameAndUnits x, NameAndUnits y)
        {
            return x.Name == y.Name && x.Units == y.Units;
        }

        public int GetHashCode([DisallowNull] NameAndUnits obj)
        {
            return obj.Name.GetHashCode();
        }
    }

    public class SimStateEventArgs: EventArgs
    {
        public SimState SimState;
    }

    class AircraftEventArgs : EventArgs
    {
        public string Aircraft;
    }

    [StructLayout(LayoutKind.Explicit, CharSet = CharSet.Ansi, Pack = 1)]
    struct ContainerStruct
    {
        [FieldOffset(0)]
        public double doubleValue;
    }
}

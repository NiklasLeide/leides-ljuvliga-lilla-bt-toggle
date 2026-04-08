using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Enumeration;

namespace BtToggle;

class Program
{
    // Native Bluetooth API for real connect/disconnect
    [DllImport("BluetoothApis.dll", SetLastError = true)]
    private static extern uint BluetoothSetServiceState(
        IntPtr hRadio,
        ref BLUETOOTH_DEVICE_INFO pbtdi,
        ref Guid pGuidService,
        uint dwServiceFlags);

    [DllImport("BluetoothApis.dll", SetLastError = true)]
    private static extern IntPtr BluetoothFindFirstRadio(
        ref BLUETOOTH_FIND_RADIO_PARAMS pbtfrp,
        out IntPtr phRadio);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool CloseHandle(IntPtr hObject);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct BLUETOOTH_DEVICE_INFO
    {
        public uint dwSize;
        public ulong Address;
        public uint ulClassofDevice;
        [MarshalAs(UnmanagedType.Bool)] public bool fConnected;
        [MarshalAs(UnmanagedType.Bool)] public bool fRemembered;
        [MarshalAs(UnmanagedType.Bool)] public bool fAuthenticated;
        public SYSTEMTIME stLastSeen;
        public SYSTEMTIME stLastUsed;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 248)]
        public string szName;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct SYSTEMTIME
    {
        public ushort wYear, wMonth, wDayOfWeek, wDay;
        public ushort wHour, wMinute, wSecond, wMilliseconds;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct BLUETOOTH_FIND_RADIO_PARAMS
    {
        public uint dwSize;
    }

    private const uint BLUETOOTH_SERVICE_ENABLE = 0x01;
    private const uint BLUETOOTH_SERVICE_DISABLE = 0x00;

    // Well-known Bluetooth audio service GUIDs
    private static readonly Guid AudioSinkServiceId =
        new("0000110b-0000-1000-8000-00805f9b34fb"); // A2DP Sink
    private static readonly Guid HandsfreeServiceId =
        new("0000111e-0000-1000-8000-00805f9b34fb"); // Handsfree
    private static readonly Guid AudioSourceServiceId =
        new("0000110a-0000-1000-8000-00805f9b34fb"); // A2DP Source

    static async Task<int> Main(string[] args)
    {
        if (args.Length < 1)
        {
            Console.Error.WriteLine("Usage: bt-toggle <connect|disconnect>");
            return 1;
        }

        var macAddress = LoadMacAddress();
        if (macAddress == null) return 1;

        var command = args[0].ToLowerInvariant();

        return command switch
        {
            "connect" => await SetDeviceServiceState(macAddress, enable: true),
            "disconnect" => await SetDeviceServiceState(macAddress, enable: false),
            _ => ShowUnknownCommand(command)
        };
    }

    private static string? LoadMacAddress()
    {
        var envPaths = new[]
        {
            Path.Combine(AppContext.BaseDirectory, ".env"),
            Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".env"),
        };

        foreach (var path in envPaths)
        {
            if (!File.Exists(path)) continue;

            foreach (var line in File.ReadAllLines(path))
            {
                var trimmed = line.Trim();
                if (trimmed.StartsWith("BT_MAC_ADDRESS="))
                {
                    return trimmed["BT_MAC_ADDRESS=".Length..].Trim().Trim('"');
                }
            }
        }

        Console.Error.WriteLine("BT_MAC_ADDRESS not found.");
        Console.Error.WriteLine("Create a .env file with: BT_MAC_ADDRESS=XX:XX:XX:XX:XX:XX");
        return null;
    }

    private static async Task<int> SetDeviceServiceState(string targetMac, bool enable)
    {
        var action = enable ? "Connecting" : "Disconnecting";
        var flag = enable ? BLUETOOTH_SERVICE_ENABLE : BLUETOOTH_SERVICE_DISABLE;

        // Use WinRT to get the device name and verify it exists
        var macValue = ParseMacAddress(targetMac);
        var device = await BluetoothDevice.FromBluetoothAddressAsync(macValue);

        if (device == null)
        {
            Console.Error.WriteLine($"Device not found: {targetMac}");
            Console.Error.WriteLine("Make sure the device is paired in Windows Bluetooth settings.");
            return 1;
        }

        var deviceName = device.Name;
        device.Dispose();

        // Get a handle to the local Bluetooth radio
        var radioParams = new BLUETOOTH_FIND_RADIO_PARAMS
        {
            dwSize = (uint)Marshal.SizeOf<BLUETOOTH_FIND_RADIO_PARAMS>()
        };

        var findHandle = BluetoothFindFirstRadio(ref radioParams, out var radioHandle);
        if (findHandle == IntPtr.Zero)
        {
            Console.Error.WriteLine("No Bluetooth radio found.");
            return 1;
        }

        // Set up the device info struct
        var deviceInfo = new BLUETOOTH_DEVICE_INFO
        {
            dwSize = (uint)Marshal.SizeOf<BLUETOOTH_DEVICE_INFO>(),
            Address = macValue,
        };

        Console.WriteLine($"{action}: {deviceName}");

        // Toggle each audio service
        var services = new[] { AudioSinkServiceId, HandsfreeServiceId, AudioSourceServiceId };
        var anySuccess = false;

        foreach (var serviceId in services)
        {
            var guid = serviceId;
            var result = BluetoothSetServiceState(radioHandle, ref deviceInfo, ref guid, flag);
            if (result == 0)
                anySuccess = true;
        }

        CloseHandle(radioHandle);

        if (!anySuccess)
        {
            Console.Error.WriteLine($"Failed to {(enable ? "connect" : "disconnect")}.");
            return 1;
        }

        Console.WriteLine(enable ? "Connected." : "Disconnected.");
        return 0;
    }

    private static ulong ParseMacAddress(string mac)
    {
        var hex = mac.Replace(":", "").Replace("-", "");
        return Convert.ToUInt64(hex, 16);
    }

    private static int ShowUnknownCommand(string command)
    {
        Console.Error.WriteLine($"Unknown command: {command}");
        Console.Error.WriteLine("Usage: bt-toggle <connect|disconnect>");
        return 1;
    }
}

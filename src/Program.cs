using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Enumeration;

namespace BtToggle;

class Program
{
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
            "connect" => await ConnectAsync(macAddress),
            "disconnect" => await DisconnectAsync(macAddress),
            _ => ShowUnknownCommand(command)
        };
    }

    private static string? LoadMacAddress()
    {
        // Look for .env next to the executable, then in the project root
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

    private static async Task<int> ConnectAsync(string targetMac)
    {
        var device = await GetDeviceAsync(targetMac);
        if (device == null) return 1;

        using (device)
        {
            if (device.ConnectionStatus == BluetoothConnectionStatus.Connected)
            {
                Console.WriteLine($"Already connected: {device.Name}");
                return 0;
            }

            // Access an RFCOMM service to trigger the Bluetooth Classic connection.
            // For audio devices, requesting services initiates the connection.
            var servicesResult = await device.GetRfcommServicesAsync();

            if (servicesResult.Error != BluetoothError.Success)
            {
                Console.Error.WriteLine($"Failed to connect: {servicesResult.Error}");
                return 1;
            }

            Console.WriteLine($"Connected: {device.Name}");
            return 0;
        }
    }

    private static async Task<int> DisconnectAsync(string targetMac)
    {
        var selector = BluetoothDevice.GetDeviceSelectorFromPairingState(true);
        var devices = await DeviceInformation.FindAllAsync(selector);

        var macLookup = ParseMacAddress(targetMac);

        foreach (var deviceInfo in devices)
        {
            var btDevice = await BluetoothDevice.FromIdAsync(deviceInfo.Id);
            if (btDevice == null) continue;

            using (btDevice)
            {
                if (btDevice.BluetoothAddress == macLookup)
                {
                    if (btDevice.ConnectionStatus == BluetoothConnectionStatus.Disconnected)
                    {
                        Console.WriteLine($"Already disconnected: {btDevice.Name}");
                        return 0;
                    }

                    // Disposing the BluetoothDevice and its services signals the OS to drop the connection.
                    // Force GC to release COM references that hold the connection open.
                    Console.WriteLine($"Disconnecting: {btDevice.Name}");
                }
            }
        }

        // Force release of all COM references
        GC.Collect();
        GC.WaitForPendingFinalizers();

        Console.WriteLine("Disconnected.");
        return 0;
    }

    private static async Task<BluetoothDevice?> GetDeviceAsync(string targetMac)
    {
        var macAddress = ParseMacAddress(targetMac);
        var device = await BluetoothDevice.FromBluetoothAddressAsync(macAddress);

        if (device == null)
        {
            Console.Error.WriteLine($"Device not found: {targetMac}");
            Console.Error.WriteLine("Make sure the device is paired in Windows Bluetooth settings.");
            return null;
        }

        return device;
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

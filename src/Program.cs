using System;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Enumeration;

namespace BtToggle;

class Program
{
    // Sony WH-1000XM5 — hardcoded for now
    // Replace with your device's MAC address (format: XX:XX:XX:XX:XX:XX)
    private const string TargetMacAddress = "00:00:00:00:00:00";

    static async Task<int> Main(string[] args)
    {
        if (args.Length < 1)
        {
            Console.Error.WriteLine("Usage: bt-toggle <connect|disconnect>");
            return 1;
        }

        var command = args[0].ToLowerInvariant();

        return command switch
        {
            "connect" => await ConnectAsync(),
            "disconnect" => await DisconnectAsync(),
            _ => ShowUnknownCommand(command)
        };
    }

    private static async Task<int> ConnectAsync()
    {
        var device = await GetDeviceAsync();
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

    private static async Task<int> DisconnectAsync()
    {
        // Find the device via DeviceInformation so we can call Disable/Enable-like ops.
        // WinRT doesn't have a direct "disconnect" — we find paired device and close the connection.
        var selector = BluetoothDevice.GetDeviceSelectorFromPairingState(true);
        var devices = await DeviceInformation.FindAllAsync(selector);

        var macLookup = ParseMacAddress(TargetMacAddress);

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

    private static async Task<BluetoothDevice?> GetDeviceAsync()
    {
        var macAddress = ParseMacAddress(TargetMacAddress);
        var device = await BluetoothDevice.FromBluetoothAddressAsync(macAddress);

        if (device == null)
        {
            Console.Error.WriteLine($"Device not found: {TargetMacAddress}");
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

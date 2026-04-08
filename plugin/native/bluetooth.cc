#include <napi.h>
#include <windows.h>
#include <bluetoothapis.h>

#pragma comment(lib, "Bthprops.lib")

// Well-known Bluetooth audio service GUIDs
static const GUID AUDIO_SINK_SERVICE = // A2DP Sink
    {0x0000110b, 0x0000, 0x1000, {0x80, 0x00, 0x00, 0x80, 0x5f, 0x9b, 0x34, 0xfb}};
static const GUID HANDSFREE_SERVICE = // Handsfree
    {0x0000111e, 0x0000, 0x1000, {0x80, 0x00, 0x00, 0x80, 0x5f, 0x9b, 0x34, 0xfb}};
static const GUID AUDIO_SOURCE_SERVICE = // A2DP Source
    {0x0000110a, 0x0000, 0x1000, {0x80, 0x00, 0x00, 0x80, 0x5f, 0x9b, 0x34, 0xfb}};

static BTH_ADDR ParseMacAddress(const std::string& mac) {
    std::string hex;
    for (char c : mac) {
        if (c != ':' && c != '-') hex += c;
    }
    return strtoull(hex.c_str(), nullptr, 16);
}

static HANDLE GetRadioHandle() {
    BLUETOOTH_FIND_RADIO_PARAMS params = {sizeof(BLUETOOTH_FIND_RADIO_PARAMS)};
    HANDLE hRadio = nullptr;
    HBLUETOOTH_RADIO_FIND hFind = BluetoothFindFirstRadio(&params, &hRadio);
    if (hFind) BluetoothFindRadioClose(hFind);
    return hRadio;
}

// Sets Bluetooth service state (enable or disable) for a given MAC address.
// Returns an object: { success: bool, error?: string }
static Napi::Value SetServiceState(const Napi::CallbackInfo& info, bool enable) {
    Napi::Env env = info.Env();
    auto result = Napi::Object::New(env);

    if (info.Length() < 1 || !info[0].IsString()) {
        result.Set("success", false);
        result.Set("error", Napi::String::New(env, "MAC address string required"));
        return result;
    }

    std::string mac = info[0].As<Napi::String>().Utf8Value();
    BTH_ADDR address = ParseMacAddress(mac);

    HANDLE hRadio = GetRadioHandle();
    if (!hRadio) {
        result.Set("success", false);
        result.Set("error", Napi::String::New(env, "No Bluetooth radio found"));
        return result;
    }

    BLUETOOTH_DEVICE_INFO deviceInfo = {};
    deviceInfo.dwSize = sizeof(BLUETOOTH_DEVICE_INFO);
    deviceInfo.Address.ullLong = address;

    DWORD flag = enable ? BLUETOOTH_SERVICE_ENABLE : BLUETOOTH_SERVICE_DISABLE;
    const GUID* services[] = {&AUDIO_SINK_SERVICE, &HANDSFREE_SERVICE, &AUDIO_SOURCE_SERVICE};

    bool anySuccess = false;
    for (const GUID* svc : services) {
        GUID guid = *svc;
        DWORD res = BluetoothSetServiceState(hRadio, &deviceInfo, &guid, flag);
        if (res == ERROR_SUCCESS) anySuccess = true;
    }

    CloseHandle(hRadio);

    if (!anySuccess) {
        result.Set("success", false);
        result.Set("error", Napi::String::New(env, enable ? "Failed to connect" : "Failed to disconnect"));
        return result;
    }

    result.Set("success", true);
    return result;
}

// Check if a Bluetooth device is currently connected
static Napi::Value IsConnected(const Napi::CallbackInfo& info) {
    Napi::Env env = info.Env();
    auto result = Napi::Object::New(env);

    if (info.Length() < 1 || !info[0].IsString()) {
        result.Set("connected", false);
        result.Set("error", Napi::String::New(env, "MAC address string required"));
        return result;
    }

    std::string mac = info[0].As<Napi::String>().Utf8Value();
    BTH_ADDR address = ParseMacAddress(mac);

    HANDLE hRadio = GetRadioHandle();
    if (!hRadio) {
        result.Set("connected", false);
        result.Set("error", Napi::String::New(env, "No Bluetooth radio found"));
        return result;
    }

    BLUETOOTH_DEVICE_INFO deviceInfo = {};
    deviceInfo.dwSize = sizeof(BLUETOOTH_DEVICE_INFO);
    deviceInfo.Address.ullLong = address;

    DWORD res = BluetoothGetDeviceInfo(hRadio, &deviceInfo);
    CloseHandle(hRadio);

    if (res != ERROR_SUCCESS) {
        result.Set("connected", false);
        result.Set("error", Napi::String::New(env, "Device not found"));
        return result;
    }

    result.Set("connected", Napi::Boolean::New(env, deviceInfo.fConnected));
    result.Set("name", Napi::String::New(env, (const char16_t*)deviceInfo.szName));
    return result;
}

static Napi::Value Connect(const Napi::CallbackInfo& info) {
    return SetServiceState(info, true);
}

static Napi::Value Disconnect(const Napi::CallbackInfo& info) {
    return SetServiceState(info, false);
}

static Napi::Object Init(Napi::Env env, Napi::Object exports) {
    exports.Set("connect", Napi::Function::New(env, Connect));
    exports.Set("disconnect", Napi::Function::New(env, Disconnect));
    exports.Set("isConnected", Napi::Function::New(env, IsConnected));
    return exports;
}

NODE_API_MODULE(bluetooth, Init)

#include <napi.h>
#include <windows.h>
#include <bluetoothapis.h>
#include <string>

#pragma comment(lib, "Bthprops.lib")

// Bluetooth audio service GUIDs — only A2DP, NOT Handsfree.
// Handsfree (HFP) enables the mic which forces low-quality mono audio in games.
static const GUID AUDIO_SINK_SERVICE = // A2DP Sink
    {0x0000110b, 0x0000, 0x1000, {0x80, 0x00, 0x00, 0x80, 0x5f, 0x9b, 0x34, 0xfb}};
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

// Async worker for connect/disconnect — runs BluetoothSetServiceState off the main thread
class ServiceStateWorker : public Napi::AsyncWorker {
public:
    ServiceStateWorker(Napi::Env env, const std::string& mac, bool enable)
        : Napi::AsyncWorker(env),
          deferred_(Napi::Promise::Deferred::New(env)),
          mac_(mac), enable_(enable), success_(false) {}

    Napi::Promise Promise() { return deferred_.Promise(); }

protected:
    void Execute() override {
        BTH_ADDR address = ParseMacAddress(mac_);
        HANDLE hRadio = GetRadioHandle();
        if (!hRadio) {
            error_ = "No Bluetooth radio found";
            return;
        }

        BLUETOOTH_DEVICE_INFO deviceInfo = {};
        deviceInfo.dwSize = sizeof(BLUETOOTH_DEVICE_INFO);
        deviceInfo.Address.ullLong = address;

        DWORD flag = enable_ ? BLUETOOTH_SERVICE_ENABLE : BLUETOOTH_SERVICE_DISABLE;
        const GUID* services[] = {&AUDIO_SINK_SERVICE, &AUDIO_SOURCE_SERVICE};

        for (const GUID* svc : services) {
            GUID guid = *svc;
            DWORD res = BluetoothSetServiceState(hRadio, &deviceInfo, &guid, flag);
            if (res == ERROR_SUCCESS) success_ = true;
        }

        CloseHandle(hRadio);

        if (!success_) {
            error_ = enable_ ? "Failed to connect" : "Failed to disconnect";
        }
    }

    void OnOK() override {
        auto env = Env();
        auto result = Napi::Object::New(env);
        result.Set("success", success_);
        if (!error_.empty()) {
            result.Set("error", Napi::String::New(env, error_));
        }
        deferred_.Resolve(result);
    }

    void OnError(const Napi::Error& err) override {
        deferred_.Reject(err.Value());
    }

private:
    Napi::Promise::Deferred deferred_;
    std::string mac_;
    bool enable_;
    bool success_;
    std::string error_;
};

// Check if a Bluetooth device is currently connected (fast, stays synchronous)
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

// connect(mac) -> Promise<{success, error?}>
static Napi::Value Connect(const Napi::CallbackInfo& info) {
    Napi::Env env = info.Env();
    if (info.Length() < 1 || !info[0].IsString()) {
        auto deferred = Napi::Promise::Deferred::New(env);
        deferred.Reject(Napi::String::New(env, "MAC address string required"));
        return deferred.Promise();
    }
    std::string mac = info[0].As<Napi::String>().Utf8Value();
    auto* worker = new ServiceStateWorker(env, mac, true);
    worker->Queue();
    return worker->Promise();
}

// disconnect(mac) -> Promise<{success, error?}>
static Napi::Value Disconnect(const Napi::CallbackInfo& info) {
    Napi::Env env = info.Env();
    if (info.Length() < 1 || !info[0].IsString()) {
        auto deferred = Napi::Promise::Deferred::New(env);
        deferred.Reject(Napi::String::New(env, "MAC address string required"));
        return deferred.Promise();
    }
    std::string mac = info[0].As<Napi::String>().Utf8Value();
    auto* worker = new ServiceStateWorker(env, mac, false);
    worker->Queue();
    return worker->Promise();
}

static Napi::Object Init(Napi::Env env, Napi::Object exports) {
    exports.Set("connect", Napi::Function::New(env, Connect));
    exports.Set("disconnect", Napi::Function::New(env, Disconnect));
    exports.Set("isConnected", Napi::Function::New(env, IsConnected));
    return exports;
}

NODE_API_MODULE(bluetooth, Init)

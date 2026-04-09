const path = require("path");
const WebSocket = require("ws");

// Parse Stream Deck launch arguments
const args = process.argv.slice(2);
function getArg(name) {
  const idx = args.indexOf(name);
  return idx >= 0 ? args[idx + 1] : null;
}

const port = getArg("-port");
const pluginUUID = getArg("-pluginUUID");
const registerEvent = getArg("-registerEvent");

// Load native Bluetooth addon
const bluetooth = require(
  path.join(__dirname, "..", "build", "Release", "bluetooth.node")
);

// State: 0 = disconnected, 1 = connected
const STATE_DISCONNECTED = 0;
const STATE_CONNECTED = 1;

// Track settings per action context
const contexts = new Map();

const ws = new WebSocket(`ws://127.0.0.1:${port}`);

ws.on("error", (err) => {
  // Prevent unhandled error crash
  console.error("WebSocket error:", err.message);
});

ws.on("open", () => {
  ws.send(JSON.stringify({ event: registerEvent, uuid: pluginUUID }));
});

ws.on("message", (data) => {
  const msg = JSON.parse(data.toString());

  switch (msg.event) {
    case "willAppear":
      handleWillAppear(msg);
      break;
    case "willDisappear":
      contexts.delete(msg.context);
      break;
    case "keyDown":
      handleKeyDown(msg);
      break;
    case "didReceiveSettings":
      handleDidReceiveSettings(msg);
      break;
  }
});

function handleWillAppear(msg) {
  const { context, payload } = msg;
  const settings = payload.settings || {};
  contexts.set(context, {
    macAddress: settings.macAddress || "",
    mode: settings.mode || "toggle",
  });

  if (settings.macAddress) {
    updateButtonState(context, settings.macAddress);
  } else {
    setTitle(context, "No MAC");
    showAlert(context);
  }
}

function handleDidReceiveSettings(msg) {
  const { context, payload } = msg;
  const settings = payload.settings || {};
  contexts.set(context, {
    macAddress: settings.macAddress || "",
    mode: settings.mode || "toggle",
  });

  if (settings.macAddress) {
    updateButtonState(context, settings.macAddress);
  }
}

async function handleKeyDown(msg) {
  const { context } = msg;
  const ctxData = contexts.get(context);

  if (!ctxData || !ctxData.macAddress) {
    setTitle(context, "No MAC");
    showAlert(context);
    return;
  }

  const mac = ctxData.macAddress;
  const mode = ctxData.mode || "toggle";
  const status = bluetooth.isConnected(mac);

  if (status.error) {
    setTitle(context, "Error");
    setErrorState(context);
    showAlert(context);
    return;
  }

  // Determine desired action based on mode and current state
  let shouldConnect;
  if (mode === "connect") {
    if (status.connected) {
      setTitle(context, "Already\nconnected");
      showOk(context);
      return;
    }
    shouldConnect = true;
  } else if (mode === "disconnect") {
    if (!status.connected) {
      setTitle(context, "Already\ndisconnected");
      showOk(context);
      return;
    }
    shouldConnect = false;
  } else {
    // toggle (default)
    shouldConnect = !status.connected;
  }

  // Show immediate feedback
  setTitle(context, shouldConnect ? "Connecting..." : "Disconnecting...");

  // Run the slow Bluetooth call on a worker thread (returns Promise)
  const result = await (shouldConnect
    ? bluetooth.connect(mac)
    : bluetooth.disconnect(mac));

  if (result.success) {
    setState(context, shouldConnect ? STATE_CONNECTED : STATE_DISCONNECTED);
    setTitle(context, shouldConnect ? status.name || "Connected" : "Disconnected");
    showOk(context);
  } else {
    setTitle(context, "Error");
    showAlert(context);
  }
}

function updateButtonState(context, mac) {
  const status = bluetooth.isConnected(mac);

  if (status.error) {
    setTitle(context, "Error");
    setErrorState(context);
    return;
  }

  if (status.connected) {
    setState(context, STATE_CONNECTED);
    setTitle(context, status.name || "Connected");
  } else {
    setState(context, STATE_DISCONNECTED);
    setTitle(context, "Disconnected");
  }
}

function setErrorState(context) {
  setState(context, STATE_DISCONNECTED);
  setImage(context, getBase64Image("bt-error"));
}

// Stream Deck WebSocket helpers
function send(event, context, payload) {
  ws.send(JSON.stringify({ event, context, payload }));
}

function setState(context, state) {
  send("setState", context, { state });
}

function setTitle(context, title) {
  send("setTitle", context, { title });
}

function setImage(context, image) {
  send("setImage", context, { image });
}

function showOk(context) {
  ws.send(JSON.stringify({ event: "showOk", context }));
}

function showAlert(context) {
  ws.send(JSON.stringify({ event: "showAlert", context }));
}

function getBase64Image(name) {
  try {
    const fs = require("fs");
    const imgPath = path.join(__dirname, "..", "assets", "actions", `${name}.svg`);
    const data = fs.readFileSync(imgPath);
    return `data:image/svg+xml;base64,${data.toString("base64")}`;
  } catch {
    return "";
  }
}

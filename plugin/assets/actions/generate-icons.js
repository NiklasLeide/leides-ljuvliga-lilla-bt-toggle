// Generate Stream Deck icon PNGs using Canvas-free SVG-to-base64 approach.
// Run: node generate-icons.js
// Outputs PNG files in the current directory.

const fs = require("fs");
const path = require("path");

function createSvg(color, size) {
  const scale = size / 72;
  const sw = 3 * scale;
  const cx = size / 2;
  const cy = size / 2;
  const h = 20 * scale;
  const w = 10 * scale;

  return `<svg xmlns="http://www.w3.org/2000/svg" width="${size}" height="${size}" viewBox="0 0 ${size} ${size}">
  <rect width="${size}" height="${size}" rx="${8 * scale}" fill="#000"/>
  <g stroke="${color}" stroke-width="${sw}" stroke-linecap="round" stroke-linejoin="round" fill="none">
    <polyline points="${cx - w},${cy - h * 0.5} ${cx + w},${cy - h} ${cx},${cy} ${cx + w},${cy + h} ${cx - w},${cy + h * 0.5}"/>
    <line x1="${cx}" y1="${cy - h}" x2="${cx}" y2="${cy + h}"/>
  </g>
</svg>`;
}

const icons = [
  { name: "bt-connected", color: "#4CAF50" },
  { name: "bt-disconnected", color: "#888888" },
  { name: "bt-error", color: "#F44336" },
];

const dir = __dirname;

for (const icon of icons) {
  // Stream Deck accepts SVG files too — save as SVG
  for (const [suffix, size] of [["", 72], ["@2x", 144]]) {
    const svg = createSvg(icon.color, size);
    const filename = `${icon.name}${suffix}.svg`;
    fs.writeFileSync(path.join(dir, filename), svg);
    console.log(`Created ${filename}`);
  }
}

console.log("Done. Icons generated as SVG files.");

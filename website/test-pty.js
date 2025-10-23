const pty = require("node-pty");

const proc = pty.spawn(
  "powershell.exe",
  [
    "-NoExit",
    "-Command",
    "cd dotnet/console-app/LablabBean.Console; dotnet run",
  ],
  {
    name: "xterm-color",
    cols: 80,
    rows: 24,
  },
);

let buffer = "";
let hasOutput = false;

proc.onData((data) => {
  buffer += data;
  hasOutput = true;
  process.stdout.write(data);
});

proc.onExit((e) => {
  console.log(`\n\n=== EXITED with code ${e.exitCode} ===`);
  console.log(`Total output length: ${buffer.length} chars`);
  console.log(`Has output: ${hasOutput}`);
  process.exit();
});

setTimeout(() => {
  console.log(`\n\n=== TIMEOUT after 15s ===`);
  console.log(`Total output: ${buffer.length} chars`);
  console.log(`Has output: ${hasOutput}`);
  if (buffer.length > 0) {
    console.log("\n=== First 500 chars ===");
    console.log(buffer.substring(0, 500));
  }
  proc.kill();
  process.exit();
}, 15000);

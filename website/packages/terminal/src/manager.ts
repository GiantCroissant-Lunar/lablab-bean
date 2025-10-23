import * as pty from "node-pty";
import type { WebSocket } from "ws";
import type { TerminalOptions, TerminalSession } from "./types.js";
import { randomUUID } from "crypto";

export class TerminalManager {
  private sessions: Map<string, TerminalSession> = new Map();

  createSession(ws: WebSocket, options: TerminalOptions = {}): string {
    const sessionId = randomUUID();

    // Determine shell and args based on platform
    let shell: string;
    let shellArgs: string[] = [];

    if (process.platform === "win32") {
      // On Windows, use PowerShell to run the console app
      shell = "powershell.exe";

      // Auto-run console app if enabled (default: true)
      if (options.autoRunConsoleApp !== false) {
        // Find the console app path relative to the terminal package
        const consoleAppPath =
          options.consoleAppPath ||
          "..\\..\\..\\dotnet\\console-app\\LablabBean.Console";

        shellArgs = [
          "-NoExit", // Keep PowerShell open after command
          "-Command",
          `cd "${consoleAppPath}"; dotnet run`,
        ];
      }
    } else {
      shell = options.shell || "bash";
    }

    // Create PTY process
    const ptyProcess = pty.spawn(shell, shellArgs, {
      name: "xterm-color",
      cols: options.cols || 80,
      rows: options.rows || 24,
      cwd: options.cwd || process.cwd(),
      env: { ...process.env, ...options.env },
    });

    // Handle PTY data
    ptyProcess.onData((data) => {
      if (ws.readyState === 1) {
        // WebSocket.OPEN
        ws.send(data);
      }
    });

    // Handle PTY exit
    ptyProcess.onExit(({ exitCode, signal }) => {
      console.log(`PTY process exited with code ${exitCode}, signal ${signal}`);
      this.destroySession(sessionId);
    });

    // Store session
    const session: TerminalSession = {
      id: sessionId,
      pty: ptyProcess,
      ws,
      createdAt: new Date(),
    };

    this.sessions.set(sessionId, session);

    // Handle WebSocket close
    ws.on("close", () => {
      this.destroySession(sessionId);
    });

    return sessionId;
  }

  getSession(sessionId: string): TerminalSession | undefined {
    return this.sessions.get(sessionId);
  }

  writeToSession(sessionId: string, data: string): boolean {
    const session = this.sessions.get(sessionId);
    if (session) {
      session.pty.write(data);
      return true;
    }
    return false;
  }

  resizeSession(sessionId: string, cols: number, rows: number): boolean {
    const session = this.sessions.get(sessionId);
    if (session) {
      session.pty.resize(cols, rows);
      return true;
    }
    return false;
  }

  destroySession(sessionId: string): void {
    const session = this.sessions.get(sessionId);
    if (session) {
      try {
        session.pty.kill();
      } catch (error) {
        console.error("Error killing PTY:", error);
      }
      this.sessions.delete(sessionId);
    }
  }

  destroyAllSessions(): void {
    for (const sessionId of this.sessions.keys()) {
      this.destroySession(sessionId);
    }
  }

  getSessionCount(): number {
    return this.sessions.size;
  }
}

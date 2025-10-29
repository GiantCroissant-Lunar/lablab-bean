import type { Server } from "http";
import { WebSocketServer } from "ws";
import { TerminalManager } from "./manager.js";
import type { TerminalOptions } from "./types.js";

export class TerminalServer {
  private wss: WebSocketServer;
  private manager: TerminalManager;
  private defaultOptions: TerminalOptions;

  constructor(
    server: Server,
    options: {
      path?: string;
      autoRunConsoleApp?: boolean;
      consoleAppPath?: string;
    } = {},
  ) {
    this.manager = new TerminalManager();

    // Store default options for new sessions
    this.defaultOptions = {
      autoRunConsoleApp: options.autoRunConsoleApp,
      consoleAppPath: options.consoleAppPath,
    };

    this.wss = new WebSocketServer({
      server,
      path: options.path || "/terminal",
    });

    this.wss.on("connection", (ws) => {
      console.log("New WebSocket connection");

      const terminalOptions: TerminalOptions = {
        cols: 80,
        rows: 24,
        ...this.defaultOptions, // Include console app options
      };

      const sessionId = this.manager.createSession(ws, terminalOptions);
      console.log(`Created terminal session: ${sessionId}`);

      ws.on("message", (data) => {
        try {
          const message = data.toString();

          // Try to parse as JSON for control messages
          try {
            const parsed = JSON.parse(message);

            if (parsed.type === "resize") {
              console.log(
                `Resizing terminal ${sessionId} to ${parsed.cols}x${parsed.rows}`,
              );
              this.manager.resizeSession(sessionId, parsed.cols, parsed.rows);
              return;
            }
          } catch {
            // Not JSON, treat as terminal input
          }

          // Write data to terminal
          this.manager.writeToSession(sessionId, message);
        } catch (error) {
          console.error("Error handling message:", error);
        }
      });

      ws.on("error", (error) => {
        console.error("WebSocket error:", error);
      });

      ws.on("close", () => {
        console.log(`WebSocket closed for session: ${sessionId}`);
      });
    });

    this.wss.on("error", (error) => {
      console.error("WebSocket server error:", error);
    });
  }

  close(): void {
    this.manager.destroyAllSessions();
    this.wss.close();
  }

  getSessionCount(): number {
    return this.manager.getSessionCount();
  }
}

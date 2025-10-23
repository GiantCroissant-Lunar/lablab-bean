import type { IPty } from "node-pty";
import type { WebSocket } from "ws";

export interface TerminalOptions {
  shell?: string;
  cwd?: string;
  env?: Record<string, string>;
  cols?: number;
  rows?: number;
  autoRunConsoleApp?: boolean; // Auto-run console app on session start (default: true)
  consoleAppPath?: string; // Path to console app (relative to terminal package)
}

export interface TerminalSession {
  id: string;
  pty: IPty;
  ws: WebSocket;
  createdAt: Date;
}

export interface ResizeMessage {
  type: "resize";
  cols: number;
  rows: number;
}

export type TerminalMessage = ResizeMessage | string;

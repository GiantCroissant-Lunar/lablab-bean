import type { Server } from 'http';
import { TerminalServer } from '@lablab-bean/terminal';

let terminalServer: TerminalServer | null = null;

export function setupTerminalServer(server: Server) {
  if (!terminalServer) {
    terminalServer = new TerminalServer(server, {
      path: '/terminal',
    });
    console.log('Terminal WebSocket server started on /terminal');
  }
  return terminalServer;
}

export function getTerminalServer() {
  return terminalServer;
}

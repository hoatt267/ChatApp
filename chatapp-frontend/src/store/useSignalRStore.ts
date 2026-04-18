import { create } from "zustand";
import * as signalR from "@microsoft/signalr";
import { APP_CONFIG } from "../config";
import { useAuthStore } from "../features/auth/store/useAuthStore";

interface SignalRState {
  connection: signalR.HubConnection | null;
  connect: () => void;
  disconnect: () => void;
}

export const useSignalRStore = create<SignalRState>((set, get) => ({
  connection: null,

  connect: () => {
    if (get().connection) return;

    const token = useAuthStore.getState().accessToken;
    if (!token) return;

    const newConnection = new signalR.HubConnectionBuilder()
      .withUrl(APP_CONFIG.SIGNALR_URL, {
        accessTokenFactory: () => token,
        skipNegotiation: true,
        transport: signalR.HttpTransportType.WebSockets,
      })
      .withAutomaticReconnect()
      .configureLogging(signalR.LogLevel.Error)
      .build();

    newConnection
      .start()
      .then(() => {
        console.log("🟢 SIGNALR CONNECTED!");
        set({ connection: newConnection });
      })
      .catch((err) => {
        console.error("🔴 SIGNALR CONNECTION ERROR:", err);
        set({ connection: null });
      });
  },

  disconnect: () => {
    const { connection } = get();
    if (connection) {
      connection.stop();
      set({ connection: null });
      console.log("⚪ SIGNALR DISCONNECTED");
    }
  },
}));
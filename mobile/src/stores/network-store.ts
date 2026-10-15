import * as Network from "expo-network";
import { create } from "zustand";

type NetworkState = {
  connected: boolean;
  reachable: boolean;
  initialized: boolean;
  initialize: () => Promise<() => void>;
};

export const useNetworkStore = create<NetworkState>((set) => ({
  connected: true,
  reachable: true,
  initialized: false,
  initialize: async () => {
    const current = await Network.getNetworkStateAsync();
    set({
      connected: current.isConnected !== false,
      reachable: current.isInternetReachable !== false,
      initialized: true,
    });
    const subscription = Network.addNetworkStateListener((state) =>
      set({
        connected: state.isConnected !== false,
        reachable: state.isInternetReachable !== false,
      }),
    );
    return () => subscription.remove();
  },
}));

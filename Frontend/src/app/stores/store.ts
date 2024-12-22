import { createContext, useContext } from "react";
import UserStore from "./userStore";
import WorkspaceStore from "./workspaceStore";

interface Store {
  userStore: UserStore;
  workspaceStore: WorkspaceStore;
}

export const store: Store = {
  userStore: new UserStore(),
  workspaceStore: new WorkspaceStore(),
};

export function useStore() {
  return useContext(createContext(store));
}

import { createContext, useContext } from "react";
import UserStore from "./userStore";
import WorkspaceStore from "./workspaceStore";
import DocumentStore from "./documentsStore";

interface Store {
  userStore: UserStore;
  workspaceStore: WorkspaceStore;
  documentStore: DocumentStore;
}

export const store: Store = {
  userStore: new UserStore(),
  workspaceStore: new WorkspaceStore(),
  documentStore: new DocumentStore(),
};

export function useStore() {
  return useContext(createContext(store));
}

import { createContext, useContext } from "react"
import UserStore from "./userStore";

interface Store {
    userStore: UserStore;
}

export const store: Store = {
    userStore: new UserStore()
}

export function useStore() {
    return useContext(createContext(store));
}
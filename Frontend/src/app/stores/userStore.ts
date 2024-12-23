import { User, UserLoginValues, UserRegisterValues } from "../models/User";
import { makeAutoObservable, runInAction } from "mobx";
import { router } from "../routes/routes";
import agent from "../api/agent";
import { store } from "./store";

export default class UserStore {
  user: User | null = null;

  constructor() {
    makeAutoObservable(this);
  }

  login = async (value: UserLoginValues) => {
    try {
      const user = await agent.Account.login(value);
      runInAction(() => (this.user = user));
      store.workspaceStore.loadWorkspaces(user.username);
      router.navigate("/nodocument");
    } catch (error) {
      console.error(error);
    }
  };

  register = async (value: UserRegisterValues) => {
    try {
      await agent.Account.register(value);
      router.navigate("/");
    } catch (error) {
      console.error(error);
    }
  };

  logout = () => {
    runInAction(() => (this.user = null));
    store.workspaceStore.clearWorkspaces();
    store.documentStore.clearDocuments();
    router.navigate("/");
  };
}

import { makeAutoObservable, runInAction } from "mobx";
import Workspace, { PermissionLevel } from "../models/Workspace";
import agent from "../api/agent";
import { UserInWorkspace } from "../models/User";
import { store } from "./store";

export default class WorkspaceStore {
  workspaces: Map<string, Workspace> | null = null;
  usersInWorkspace: UserInWorkspace[] | null = null;

  constructor() {
    makeAutoObservable(this);
  }

  loadWorkspaces = async (username: string) => {
    try {
      const result = await agent.Workspaces.list(username);
      runInAction(() => {
        this.workspaces = new Map();
        result.forEach((res) => this.workspaces!.set(res.workspaceId, res));
        store.documentStore.populateDocuments(result.map((x) => x.workspaceId));
      });
    } catch (error) {
      console.error(error);
    }
  };

  createWorkspace = async (name: string, username: string) => {
    try {
      const result = await agent.Workspaces.create(name, username);
      runInAction(() => {
        this.workspaces!.set(result.workspaceId, result);
        store.documentStore.newEntry(result.workspaceId);
      });
    } catch (error) {
      console.error(error);
    }
  };

  joinWorkspace = async (username: string, link: string) => {
    try {
      const result = await agent.Workspaces.join(username, link);
      runInAction(() => {
        this.workspaces!.set(result.workspaceId, result);
        store.documentStore.newEntry(result.workspaceId);
      });
    } catch (error) {
      console.error(error);
    }
  };

  delete = async (workspaceId: string, username: string) => {
    try {
      await agent.Workspaces.delete(workspaceId, username);
      store.documentStore.clearEntries(workspaceId);
      runInAction(() => this.workspaces!.delete(workspaceId));
    } catch (error) {
      console.error(error);
    }
  };

  changeName = async (workspaceId: string, newName: string) => {
    try {
      await agent.Workspaces.changeName(workspaceId, newName);
      runInAction(() => {
        const old = this.workspaces!.get(workspaceId);
        this.workspaces!.delete(workspaceId);
        this.workspaces!.set(workspaceId, { ...old!, workspaceName: newName });
      });
      return true;
    } catch (error) {
      console.error(error);
      return false;
    }
  };

  refresh = async (ownerUsername: string, workspaceId: string) => {
    try {
      const result = await agent.Workspaces.refresh(workspaceId, ownerUsername);
      store.documentStore.loadDocuments(workspaceId);
      runInAction(() => {
        this.workspaces!.delete(workspaceId);
        this.workspaces!.set(workspaceId, result);
      });
    } catch (error) {
      console.error(error);
    }
  };

  users = async (workspaceId: string) => {
    try {
      const result = await agent.Workspaces.users(workspaceId);
      runInAction(() => {
        this.usersInWorkspace = [];
        result.forEach((ent) => this.usersInWorkspace?.push(ent));
      });
    } catch (error) {
      console.error(error);
    }
  };

  kick = async (workspaceId: string, username: string, performer: string) => {
    try {
      await agent.Workspaces.kick(workspaceId, username, performer);
      runInAction(() => {
        this.usersInWorkspace = this.usersInWorkspace!.filter(
          (x) => x.username != username
        );
      });
      return true;
    } catch (error) {
      console.error(error);
      return false;
    }
  };

  permChange = async (
    workspaceId: string,
    username: string,
    newPermLevel: PermissionLevel,
    performer: string
  ) => {
    try {
      await agent.Workspaces.permChange(
        workspaceId,
        username,
        newPermLevel,
        performer
      );
      runInAction(() => {
        this.usersInWorkspace = this.usersInWorkspace!.map((x) => {
          return {
            ...x,
            permission: x.username == username ? newPermLevel : x.permission,
          };
        });
      });
      return true;
    } catch (error) {
      console.error(error);
      return false;
    }
  };

  clearWorkspaces = () => {
    runInAction(() => {
      this.workspaces = null;
      this.usersInWorkspace = null;
    });
  };
}

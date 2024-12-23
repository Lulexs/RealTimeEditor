import { makeAutoObservable, runInAction } from "mobx";
import Workspace, { PermissionLevel } from "../models/Workspace";
import agent from "../api/agent";
import { UserInWorkspace } from "../models/User";

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
      });
    } catch (error) {
      console.error(error);
    }
  };

  createWorkspace = async (name: string, username: string) => {
    try {
      const result = await agent.Workspaces.create(name, username);
      runInAction(() => this.workspaces!.set(result.workspaceId, result));
    } catch (error) {
      console.error(error);
    }
  };

  joinWorkspace = async (username: string, link: string) => {
    try {
      const result = await agent.Workspaces.join(username, link);
      runInAction(() => this.workspaces!.set(result.workspaceId, result));
    } catch (error) {
      console.error(error);
    }
  };

  delete = async (workspaceId: string, username: string) => {
    try {
      await agent.Workspaces.delete(workspaceId, username);
      runInAction(() => this.workspaces!.delete(workspaceId));
    } catch (error) {
      console.error(error);
    }
  };

  changeName = async (
    workspaceId: string,
    ownerUsername: string,
    userUsername: string,
    newName: string
  ) => {
    try {
      await agent.Workspaces.changeName(
        workspaceId,
        ownerUsername,
        userUsername,
        newName
      );
      runInAction(() => {
        const old = this.workspaces!.get(workspaceId);
        this.workspaces!.delete(workspaceId);
        this.workspaces!.set(workspaceId, { ...old!, workspaceName: newName });
      });
    } catch (error) {
      console.error(error);
    }
  };

  refresh = async (ownerUsername: string, workspaceId: string) => {
    try {
      const result = await agent.Workspaces.refresh(workspaceId, ownerUsername);
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
    } catch (error) {
      console.error(error);
    }
  };

  permChange = async (
    username: string,
    newPermLevel: PermissionLevel,
    performer: string
  ) => {
    try {
      await agent.Workspaces.permChange(username, newPermLevel, performer);
      runInAction(() => {
        this.usersInWorkspace = this.usersInWorkspace!.map((x) => {
          return {
            ...x,
            permission: x.username == username ? newPermLevel : x.permission,
          };
        });
      });
    } catch (error) {
      console.error(error);
    }
  };

  clearWorkspaces = () => {
    runInAction(() => {
      this.workspaces = null;
      this.usersInWorkspace = null;
    });
  };
}

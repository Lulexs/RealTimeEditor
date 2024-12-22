import { makeAutoObservable } from "mobx";
import Workspace from "../models/Workspace";

export default class WorkspaceStore {
    workspaces: Map<string, Workspace> = new Map();
    selectedWorkspace: Workspace | undefined;

    constructor() {
        makeAutoObservable(this);
    }

    loadWorkspaces = async() => {
        
    }
}

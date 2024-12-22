export enum PermissionLevel {
  ViewOnly,
  Edit,
  Admin,
  Owner,
}

export default interface Workspace {
  workspaceId: string;
  workspaceName: string;
  ownername: string;
  premissionlevel: PermissionLevel;
  createdat: Date;
}

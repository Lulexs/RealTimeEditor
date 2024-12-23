export enum PermissionLevel {
  ViewOnly,
  Edit,
  Admin,
  Owner,
}

export default interface Workspace {
  workspaceId: string;
  workspaceName: string;
  ownerUsername: string;
  permission: PermissionLevel;
  createdAt: Date;
}

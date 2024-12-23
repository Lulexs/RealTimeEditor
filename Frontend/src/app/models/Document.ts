export interface Document {
  workspaceId: string;
  documentId: string;
  documentName: string;
  createdAt: Date;
  creatorUsername: string;
  snapshotIds: Snapshot[];
}

export interface Snapshot {
  name: string;
  createdAt: Date;
}

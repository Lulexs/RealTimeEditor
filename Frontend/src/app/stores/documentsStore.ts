import { makeAutoObservable, runInAction } from "mobx";
import { Document } from "../models/Document";
import agent from "../api/agent";
import { notifications } from "@mantine/notifications";

export default class DocumentStore {
  documents: Map<string, Map<string, Document>> = new Map();
  selectedDocument: Document | null = null;

  constructor() {
    makeAutoObservable(this);
  }

  populateDocuments = (workspaceIds: string[]) => {
    workspaceIds.forEach((id) => this.documents.set(id, new Map()));
  };

  clearEntries = (workspaceId: string) => {
    this.documents.delete(workspaceId);
  };

  clearDocuments = () => {
    runInAction(() => {
      this.documents = new Map();
      this.selectedDocument = null;
    });
  };

  selectDocument = (doc: Document) => {
    runInAction(() => (this.selectedDocument = doc));
  };

  loadDocuments = async (workspaceId: string) => {
    try {
      const result = await agent.Documents.list(workspaceId);
      runInAction(() => {
        this.documents.set(workspaceId, new Map());
        result.forEach((res) =>
          this.documents.get(workspaceId)!.set(res.documentId, res)
        );
      });
    } catch (error) {
      console.error(error);
    }
  };

  newDocument = async (
    workspaceId: string,
    documentName: string,
    creatorUsername: string
  ) => {
    try {
      const result = await agent.Documents.create(
        workspaceId,
        documentName,
        creatorUsername
      );
      runInAction(() => {
        this.documents.get(workspaceId)!.set(result.documentId, result);
      });
    } catch (error) {
      console.error(error);
    }
  };

  delete = async (
    workspaceId: string,
    documentId: string,
    username: string
  ) => {
    try {
      await agent.Documents.delete(workspaceId, documentId, username);
      runInAction(() => this.documents.get(workspaceId)!.delete(documentId));
    } catch (error) {
      console.error(error);
    }
  };

  changeDocumentName = async (
    workspaceId: string,
    documentId: string,
    newName: string
  ) => {
    try {
      await agent.Documents.changeName(workspaceId, documentId, newName);
      runInAction(() => {
        const workspaceMap = this.documents.get(workspaceId);
        if (workspaceMap) {
          const document = workspaceMap.get(documentId);
          if (document) {
            document.documentName = newName;
            workspaceMap.set(documentId, document);
            this.documents.set(workspaceId, workspaceMap);
          }
        }
      });
    } catch (error) {
      console.error(error);
    }
  };

  createSnapshot = async (documentId: string) => {
    try {
      const result = await agent.Documents.snapshot(documentId);
      runInAction(() => {
        this.selectedDocument?.snapshotIds.push(result);
        console.log(this.documents);
        notifications.show({
          title: "Success",
          message: `Successfully created snapshot ${result.name}, at ${new Date(
            result.createdAt
          ).toLocaleString()}`,
        });
      });
    } catch (error) {
      console.error(error);
    }
  };

  forkSnapshot = async (
    workspaceId: string,
    documentId: string,
    documentName: string,
    snapshotName: string,
    forker: string
  ) => {
    try {
      const result = await agent.Documents.forkSnapshot(
        workspaceId,
        documentId,
        documentName,
        snapshotName,
        forker
      );
      runInAction(() => {
        this.documents.get(workspaceId)!.set(result.documentId, result);
      });
    } catch (error) {
      console.error(error);
    }
  };
}

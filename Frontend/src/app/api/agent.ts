import { notifications } from "@mantine/notifications";
import axios, { AxiosError, AxiosResponse } from "axios";
import {
  User,
  UserInWorkspace,
  UserLoginValues,
  UserRegisterValues,
} from "../models/User";
import Workspace, { PermissionLevel } from "../models/Workspace";
import { Document, Snapshot } from "../models/Document";

axios.defaults.baseURL = "http://localhost:5287";

const responseBody = <T>(response: AxiosResponse<T>) => response?.data;

axios.interceptors.response.use(
  (value) => value,
  (error: AxiosError) => {
    notifications.show({
      color: "red",
      title: "Error",
      message: error.response?.data as string,
    });
    return Promise.reject(error);
  }
);

const requests = {
  get: <T>(url: string) => axios.get<T>(url).then(responseBody),
  post: <T>(url: string, body: {}) =>
    axios.post<T>(url, body).then(responseBody),
  put: <T>(url: string, body: {}) => axios.put<T>(url, body).then(responseBody),
  del: <T>(url: string) => axios.delete<T>(url).then(responseBody),
};

const Account = {
  login: (user: UserLoginValues) => requests.post<User>("/users/login", user),
  register: (user: UserRegisterValues) =>
    requests.post<void>("/users/register", user),
};

const Workspaces = {
  list: (username: string) =>
    requests.get<Workspace[]>(`/workspaces/${username}`),
  create: (name: string, ownerName: string) =>
    requests.post<Workspace>("/workspaces", {
      Name: name,
      OwnerName: ownerName,
    }),
  join: (username: string, link: string) =>
    requests.post<Workspace>("/workspaces/join", {
      Username: username,
      JoinCode: link,
    }),
  delete: (workspaceId: string, username: string) =>
    requests.del<void>(`/workspaces/${workspaceId}/${username}`),
  changeName: (workspaceId: string, newName: string) =>
    requests.put<void>(`/workspaces`, {
      WorkspaceId: workspaceId,
      NewName: newName,
    }),
  refresh: (workspaceId: string, ownerUsername: string) =>
    requests.get<Workspace>(`/workspaces/${ownerUsername}/${workspaceId}`),
  users: (workspaceId: string) =>
    requests.get<UserInWorkspace[]>(`/workspaces/users/${workspaceId}`),
  kick: (workspaceId: string, username: string, performer: string) =>
    requests.del<void>(
      `/workspaces/users/${workspaceId}/${username}/${performer}`
    ),
  permChange: (
    workspaceId: string,
    username: string,
    newPermLevel: PermissionLevel,
    performer: string
  ) =>
    requests.put<void>(
      `/workspaces/users/${workspaceId}/${username}/${newPermLevel}/${performer}`,
      {}
    ),
  lock: (workspaceId: string, newName: string) =>
    requests.post<void>(`/workspaces/lock`, {
      WorkspaceId: workspaceId,
      NewName: newName,
    }),
  lockUserManagement: (workspaceId: string) =>
    requests.post<void>(`/workspaces/lockKickChangePermLevel`, {
      WorkspaceId: workspaceId,
    }),
};

const Documents = {
  list: (workspaceId: string) =>
    requests.get<Document[]>(`/documents/${workspaceId}`),
  create: (
    workspaceId: string,
    documentName: string,
    creatorUsername: string
  ) =>
    requests.post<Document>(`/documents`, {
      workspaceId,
      documentName,
      creatorUsername,
    }),
  changeName: (workspaceId: string, documentId: string, newName: string) =>
    requests.put<void>(`/documents`, {
      WorkspaceId: workspaceId,
      DocumentId: documentId,
      NewName: newName,
    }),
  delete: (workspaceId: string, documentId: string, username: string) =>
    requests.del<void>(`/documents/${workspaceId}/${documentId}/${username}`),
  snapshot: (documentId: string) =>
    requests.post<Snapshot>(`/documents/snapshots/${documentId}`, {}),
  forkSnapshot: (
    workspaceId: string,
    documentId: string,
    documentName: string,
    snapshotName: string,
    forker: string
  ) =>
    requests.post<Document>(`/documents/snapshots`, {
      WorkspaceId: workspaceId,
      DocumentId: documentId,
      DocumentName: documentName,
      SnapshotName: snapshotName,
      Forker: forker,
    }),
  lock: (workspaceId: string, documentId: string, newName: string) =>
    requests.post<void>(`/documents/lock`, {
      WorkspaceId: workspaceId,
      DocumentId: documentId,
      NewName: newName,
    }),
};

const agent = {
  Account,
  Workspaces,
  Documents,
};

export default agent;

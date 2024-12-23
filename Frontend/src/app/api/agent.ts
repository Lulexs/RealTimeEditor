import { notifications } from "@mantine/notifications";
import axios, { AxiosError, AxiosResponse } from "axios";
import {
  User,
  UserInWorkspace,
  UserLoginValues,
  UserRegisterValues,
} from "../models/User";
import Workspace, { PermissionLevel } from "../models/Workspace";

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
  changeName: (
    workspaceId: string,
    ownerUsername: string,
    userUsername: string,
    newName: string
  ) =>
    requests.put<void>(`/workspaces`, {
      WorkspaceId: workspaceId,
      OwnerUsername: ownerUsername,
      UserUsername: userUsername,
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
    username: string,
    newPermLevel: PermissionLevel,
    performer: string
  ) =>
    requests.put<void>(
      `/workspaces/users/${username}/${newPermLevel}/${performer}`,
      {}
    ),
};

const agent = {
  Account,
  Workspaces,
};

export default agent;

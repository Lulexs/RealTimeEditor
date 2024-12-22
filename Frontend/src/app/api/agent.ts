import { notifications } from "@mantine/notifications";
import axios, { AxiosError, AxiosResponse } from "axios";
import { User, UserLoginValues, UserRegisterValues } from "../models/User";
import Workspace from "../models/Workspace";

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
  list: () => requests.get<Workspace[]>("/workspaces"),
  create: (name: string, ownername: string) =>
    requests.post<Workspace>("/workspaces", {
      Name: name,
      OwnerName: ownername,
    }),
  delete: (ownername: string, workspaceid: string) =>
    requests.del<void>(`/workspaces/${ownername}/${workspaceid}`),
};

const agent = {
  Account,
  Workspaces,
};

export default agent;

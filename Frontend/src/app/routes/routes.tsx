import { createBrowserRouter, RouteObject } from "react-router";
import HomePage from "../../features/HomePage/HomePage";
import Login from "../../features/Auth/Login";
import Register from "../../features/Auth/Register";

export const routes: RouteObject[] = [
  {
    path: "",
    element: <HomePage />,
  },
  {
    path: "/login",
    element: <Login />,
  },
  {
    path: "/register",
    element: <Register />,
  },
];

export const router = createBrowserRouter(routes);

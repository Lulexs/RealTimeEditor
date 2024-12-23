import { createBrowserRouter, RouteObject } from "react-router";
import HomePage from "../../features/HomePage/HomePage";
import Login from "../../features/Auth/Login";
import Register from "../../features/Auth/Register";
import CollaborativeEditor from "../../features/CollaborativeEditor/CollaborativeEditor";

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
  {
    path: "/nodocument",
    element: <CollaborativeEditor />,
  },
  {
    path: "/*",
    element: <HomePage />,
  },
];

export const router = createBrowserRouter(routes);

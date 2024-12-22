import { MantineProvider } from "@mantine/core";
import { ContextMenuProvider } from "mantine-contextmenu";
import { Notifications } from "@mantine/notifications";

import { RouterProvider } from "react-router";
import { router } from "./app/routes/routes";

import "./index.css";
import "@mantine/core/styles.css";
import "@mantine/notifications/styles.css";
import "./layout.css";
import "mantine-contextmenu/styles.layer.css";
import "@mantine/core/styles.layer.css";

function App() {
  return (
    <MantineProvider>
      <Notifications />
      <ContextMenuProvider>
        <RouterProvider router={router} />
      </ContextMenuProvider>
    </MantineProvider>
  );
}

export default App;

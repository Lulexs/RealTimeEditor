import { MantineProvider } from "@mantine/core";
import Editor from "./Editor/Editor";
import "@mantine/core/styles.css";
import "@mantine/notifications/styles.css";
import { Notifications } from "@mantine/notifications";

function App() {
  return (
    <MantineProvider>
      <Notifications />
      <Editor />
    </MantineProvider>
  );
}

export default App;

import { MantineProvider } from "@mantine/core";
import Editor from "./Editor/Editor";
import "@mantine/core/styles.css";
import "@mantine/notifications/styles.css";
import "./index.css";
import { Notifications } from "@mantine/notifications";
import HomePage from "./HomePage/HomePage";

function App() {
  return (
    <MantineProvider>
      <Notifications />
      <HomePage />
      {/* <Editor /> */}
    </MantineProvider>
  );
}

export default App;

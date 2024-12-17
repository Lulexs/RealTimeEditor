import { MantineProvider } from "@mantine/core";
import Editor from "./Editor/Editor";
import "@mantine/core/styles.css";

function App() {
  return (
    <MantineProvider>
      <Editor />
    </MantineProvider>
  );
}

export default App;

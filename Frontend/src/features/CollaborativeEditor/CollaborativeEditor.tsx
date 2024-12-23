import { useEffect, useState } from "react";
import { Box, ActionIcon, Transition, Paper } from "@mantine/core";
import { IconChevronRight, IconChevronLeft } from "@tabler/icons-react";
import classes from "./CollaborativeEditor.module.css";
import Editor from "../Editor/Editor";
import WorkspacePanel from "../ControlPanel/ControlPanel";
import { useStore } from "../../app/stores/store";
import { observer } from "mobx-react-lite";
import { useNavigate } from "react-router";

export default observer(function CollaborativeEditor() {
  const [isPanelVisible, setPanelVisible] = useState(true);
  const { documentStore, userStore } = useStore();
  const navigate = useNavigate();

  const togglePanel = () => {
    setPanelVisible(!isPanelVisible);
  };

  useEffect(() => {
    if (userStore.user == null) {
      navigate("/");
    }
  }, []);

  return (
    <Box className={classes.container}>
      <Transition
        mounted={isPanelVisible}
        transition="slide-right"
        duration={100}
        timingFunction="ease"
      >
        {() => (
          <Paper
            className={classes.workspacePanelContainer}
            style={classes}
            shadow="sm"
          >
            <WorkspacePanel />
          </Paper>
        )}
      </Transition>

      <Box className={classes.editorContainer}>
        {documentStore.selectedDocument !== null ? (
          <Editor />
        ) : (
          <Box className={classes.noDocumentSelected}>
            Select a document from the workspace to start editing
          </Box>
        )}

        <ActionIcon
          variant="filled"
          className={classes.toggleButton}
          onClick={() => togglePanel()}
          size="lg"
        >
          {isPanelVisible ? (
            <IconChevronLeft size={20} />
          ) : (
            <IconChevronRight size={20} />
          )}
        </ActionIcon>
      </Box>
    </Box>
  );
});

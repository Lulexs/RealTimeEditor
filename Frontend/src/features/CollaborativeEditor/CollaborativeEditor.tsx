import { useState } from "react";
import { Box, ActionIcon, Transition, Paper } from "@mantine/core";
import { IconChevronRight, IconChevronLeft } from "@tabler/icons-react";
import classes from "./CollaborativeEditor.module.css";
import Editor from "../Editor/Editor";
import WorkspacePanel from "../ControlPanel/ControlPanel";

interface Document {
  id: string;
  name: string;
  workspaceId: string;
  createdAt: string;
  ownerUsername: string;
  documentName: string;
  snapshots: any[];
}

interface EditorProps {
  workspaceId?: string;
  documentId?: string;
  createdAt?: string;
  ownerUsername?: string;
  documentName?: string;
  snapshots?: any[];
  userProfile: {
    name: string;
    color: string;
  };
  activeUsers: any[];
  defaultProps: any;
}

export function CollaborativeEditor() {
  const [isPanelVisible, setPanelVisible] = useState(true);
  const [selectedDocument, setSelectedDocument] = useState<string | null>("1");

  const togglePanel = () => {
    console.log("Here");
    setPanelVisible(!isPanelVisible);
  };

  const handleDocumentSelect = (workspaceId: number, documentId: number) => {
    console.log(
      `Selected document ${documentId} from workspace ${workspaceId}`
    );
  };

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
        {selectedDocument ? (
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
}

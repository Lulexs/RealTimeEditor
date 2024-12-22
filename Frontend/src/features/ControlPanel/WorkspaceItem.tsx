import { Box, Text, Group, UnstyledButton } from "@mantine/core";
import {
  IconChevronRight,
  IconChevronDown,
  IconFileText,
  IconFolder,
} from "@tabler/icons-react";
import { useContextMenu } from "mantine-contextmenu";
import styles from "./ControlPanel.module.css";
import { useState } from "react";
import {
  workspaceContextMenu,
  documentContextMenu,
} from "./ContextMenuActions";

interface Document {
  id: number;
  name: string;
}

interface Workspace {
  id: number;
  name: string;
  dateCreated: string;
  documents: Document[];
}

interface WorkspaceItemProps {
  workspace: Workspace;
}

const WorkspaceItem = ({ workspace }: WorkspaceItemProps) => {
  const { showContextMenu } = useContextMenu();
  const [expanded, setExpanded] = useState(false);

  return (
    <Box className={styles.workspaceItem}>
      <UnstyledButton
        onClick={() => setExpanded((p) => !p)}
        onContextMenu={showContextMenu(workspaceContextMenu, {
          styles: {
            item: {
              padding: "8px 12px",
              margin: "4px 0",
              borderRadius: "4px",
              cursor: "pointer",
              transition: "background 0.3s ease",
              color: "#333",
            },
          },
        })}
        className={styles.workspaceButton}
      >
        <Group>
          {expanded ? <IconChevronDown /> : <IconChevronRight />}
          <IconFolder />
          <Box>
            <Text size="sm" fw={500}>
              {workspace.name}
            </Text>
            <Text size="xs" c="dimmed">
              Created: {new Date(workspace.dateCreated).toLocaleDateString()}
            </Text>
          </Box>
        </Group>
      </UnstyledButton>

      {expanded && (
        <Box className={styles.documentList}>
          {workspace.documents.map((doc) => (
            <UnstyledButton
              key={doc.id}
              className={styles.documentButton}
              onContextMenu={showContextMenu(documentContextMenu(), {
                styles: {
                  item: {
                    padding: "8px 12px",
                    margin: "4px 0",
                    borderRadius: "4px",
                    cursor: "pointer",
                    transition: "background 0.3s ease",
                    color: "red",
                  },
                },
              })}
            >
              <Group>
                <IconFileText />
                <Text size="sm">{doc.name}</Text>
              </Group>
            </UnstyledButton>
          ))}
        </Box>
      )}
    </Box>
  );
};

export default WorkspaceItem;

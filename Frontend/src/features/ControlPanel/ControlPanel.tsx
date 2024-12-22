import { useState } from "react";
import { Box, Paper, Text, Group, UnstyledButton, Avatar } from "@mantine/core";
import {
  IconChevronRight,
  IconChevronDown,
  IconFileText,
  IconFolder,
  IconUsers,
  IconLink,
  IconPencil,
  IconTrash,
  IconPlus,
  IconLogout,
} from "@tabler/icons-react";
import { useContextMenu } from "mantine-contextmenu";
import styles from "./ControlPanel.module.css";

interface Document {
  id: number;
  name: string;
}

interface Workspace {
  id: number;
  name: string;
  dateCreated: string;
  expanded: boolean;
  documents: Document[];
}

interface WorkspaceItemProps {
  workspace: Workspace;
  onToggle: (id: number) => void;
  onWorkspaceAction: (id: number, action: string) => void;
  onDocumentAction: (
    workspaceId: number,
    documentId: number,
    action: string
  ) => void;
}

const WorkspaceItem = ({
  workspace,
  onToggle,
  onWorkspaceAction,
  onDocumentAction,
}: WorkspaceItemProps) => {
  const { showContextMenu } = useContextMenu();

  const workspaceContextMenu = [
    {
      key: "new",
      icon: <IconPlus size={16} />,
      title: "New Document",
      onClick: () => onWorkspaceAction(workspace.id, "new"),
    },
    {
      key: "invite",
      icon: <IconLink size={16} />,
      title: "Invite Code",
      onClick: () => onWorkspaceAction(workspace.id, "invite"),
    },
    {
      key: "rename",
      icon: <IconPencil size={16} />,
      title: "Change Name",
      onClick: () => onWorkspaceAction(workspace.id, "rename"),
    },
    {
      key: "users",
      icon: <IconUsers size={16} />,
      title: "Manage Users",
      onClick: () => onWorkspaceAction(workspace.id, "users"),
    },
    {
      key: "delete",
      icon: <IconTrash size={16} />,
      title: "Delete",
      style: { color: "red" },
      onClick: () => onWorkspaceAction(workspace.id, "delete"),
    },
  ];

  const documentContextMenu = (docId: number) => [
    {
      key: "delete",
      icon: <IconTrash size={16} />,
      title: "Delete",
      color: "red",
      onClick: () => onDocumentAction(workspace.id, docId, "delete"),
    },
  ];

  return (
    <Box className={styles.workspaceItem}>
      <UnstyledButton
        onClick={() => onToggle(workspace.id)}
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
          {workspace.expanded ? <IconChevronDown /> : <IconChevronRight />}
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

      {workspace.expanded && (
        <Box className={styles.documentList}>
          {workspace.documents.map((doc) => (
            <UnstyledButton
              key={doc.id}
              className={styles.documentButton}
              onContextMenu={showContextMenu(documentContextMenu(doc.id), {
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

export default function ControlPanel() {
  const [workspaces, setWorkspaces] = useState<Workspace[]>([
    {
      id: 1,
      name: "Project Alpha",
      dateCreated: "2024-12-20",
      expanded: false,
      documents: [
        { id: 1, name: "Document 1.txt" },
        { id: 2, name: "Document 2.txt" },
      ],
    },
    {
      id: 2,
      name: "Project Beta",
      dateCreated: "2024-12-21",
      expanded: false,
      documents: [
        { id: 3, name: "Requirements.txt" },
        { id: 4, name: "Notes.txt" },
      ],
    },
  ]);

  const toggleWorkspace = (workspaceId: number) => {
    setWorkspaces(
      workspaces.map((ws) =>
        ws.id === workspaceId ? { ...ws, expanded: !ws.expanded } : ws
      )
    );
  };

  const handleWorkspaceAction = (workspaceId: number, action: string) => {
    console.log(`Workspace ${workspaceId} action: ${action}`);
  };

  const handleDocumentAction = (
    workspaceId: number,
    documentId: number,
    action: string
  ) => {
    console.log(
      `Document ${documentId} in workspace ${workspaceId} action: ${action}`
    );
  };

  const { showContextMenu: showControlMenu } = useContextMenu();
  const { showContextMenu: showUserMenu } = useContextMenu();

  const controlMenuItems = [
    {
      key: "new-workspace",
      icon: <IconPlus size={16} />,
      title: "New Workspace",
      onClick: () => console.log("Create new workspace"),
    },
  ];

  const userMenuItems = [
    {
      key: "logout",
      icon: <IconLogout size={16} />,
      title: "Logout",
      onClick: () => console.log("User logged out"),
    },
  ];

  return (
    <Paper shadow="sm" p="md" className={styles.container}>
      <Text
        size="lg"
        fw={500}
        className={styles.title}
        onContextMenu={showControlMenu(controlMenuItems, {
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
      >
        Workspaces
      </Text>
      <Box>
        {workspaces.map((workspace) => (
          <WorkspaceItem
            key={workspace.id}
            workspace={workspace}
            onToggle={toggleWorkspace}
            onWorkspaceAction={handleWorkspaceAction}
            onDocumentAction={handleDocumentAction}
          />
        ))}
      </Box>

      <Box mt="auto" className={styles.userSection}>
        <Group
          onClick={showUserMenu(userMenuItems, {
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
          className={styles.userContainer}
        >
          <Avatar
            radius="xl"
            size={40}
            src="https://raw.githubusercontent.com/mantinedev/mantine/master/.demo/avatars/avatar-9.png"
          />
          <Text size="sm" fw={500}>
            Username
          </Text>
        </Group>
      </Box>
    </Paper>
  );
}

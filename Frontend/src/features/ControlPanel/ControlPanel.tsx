import { Box, Paper, Text, Group, Avatar } from "@mantine/core";
import { useContextMenu } from "mantine-contextmenu";
import WorkspaceItem from "./WorkspaceItem";
import styles from "./ControlPanel.module.css";
import { IconPlus, IconLogout, IconJoinBevel } from "@tabler/icons-react";
import { useStore } from "../../app/stores/store";

const workspaces = [
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
];

export default function ControlPanel() {
  const { showContextMenu } = useContextMenu();
  const { userStore } = useStore();

  const controlMenuItems = [
    {
      key: "new-workspace",
      icon: <IconPlus size={16} />,
      title: "New Workspace",
      onClick: () => console.log("Create new workspace"),
    },
    {
      key: "join-workspace",
      icon: <IconJoinBevel size={16} />,
      title: "Join Workspace",
      onClick: () => console.log("Join workspace"),
    },
  ];

  const userMenuItems = [
    {
      key: "logout",
      icon: <IconLogout size={16} />,
      title: "Logout",
      onClick: () => {
        userStore.logout();
      },
    },
  ];

  return (
    <Paper shadow="sm" p="md" className={styles.container}>
      <Text
        size="lg"
        fw={500}
        className={styles.title}
        onContextMenu={showContextMenu(controlMenuItems, {
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
          <WorkspaceItem key={workspace.id} workspace={workspace} />
        ))}
      </Box>

      <Box mt="auto" className={styles.userSection}>
        <Group
          onClick={showContextMenu(userMenuItems, {
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
          <Avatar radius="xl" size={40} src={userStore.user?.avatar} />
          <Text size="sm" fw={500}>
            {userStore.user?.username}
          </Text>
        </Group>
      </Box>
    </Paper>
  );
}

import { Box, Paper, Text, Group, Avatar, Loader, Center } from "@mantine/core";
import { useContextMenu } from "mantine-contextmenu";
import WorkspaceItem from "./WorkspaceItem";
import styles from "./ControlPanel.module.css";
import { IconPlus, IconLogout, IconJoinBevel } from "@tabler/icons-react";
import { useStore } from "../../app/stores/store";
import { observer } from "mobx-react-lite";
import CreateWorkspaceDialog from "./Dialogs/CreateWorkspaceDialog";
import { useDisclosure } from "@mantine/hooks";
import JoinWorkspaceDialog from "./Dialogs/JoinWorkspaceDialog";

export default observer(function ControlPanel() {
  const { showContextMenu } = useContextMenu();
  const { userStore, workspaceStore } = useStore();

  const [
    createWorkspaceDialogOpened,
    { toggle: toggleCreateWorkspaceDialog, close: closeCreateWorkspaceDialog },
  ] = useDisclosure(false);

  const [
    joinWorkspaceDialogOpened,
    { toggle: toggleJoinWorkspaceDialog, close: closeJoinWorkspaceDialog },
  ] = useDisclosure(false);

  const controlMenuItems = [
    {
      key: "new-workspace",
      icon: <IconPlus size={16} />,
      title: "New Workspace",
      onClick: () => {
        toggleCreateWorkspaceDialog();
      },
    },
    {
      key: "join-workspace",
      icon: <IconJoinBevel size={16} />,
      title: "Join Workspace",
      onClick: () => {
        toggleJoinWorkspaceDialog();
      },
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
    <>
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
          {!workspaceStore.workspaces ? (
            <Center my="xl">
              <Loader size={30} />
            </Center>
          ) : (
            [...workspaceStore.workspaces.values()].map((workspace) => (
              <WorkspaceItem
                key={workspace.workspaceId}
                workspace={workspace}
              />
            ))
          )}
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
      <CreateWorkspaceDialog
        opened={createWorkspaceDialogOpened}
        onClose={closeCreateWorkspaceDialog}
        onCreate={(name: string) => {
          workspaceStore.createWorkspace(
            name,
            userStore.user?.username ?? "InvalidUsername"
          );
        }}
      />
      <JoinWorkspaceDialog
        opened={joinWorkspaceDialogOpened}
        onClose={closeJoinWorkspaceDialog}
        onJoin={(joinCode) => {
          workspaceStore.joinWorkspace(
            userStore.user?.username ?? "InvalidUsername",
            joinCode
          );
        }}
      />
    </>
  );
});

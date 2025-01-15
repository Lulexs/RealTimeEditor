import { Box, Text, Group, UnstyledButton } from "@mantine/core";
import {
  IconChevronRight,
  IconChevronDown,
  IconFolder,
  IconLink,
  IconPencil,
  IconPlus,
  IconRefresh,
  IconTrash,
  IconUsers,
} from "@tabler/icons-react";
import { useContextMenu } from "mantine-contextmenu";
import styles from "./ControlPanel.module.css";
import { useRef, useState } from "react";
import Workspace, { PermissionLevel } from "../../app/models/Workspace";
import { useStore } from "../../app/stores/store";
import GenerateCodeDialog from "./Dialogs/GenerateCodeDialog";
import { useDisclosure } from "@mantine/hooks";
import ChangeWorkspaceNameDialog from "./Dialogs/ChangeWorkspaceNameDialog";
import ManageUsersDialog from "./Dialogs/ManageUsersDialog";
import { observer } from "mobx-react-lite";
import DocumentItem from "./DocumentItem";
import CreateDocumentDialog from "./Dialogs/CreateDocumentDialog";

interface WorkspaceItemProps {
  workspace: Workspace;
}

const WorkspaceItem = observer(({ workspace }: WorkspaceItemProps) => {
  const { showContextMenu } = useContextMenu();
  const [expanded, setExpanded] = useState(false);
  const { workspaceStore, userStore, documentStore } = useStore();
  const loadedRef = useRef<boolean>(false);

  const [
    generateCodeDialogOpened,
    { toggle: togleGenerateCodeDialog, close: closeGenerateCodeDialog },
  ] = useDisclosure(false);

  const [
    changeWorkspaceNameDialogOpened,
    {
      toggle: toggleChangeWorkspaceNameDialog,
      close: closeChangeWorkspaceNameDialog,
    },
  ] = useDisclosure(false);

  const [
    manageUsersDialogOpened,
    { toggle: toggleManageUsersDialog, close: closeManageUsersDialog },
  ] = useDisclosure(false);

  const [
    createDocumentDialogOpened,
    { toggle: toggleCreateDocumentDialog, close: closeCreateDocumentDialog },
  ] = useDisclosure(false);

  const workspaceContextMenu = [
    {
      key: "new",
      icon: <IconPlus size={16} />,
      title: "New Document",
      onClick: () => {
        toggleCreateDocumentDialog();
      },
      requiredPermission: PermissionLevel.Admin,
    },
    {
      key: "invite",
      icon: <IconLink size={16} />,
      title: "Invite Code",
      onClick: () => {
        togleGenerateCodeDialog();
      },
      requiredPermission: PermissionLevel.Admin,
    },
    {
      key: "Refresh",
      icon: <IconRefresh size={16} />,
      title: "Refresh",
      onClick: () => {
        workspaceStore.refresh(workspace.ownerUsername, workspace.workspaceId);
      },
      requiredPermission: PermissionLevel.ViewOnly,
    },
    {
      key: "rename",
      icon: <IconPencil size={16} />,
      title: "Change Name",
      onClick: () => {
        toggleChangeWorkspaceNameDialog();
      },
      requiredPermission: PermissionLevel.Admin,
    },
    {
      key: "users",
      icon: <IconUsers size={16} />,
      title: "Manage Users",
      onClick: () => {
        workspaceStore.users(workspace.workspaceId);
        toggleManageUsersDialog();
      },
      requiredPermission: PermissionLevel.Admin,
    },
    {
      key: "delete",
      icon: <IconTrash size={16} />,
      title: "Delete",
      style: { color: "red" },
      onClick: () => {
        workspaceStore.delete(workspace.workspaceId, workspace.ownerUsername);
      },
      requiredPermission: PermissionLevel.Owner,
    },
  ];

  return (
    <>
      <Box className={styles.workspaceItem}>
        <UnstyledButton
          onClick={() => {
            if (loadedRef.current === false) {
              documentStore.loadDocuments(workspace.workspaceId);
              loadedRef.current = true;
            }
            setExpanded((p) => !p);
          }}
          onContextMenu={showContextMenu(
            workspaceContextMenu
              .filter(
                (a) =>
                  a.requiredPermission <=
                  workspaceStore.workspaces!.get(workspace.workspaceId)!
                    .permission
              )
              .map(({ requiredPermission, ...rest }) => rest),
            {
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
            }
          )}
          className={styles.workspaceButton}
        >
          <Group>
            {expanded ? <IconChevronDown /> : <IconChevronRight />}
            <IconFolder />
            <Box>
              <Text size="sm" fw={500}>
                {workspace.workspaceName}
              </Text>
              <Text size="xs" c="dimmed">
                Created: {new Date(workspace.createdAt).toLocaleDateString()}
              </Text>
            </Box>
          </Group>
        </UnstyledButton>

        {expanded && (
          <Box className={styles.documentList}>
            {[
              ...documentStore.documents.get(workspace.workspaceId)!.values(),
            ].map((doc) => (
              <DocumentItem
                key={doc.documentId}
                document={doc}
                permissionLevel={workspace.permission}
              />
            ))}
          </Box>
        )}
      </Box>
      <GenerateCodeDialog
        workspaceId={workspace.workspaceId}
        opened={generateCodeDialogOpened}
        onClose={closeGenerateCodeDialog}
      />
      <ChangeWorkspaceNameDialog
        opened={changeWorkspaceNameDialogOpened}
        onClose={closeChangeWorkspaceNameDialog}
        onRename={async (newName: string) => {
          return await workspaceStore.changeName(
            workspace.workspaceId,
            workspace.ownerUsername,
            userStore.user?.username ?? "InvalidUsername",
            newName
          );
        }}
      />
      <ManageUsersDialog
        opened={manageUsersDialogOpened}
        onClose={closeManageUsersDialog}
        onKickUser={(username) => {
          workspaceStore.kick(
            workspace.workspaceId,
            username,
            userStore.user?.username ?? "InvalidUser"
          );
        }}
        onPermissionChange={(username, newPerm) => {
          workspaceStore.permChange(
            username,
            newPerm,
            userStore.user?.username ?? "InvalidUser"
          );
        }}
      />
      <CreateDocumentDialog
        opened={createDocumentDialogOpened}
        onClose={closeCreateDocumentDialog}
        onCreate={(docName) => {
          documentStore.newDocument(
            workspace.workspaceId,
            docName,
            userStore.user?.username ?? "InvalidUser"
          );
        }}
      />
    </>
  );
});

export default WorkspaceItem;

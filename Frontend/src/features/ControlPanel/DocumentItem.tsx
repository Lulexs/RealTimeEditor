import { Text, Group, UnstyledButton } from "@mantine/core";
import { IconFileText, IconPencil, IconTrash } from "@tabler/icons-react";
import { useContextMenu } from "mantine-contextmenu";
import styles from "./ControlPanel.module.css";
import { Document } from "../../app/models/Document";
import { PermissionLevel } from "../../app/models/Workspace";
import { useDisclosure } from "@mantine/hooks";
import ChangeDocumentNameDialog from "./Dialogs/ChangeDocumentNameDialog";
import { useStore } from "../../app/stores/store";
import { observer } from "mobx-react-lite";

interface DocumentItemProps {
  document: Document;
  permissionLevel: PermissionLevel;
}

const DocumentItem = observer(
  ({ document, permissionLevel }: DocumentItemProps) => {
    const { showContextMenu } = useContextMenu();
    const { documentStore, userStore } = useStore();

    const [
      changeDocumentNameDialogOpened,
      {
        toggle: toggleChangeDocumentNameDialog,
        close: closeChangeDocumentNameDialog,
      },
    ] = useDisclosure(false);

    const documentContextMenu = [
      {
        key: "rename",
        icon: <IconPencil size={16} />,
        title: "Rename",
        onClick: () => {
          toggleChangeDocumentNameDialog();
        },
        requiredPermission: PermissionLevel.Edit,
      },
      {
        key: "delete",
        icon: <IconTrash size={16} />,
        title: "Delete",
        style: { color: "red" },
        onClick: () => {
          documentStore.delete(
            document.workspaceId,
            document.documentId,
            userStore.user?.username ?? "InvalidUsername"
          );
        },
        requiredPermission: PermissionLevel.Admin,
      },
    ];

    return (
      <>
        <UnstyledButton
          key={document.documentId}
          className={styles.documentButton}
          onClick={() => {
            documentStore.selectDocument(document);
          }}
          onContextMenu={showContextMenu(
            documentContextMenu
              .filter((item) => item.requiredPermission <= permissionLevel)
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
        >
          <Group>
            <IconFileText />
            <Text size="sm">{document.documentName}</Text>
          </Group>
        </UnstyledButton>
        <ChangeDocumentNameDialog
          opened={changeDocumentNameDialogOpened}
          onClose={closeChangeDocumentNameDialog}
          onRename={(newName: string) => {
            documentStore.changeDocumentName(
              document.workspaceId,
              document.documentId,
              newName
            );
          }}
          currentName={document.documentName}
        />
      </>
    );
  }
);

export default DocumentItem;

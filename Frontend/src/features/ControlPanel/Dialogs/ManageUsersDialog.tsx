import {
  Modal,
  Button,
  Group,
  Stack,
  Checkbox,
  Text,
  ActionIcon,
  Center,
  Loader,
} from "@mantine/core";
import { IconX } from "@tabler/icons-react";
import { UserInWorkspace } from "../../../app/models/User";
import { PermissionLevel } from "../../../app/models/Workspace";
import { observer } from "mobx-react-lite";
import { useStore } from "../../../app/stores/store";

interface UserPermissionsDialogProps {
  opened: boolean;
  onClose: () => void;
  onPermissionChange: (
    username: string,
    newPermission: PermissionLevel
  ) => void;
  onKickUser: (username: string) => void;
}

export default observer(function ManageUsersDialog({
  opened,
  onClose,
  onPermissionChange,
  onKickUser,
}: UserPermissionsDialogProps) {
  const { workspaceStore } = useStore();

  const handlePermissionChange = (
    username: string,
    newPermission: PermissionLevel
  ) => {
    onPermissionChange(username, newPermission);
  };

  const renderPermissionCheckboxes = (user: UserInWorkspace) => {
    const permissions = [
      PermissionLevel.ViewOnly,
      PermissionLevel.Edit,
      PermissionLevel.Admin,
    ];
    return permissions.map((permission) => (
      <Checkbox
        key={`${user.username}-${permission}`}
        checked={user.permission === permission}
        onChange={() => handlePermissionChange(user.username, permission)}
        disabled={
          permission === PermissionLevel.Owner ||
          user.permission === PermissionLevel.Owner
        }
        label={PermissionLevel[permission]}
        styles={{
          root: {
            display: "inline-flex",
            marginRight: "1rem",
          },
        }}
      />
    ));
  };

  const renderContent = () => {
    if (workspaceStore.usersInWorkspace === null) {
      return (
        <Center my="xl">
          <Loader size="md" variant="dots" />
        </Center>
      );
    }

    if (workspaceStore.usersInWorkspace.length === 0) {
      return (
        <Center my="xl">
          <Text c="dimmed">No users in this workspace</Text>
        </Center>
      );
    }

    return workspaceStore.usersInWorkspace.map((user) => (
      <Group key={user.username} align="center" style={{ width: "100%" }}>
        <Group style={{ flex: 1, justifyContent: "flex-start", minWidth: 0 }}>
          <Text
            fw={500}
            size="sm"
            style={{
              width: "150px",
              minWidth: "150px",
              overflow: "hidden",
              textOverflow: "ellipsis",
              whiteSpace: "nowrap",
            }}
          >
            {user.username}
          </Text>
          <Group gap="xs" style={{ flex: 1, justifyContent: "flex-start" }}>
            {renderPermissionCheckboxes(user)}
          </Group>
        </Group>
        {user.permission !== PermissionLevel.Owner && (
          <ActionIcon
            color="red"
            variant="subtle"
            onClick={() => onKickUser(user.username)}
            title="Kick user"
            style={{ flexShrink: 0 }}
          >
            <IconX size={16} />
          </ActionIcon>
        )}
      </Group>
    ));
  };

  return (
    <Modal
      opened={opened}
      onClose={onClose}
      title="Manage Workspace Users"
      size="lg"
    >
      <Stack gap="lg">
        {renderContent()}
        <Group mt="md" justify="flex-end">
          <Button onClick={onClose}>Close</Button>
        </Group>
      </Stack>
    </Modal>
  );
});

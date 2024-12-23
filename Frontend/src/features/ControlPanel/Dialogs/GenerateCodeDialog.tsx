import { Modal, Button, Checkbox, Stack, Group } from "@mantine/core";
import { notifications } from "@mantine/notifications";
import { useState } from "react";

interface GenerateCodeDialogProps {
  workspaceId: string;
  opened: boolean;
  onClose: () => void;
}

export default function GenerateCodeDialog({
  workspaceId,
  opened,
  onClose,
}: GenerateCodeDialogProps) {
  const [permissions, setPermissions] = useState<number>(0);

  const handleCheckboxChange = (permission: number) => {
    setPermissions(permission);
  };

  const handleGenerate = () => {
    notifications.show({
      title: "Invite code",
      message: `${workspaceId}\\${permissions}`,
    });
    onClose();
  };

  return (
    <Modal
      opened={opened}
      onClose={onClose}
      title="Generate Workspace Access Code"
      size="sm"
    >
      <Stack gap="md">
        <Checkbox
          label="Read-Only Access"
          checked={permissions == 0}
          onChange={() => handleCheckboxChange(0)}
        />
        <Checkbox
          label="Edit Access"
          checked={permissions == 1}
          onChange={() => handleCheckboxChange(1)}
        />
        <Checkbox
          label="Admin Access"
          checked={permissions == 2}
          onChange={() => handleCheckboxChange(2)}
        />

        <Group flex={1} mt="md">
          <Button onClick={handleGenerate}>Generate Code</Button>
        </Group>
      </Stack>
    </Modal>
  );
}

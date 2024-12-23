import { Modal, Button, TextInput, Stack, Group } from "@mantine/core";
import { useState } from "react";

interface CreateWorkspaceDialogProps {
  opened: boolean;
  onClose: () => void;
  onCreate: (workspaceName: string) => void;
}

export default function CreateWorkspaceDialog({
  opened,
  onClose,
  onCreate,
}: CreateWorkspaceDialogProps) {
  const [workspaceName, setWorkspaceName] = useState("");
  const [error, setError] = useState("");

  const handleCreate = () => {
    if (!workspaceName.trim()) {
      setError("Workspace name cannot be empty");
      return;
    }
    onCreate(workspaceName.trim());
    setWorkspaceName("");
    setError("");
    onClose();
  };

  const handleChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    setWorkspaceName(event.currentTarget.value);
    if (error) setError("");
  };

  const handleClose = () => {
    setWorkspaceName("");
    setError("");
    onClose();
  };

  return (
    <Modal
      opened={opened}
      onClose={handleClose}
      title="Create New Workspace"
      size="sm"
    >
      <Stack gap="md">
        <TextInput
          label="Workspace Name"
          placeholder="Enter workspace name"
          value={workspaceName}
          onChange={handleChange}
          error={error}
          data-autofocus
        />

        <Group mt="md">
          <Button onClick={handleCreate} disabled={!workspaceName.trim()}>
            Create Workspace
          </Button>
        </Group>
      </Stack>
    </Modal>
  );
}

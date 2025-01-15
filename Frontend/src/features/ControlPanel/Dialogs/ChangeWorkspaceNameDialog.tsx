import { Modal, Button, TextInput, Stack, Group } from "@mantine/core";
import { useState } from "react";

interface RenameDialogProps {
  opened: boolean;
  onClose: () => void;
  onRename: (newName: string) => Promise<boolean>;
  currentName?: string;
}

export default function ChangeWorkspaceNameDialog({
  opened,
  onClose,
  onRename,
  currentName = "",
}: RenameDialogProps) {
  const [newName, setNewName] = useState(currentName);
  const [error, setError] = useState("");

  const handleRename = async () => {
    if (!newName.trim()) {
      setError("Workspace name cannot be empty");
      return;
    }
    const res = await onRename(newName.trim());
    if (res) {
      setNewName("");
      setError("");
      onClose();
    }
  };

  const handleChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    setNewName(event.currentTarget.value);
    if (error) setError("");
  };

  return (
    <Modal opened={opened} onClose={onClose} title="Rename Workspace" size="sm">
      <Stack gap="md">
        <TextInput
          label="New Name"
          placeholder="Enter new workspace name"
          value={newName}
          onChange={handleChange}
          error={error}
          data-autofocus
        />

        <Group mt="md">
          <Button
            onClick={handleRename}
            disabled={!newName.trim() || newName === currentName}
          >
            Change Name
          </Button>
        </Group>
      </Stack>
    </Modal>
  );
}

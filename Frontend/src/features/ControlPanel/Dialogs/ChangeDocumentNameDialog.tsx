import { Modal, Button, TextInput, Stack, Group } from "@mantine/core";
import { useState } from "react";

interface RenameDocumentDialogProps {
  opened: boolean;
  onClose: () => void;
  onRename: (newName: string) => Promise<boolean>;
  currentName: string;
}

export default function RenameDocumentDialog({
  opened,
  onClose,
  onRename,
  currentName = "",
}: RenameDocumentDialogProps) {
  const [newName, setNewName] = useState(currentName);
  const [error, setError] = useState("");

  const handleRename = async () => {
    if (!newName.trim()) {
      setError("Document name cannot be empty");
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

  const handleClose = () => {
    setNewName(currentName);
    setError("");
    onClose();
  };

  return (
    <Modal
      opened={opened}
      onClose={handleClose}
      title="Rename Document"
      size="sm"
    >
      <Stack gap="md">
        <TextInput
          label="New Name"
          placeholder="Enter new document name"
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

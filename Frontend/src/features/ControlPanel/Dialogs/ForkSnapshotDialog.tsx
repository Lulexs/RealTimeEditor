import { Modal, Button, TextInput, Stack, Group } from "@mantine/core";
import { useState } from "react";

interface ForkSnapshotDialogProps {
  opened: boolean;
  onClose: () => void;
  onFork: (documentName: string) => void;
}

export default function ForkSnapshotDialog({
  opened,
  onClose,
  onFork,
}: ForkSnapshotDialogProps) {
  const [documentName, setDocumentName] = useState("");
  const [error, setError] = useState("");

  const handleFork = () => {
    if (!documentName.trim()) {
      setError("Document name cannot be empty");
      return;
    }
    onFork(documentName.trim());
    setDocumentName("");
    setError("");
    onClose();
  };

  const handleChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    setDocumentName(event.currentTarget.value);
    if (error) setError("");
  };

  const handleClose = () => {
    setDocumentName("");
    setError("");
    onClose();
  };

  return (
    <Modal
      opened={opened}
      onClose={handleClose}
      title="Fork Document"
      size="sm"
    >
      <Stack gap="md">
        <TextInput
          label="New Document Name"
          placeholder="Enter name for forked document"
          value={documentName}
          onChange={handleChange}
          error={error}
          data-autofocus
        />

        <Group mt="md">
          <Button onClick={handleFork} disabled={!documentName.trim()}>
            Fork Document
          </Button>
        </Group>
      </Stack>
    </Modal>
  );
}

import { Modal, Button, TextInput, Stack, Group } from "@mantine/core";
import { useState } from "react";

interface CreateDocumentDialogProps {
  opened: boolean;
  onClose: () => void;
  onCreate: (documentName: string) => void;
}

export default function CreateDocumentDialog({
  opened,
  onClose,
  onCreate,
}: CreateDocumentDialogProps) {
  const [documentName, setDocumentName] = useState("");
  const [error, setError] = useState("");

  const handleCreate = () => {
    if (!documentName.trim()) {
      setError("Document name cannot be empty");
      return;
    }
    onCreate(documentName.trim());
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
      title="Create New Document"
      size="sm"
    >
      <Stack gap="md">
        <TextInput
          label="Document Name"
          placeholder="Enter document name"
          value={documentName}
          onChange={handleChange}
          error={error}
          data-autofocus
        />

        <Group mt="md">
          <Button onClick={handleCreate} disabled={!documentName.trim()}>
            Create Document
          </Button>
        </Group>
      </Stack>
    </Modal>
  );
}

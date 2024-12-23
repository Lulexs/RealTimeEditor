import { Modal, Button, TextInput, Stack, Group } from "@mantine/core";
import { useState } from "react";

interface JoinWorkspaceDialogProps {
  opened: boolean;
  onClose: () => void;
  onJoin: (joinCode: string) => void;
}

export default function JoinWorkspaceDialog({
  opened,
  onClose,
  onJoin,
}: JoinWorkspaceDialogProps) {
  const [joinCode, setJoinCode] = useState("");
  const [error, setError] = useState("");

  const handleJoin = () => {
    if (!joinCode.trim()) {
      setError("Join code cannot be empty");
      return;
    }
    onJoin(joinCode.trim());
    setJoinCode("");
    setError("");
    onClose();
  };

  const handleChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    setJoinCode(event.currentTarget.value);
    if (error) setError("");
  };

  const handleClose = () => {
    setJoinCode("");
    setError("");
    onClose();
  };

  return (
    <Modal
      opened={opened}
      onClose={handleClose}
      title="Join Workspace"
      size="sm"
    >
      <Stack gap="md">
        <TextInput
          label="Join Code"
          placeholder="Enter workspace join code"
          value={joinCode}
          onChange={handleChange}
          error={error}
          data-autofocus
        />

        <Group mt="md">
          <Button onClick={handleJoin} disabled={!joinCode.trim()}>
            Join Workspace
          </Button>
        </Group>
      </Stack>
    </Modal>
  );
}

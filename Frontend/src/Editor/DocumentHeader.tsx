import { useState } from "react";
import {
  Text,
  Title,
  Group,
  ActionIcon,
  TextInput,
  Select,
  Button,
  Stack,
  Tooltip,
  Badge,
  Paper,
} from "@mantine/core";
import {
  Clock,
  Edit,
  Check,
  X,
  Plus,
  History,
  Wifi,
  WifiOff,
} from "lucide-react";
import { EditorProps } from "./Editor";

type ConnectionStatus = "connected" | "connecting" | "disconnected";

interface DocumentHeaderProps extends EditorProps {
  connectionStatus: ConnectionStatus;
}

const getStatusConfig = (status: ConnectionStatus) => {
  switch (status) {
    case "connected":
      return { color: "green", icon: Wifi, text: "Connected" };
    case "connecting":
      return { color: "yellow", icon: Wifi, text: "Connecting..." };
    case "disconnected":
      return { color: "red", icon: WifiOff, text: "Disconnected" };
  }
};

const DocumentHeader = ({
  documentName,
  createdAt,
  workspaceId,
  documentId,
  ownerUsername,
  snapshots,
  connectionStatus,
}: DocumentHeaderProps) => {
  const [isEditing, setIsEditing] = useState(false);
  const [editedName, setEditedName] = useState(documentName);

  const snapshotOptions = Object.entries(snapshots).map(
    ([key, date], index) => ({
      value: key,
      label: `Snapshot ${index + 1} (${new Date(date).toLocaleString()})`,
    })
  );

  const handleNameSave = () => {
    console.log("Saving document name:", editedName);
    setIsEditing(false);
  };

  const handleCreateSnapshot = () => {
    console.log("Creating new snapshot for document:", documentId);
  };

  const statusConfig = getStatusConfig(connectionStatus);

  return (
    <Paper shadow="sm" p="md" style={{ backgroundColor: "#f3f3f3" }} withBorder>
      <Group justify="space-between" align="start">
        <Stack gap="xs">
          {isEditing ? (
            <Group>
              <TextInput
                value={editedName}
                onChange={(e) => setEditedName(e.currentTarget.value)}
                size="lg"
                style={{ width: "400px" }}
                placeholder="Enter document name..."
                autoFocus
              />
              <ActionIcon
                variant="filled"
                color="green"
                onClick={handleNameSave}
                size="lg"
              >
                <Check size={20} />
              </ActionIcon>
              <ActionIcon
                variant="filled"
                color="red"
                onClick={() => {
                  setIsEditing(false);
                  setEditedName(documentName);
                }}
                size="lg"
              >
                <X size={20} />
              </ActionIcon>
            </Group>
          ) : (
            <Group gap="xs">
              <Title order={2} style={{ color: "#343a40" }}>
                {documentName}
              </Title>
              <ActionIcon
                variant="subtle"
                onClick={() => setIsEditing(true)}
                size="lg"
              >
                <Edit size={20} />
              </ActionIcon>
            </Group>
          )}

          <Stack gap="xs">
            <Group gap="xs">
              <Clock size={16} />
              <Text size="sm" c="dimmed">
                Created {new Date(createdAt).toLocaleString()}
              </Text>
            </Group>
            <Group gap="xs">
              <statusConfig.icon
                size={16}
                color={`var(--mantine-color-${statusConfig.color}-6)`}
              />
              <Text
                size="sm"
                c={statusConfig.color}
                style={{ fontWeight: 500 }}
              >
                {statusConfig.text}
              </Text>
            </Group>
          </Stack>
        </Stack>

        <Stack gap="sm">
          <Select
            leftSection={<History size={16} />}
            label="Version History"
            data={snapshotOptions}
            value={snapshotOptions[0].value}
            clearable
            w={320}
            styles={(theme) => ({
              input: {
                "&:focus": {
                  borderColor: theme.colors.blue[6],
                },
              },
              label: {
                fontWeight: 500,
                marginBottom: 4,
              },
            })}
          />
          <Button
            leftSection={<Plus size={20} />}
            variant="light"
            onClick={handleCreateSnapshot}
            color="blue"
            fullWidth
          >
            Create Snapshot
          </Button>
        </Stack>

        <Stack gap="xs" align="end">
          <Tooltip label="Workspace ID (read-only)" position="left">
            <Badge
              variant="dot"
              color="blue"
              size="lg"
              style={{ width: "fit-content" }}
            >
              {workspaceId}
            </Badge>
          </Tooltip>
          <Tooltip label="Document ID (read-only)" position="left">
            <Badge
              variant="dot"
              color="gray"
              size="lg"
              style={{ width: "fit-content" }}
            >
              {documentId}
            </Badge>
          </Tooltip>
          <Badge
            variant="light"
            color="green"
            size="lg"
            style={{ width: "fit-content" }}
          >
            Owner: {ownerUsername}
          </Badge>
        </Stack>
      </Group>
    </Paper>
  );
};

export default DocumentHeader;

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
  Collapse,
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
  ChevronDown,
  ChevronUp,
  GitFork,
} from "lucide-react";
import { Document } from "../../app/models/Document";
import { useStore } from "../../app/stores/store";
import { useDisclosure } from "@mantine/hooks";
import ForkSnapshotDialog from "../ControlPanel/Dialogs/ForkSnapshotDialog";

type ConnectionStatus = "connected" | "connecting" | "disconnected";

interface DocumentHeaderProps {
  document: Document;
  connectionStatus: ConnectionStatus;
  selectSnapshot: React.Dispatch<React.SetStateAction<string>>;
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
  document,
  connectionStatus,
  selectSnapshot,
}: DocumentHeaderProps) => {
  const [isEditing, setIsEditing] = useState(false);
  const [editedName, setEditedName] = useState(document.documentName);
  const [isCollapsed, setIsCollapsed] = useState(false);
  const [selectedSnapshot, setSelectedSnapshot] = useState(
    document.snapshotIds[0]?.name || ""
  );
  const { documentStore, userStore } = useStore();

  const [
    forkSnapshotDialogOpened,
    { toggle: toggleForkSnapshotDialog, close: closeForkSnapshotDialog },
  ] = useDisclosure(false);

  const snapshotOptions = document.snapshotIds.map((snapshot) => ({
    value: snapshot.name,
    label: `${snapshot.name} (${new Date(
      snapshot.createdAt
    ).toLocaleString()})`,
  }));

  const handleNameSave = () => {
    documentStore.changeDocumentName(
      document.workspaceId,
      document.documentId,
      editedName
    );
    setIsEditing(false);
  };

  const handleCreateSnapshot = () => {
    documentStore.createSnapshot(document.workspaceId, document.documentId);
  };

  const handleForkSnapshot = () => {
    toggleForkSnapshotDialog();
  };

  const handleSnapshotChange = (value: string) => {
    setSelectedSnapshot(value);
    selectSnapshot((_) => value);
  };

  const statusConfig = getStatusConfig(connectionStatus);

  return (
    <>
      <Paper
        shadow="sm"
        p="md"
        style={{ backgroundColor: "#f3f3f3" }}
        withBorder
      >
        <Group
          justify="space-between"
          align="center"
          mb={isCollapsed ? 0 : "md"}
        >
          <Group gap="xs">
            <Title order={2} style={{ color: "#343a40" }}>
              {document.documentName}
            </Title>
            {!isCollapsed && (
              <ActionIcon
                variant="subtle"
                onClick={() => setIsEditing(true)}
                size="lg"
              >
                <Edit size={20} />
              </ActionIcon>
            )}
          </Group>
          <ActionIcon
            variant="subtle"
            onClick={() => setIsCollapsed(!isCollapsed)}
            size="lg"
          >
            {isCollapsed ? <ChevronDown size={20} /> : <ChevronUp size={20} />}
          </ActionIcon>
        </Group>

        <Collapse in={!isCollapsed}>
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
                      setEditedName(document.documentName);
                    }}
                    size="lg"
                  >
                    <X size={20} />
                  </ActionIcon>
                </Group>
              ) : null}

              <Stack gap="xs">
                <Group gap="xs">
                  <Clock size={16} />
                  <Text size="sm" c="dimmed">
                    Created {new Date(document.createdAt).toLocaleString()}
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
                value={selectedSnapshot}
                onChange={(v, _) => handleSnapshotChange(v!)}
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
              <Group>
                <Button
                  leftSection={<Plus size={20} />}
                  variant="light"
                  onClick={handleCreateSnapshot}
                  color="blue"
                >
                  Create Snapshot
                </Button>
                <Button
                  leftSection={<GitFork size={20} />}
                  variant="light"
                  color="blue"
                  onClick={handleForkSnapshot}
                >
                  Fork snapshot
                </Button>
              </Group>
            </Stack>

            <Stack gap="xs" align="end">
              <Tooltip label="Workspace ID (read-only)" position="left">
                <Badge
                  variant="dot"
                  color="blue"
                  size="lg"
                  style={{ width: "fit-content" }}
                >
                  {document.workspaceId}
                </Badge>
              </Tooltip>
              <Tooltip label="Document ID (read-only)" position="left">
                <Badge
                  variant="dot"
                  color="gray"
                  size="lg"
                  style={{ width: "fit-content" }}
                >
                  {document.documentId}
                </Badge>
              </Tooltip>
              <Badge
                variant="light"
                color="green"
                size="lg"
                style={{ width: "fit-content" }}
              >
                Owner: {document.creatorUsername}
              </Badge>
            </Stack>
          </Group>
        </Collapse>
      </Paper>
      <ForkSnapshotDialog
        opened={forkSnapshotDialogOpened}
        onClose={closeForkSnapshotDialog}
        onFork={(docName: string) => {
          documentStore.forkSnapshot(
            document.workspaceId,
            document.documentId,
            docName,
            selectedSnapshot,
            userStore.user?.username ?? "InvalidUser"
          );
        }}
      />
    </>
  );
};

export default DocumentHeader;

import {
  IconPlus,
  IconLink,
  IconPencil,
  IconUsers,
  IconTrash,
  IconRefresh,
} from "@tabler/icons-react";

export const workspaceContextMenu = [
  {
    key: "new",
    icon: <IconPlus size={16} />,
    title: "New Document",
    onClick: () => console.log("New document"),
  },
  {
    key: "invite",
    icon: <IconLink size={16} />,
    title: "Invite Code",
    onClick: () => console.log("Invite link"),
  },
  {
    key: "Refresh",
    icon: <IconRefresh size={16} />,
    title: "Refresh",
    onClick: () => console.log("Refresh"),
  },
  {
    key: "rename",
    icon: <IconPencil size={16} />,
    title: "Change Name",
    onClick: () => console.log("Renaming workspace"),
  },
  {
    key: "users",
    icon: <IconUsers size={16} />,
    title: "Manage Users",
    onClick: () => console.log("Manage users"),
  },
  {
    key: "delete",
    icon: <IconTrash size={16} />,
    title: "Delete",
    style: { color: "red" },
    onClick: () => console.log("delete workspace"),
  },
];

export const documentContextMenu = () => [
  {
    key: "delete",
    icon: <IconTrash size={16} />,
    title: "Delete",
    color: "red",
    onClick: () => console.log("delete document"),
  },
];

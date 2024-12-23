import { IconTrash } from "@tabler/icons-react";
import { PermissionLevel } from "../../app/models/Workspace";

export const documentContextMenu = () => [
  {
    key: "delete",
    icon: <IconTrash size={16} />,
    title: "Delete",
    color: "red",
    onClick: () => console.log("delete document"),
    requiredPermission: PermissionLevel.Admin,
  },
];
